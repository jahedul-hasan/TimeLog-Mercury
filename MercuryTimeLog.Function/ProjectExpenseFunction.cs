using Azure.Data.Tables;
using MercuryTimeLog.Domain.Entities;
using MercuryTimeLog.Function.Features.Models;
using Microsoft.Azure.Functions.Worker;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json;
using RestSharp;

namespace MercuryTimeLog.Function;

public static class ProjectExpenseFunction
{
    private const string baseURL = "https://app5.timelog.com/mercurisandbox/api/v1/";
    private const string PatToken = "49066ACD09FD0662E89B287CB345028FA6009BB22C37BC2DA570ED83E3EC0AA3-1";

    [Function("StartProjectExpenseSync")]
    public static async Task StartProjectExpenseSync([TimerTrigger("*/1 * * * *")] TimerInfo myTimer)
     {
        var projectContracts = await GetProjectContractsAsync();

        await SaveProjectExpenseAsync(projectContracts.ToList());
    }

    private static async Task SaveProjectExpenseAsync(IList<ProjectContract> projectContracts)
    {
        var projectExpenses = new List<ProjectExpense>();

        foreach (var projectContract in projectContracts)
        {
            var projectSubContractId = await GetProjectSubContractIdAsync(projectContract.ProjectID);

            if (projectSubContractId <= 0) break;

            var command = new ProjectExpenseCommand
            {
                AmountIncludingVatExpenseCurrency = 200,
                Comment = projectContract.Name,
                ExpenseCurrencyISO = projectContract.CurrencyISO,
                ExpenseNo = "1",
                ExpenseTypeID = 1005,//static temp
                ExternalID = "",
                IsBillable = false,
                PaymentMethodID = 161,//static temp
                ProfitRatio = 0,
                ProjectExpenseExchangeRate = 2,
                ProjectID = projectContract.ProjectID,
                ProjectSubContractID = projectSubContractId,
                PurchaseDate = DateTime.UtcNow,
                SalesPriceAmountProjectCurrency = 100,
                SupplierID = 0,
                SupplierInvoiceNo = "",
                VatAmountExpenseCurrency = 1
            };
            var serialize = JsonConvert.SerializeObject(command);
            var dateTime = DateTime.SpecifyKind(DateTime.UtcNow, DateTimeKind.Utc);
            var url = $"{baseURL}project-expense?version=1";
            var client = new RestClient(url);
            var request = new RestRequest();
            request.AddHeader("Content-Type", "application/json");
            request.AddHeader("Authorization", $"Bearer {PatToken}");
            request.AddParameter("application/json", serialize, ParameterType.RequestBody);
            var response = await client.ExecutePostAsync(request);
            if (response.StatusCode == System.Net.HttpStatusCode.OK)
            {
                var jsonData = JsonConvert.DeserializeObject(response.Content!)!.ToString();
                var jsonObj = JObject.Parse(jsonData!);
                var expenseObj = JObject.Parse(jsonObj["Properties"]!.ToString());
                var expense = JsonConvert.DeserializeObject<ProjectExpense>(expenseObj.ToString());
                expense!.Id = Guid.NewGuid();
                expense.PartitionKey = "ProjectExpenseKey";
                expense.RowKey = expense!.Id.ToString();
                expense.Timestamp = dateTime;
                expense.CreatedOn = dateTime;
                expense.ModifiedOn = dateTime;
                expense.PurchaseDate = DateTime.SpecifyKind(expense.PurchaseDate, DateTimeKind.Utc);
                projectExpenses.Add(expense);
            }
        }

        await _SaveProjectExpenseAsync(projectExpenses);
    }

    private static async Task<int> GetProjectSubContractIdAsync(int projectId)
    {
        var url = $"{baseURL}contract?version=1";
        var client = new RestClient(url);
        var request = new RestRequest();
        request.Method = Method.Get;
        request.AddHeader("Content-Type", "application/json");
        request.AddHeader("Authorization", $"Bearer {PatToken}");
        request.AddParameter("projectID", $"{projectId}");
        request.AddParameter("version", "1");
        var response = await client.ExecuteAsync(request);
        var jsonData = JsonConvert.DeserializeObject(response.Content!)!.ToString();
        var jsonObj = JObject.Parse(jsonData!);
        if (response.StatusCode == System.Net.HttpStatusCode.OK)
        {
            var entities = jsonObj["Entities"];
            var entityArray = JArray.Parse(entities!.ToString());
            foreach (JObject entity in entityArray)
            {
                foreach (var property in entity.Properties())
                {
                    var name = property.Name;
                    var value = property.Value;
                    if (name.Equals("Properties"))
                    {
                        var obj = JObject.Parse(value.ToString());

                        if (Convert.ToBoolean(obj["IsMileageBillable"]!.ToString()))
                        {
                            return Convert.ToInt32(obj["ContractID"]!.ToString());
                        }
                    }
                }
            }
        }
        else
        {
            Console.WriteLine($"{projectId} is not found");
        }

        return int.MinValue;
    }

    private static async Task<IEnumerable<ProjectContract>> GetProjectContractsAsync()
    {
        string connectionString = Environment.GetEnvironmentVariable("ConnectionString")!;

        string tableName = "ProjectContract";

        var tableServiceclient = new TableServiceClient(connectionString);

        var tableClient = tableServiceclient.GetTableClient(tableName);

        await tableClient.CreateIfNotExistsAsync();

        var projectContracts = tableClient.Query<ProjectContract>().ToList();

        return projectContracts;
    }

    private static async Task _SaveProjectExpenseAsync(IList<ProjectExpense> projectExpenses)
    {
        string connectionString = Environment.GetEnvironmentVariable("ConnectionString")!;

        string tableName = "ProjectExpense";

        var tableServiceClient = new TableServiceClient(connectionString);

        var tableClient = tableServiceClient.GetTableClient(tableName);

        await tableClient.CreateIfNotExistsAsync();

        var insertTasks = new List<Task>();

        foreach (var entity in projectExpenses)
        {
            insertTasks.Add(tableClient.UpsertEntityAsync(entity));
        }

        await Task.WhenAll(insertTasks);

        await Task.CompletedTask;
    }
}
