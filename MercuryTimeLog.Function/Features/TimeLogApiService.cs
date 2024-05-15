using MercuryTimeLog.Function.Features.Models;
using MercuryTimeLog.Function.Services;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using RestSharp;
using System.Xml.Linq;
using MercuryTimeLog.Domain.Common;
using MercuryTimeLog.Domain.Entities;

namespace MercuryTimeLog.Function.Features;

public interface ITimeLogApiService
{
    Task PostProjectContractsAsync();
    Task GetProjectExternalByProjectAsync();

    Task<IEnumerable<ProjectExpense>> GetProjectExpenseAsync(int projectId);

    Task SaveProjectExpenseAsync(ProjectExpenseCommand command);
}

public class TimeLogApiService : ITimeLogApiService
{
    private int count = 1;
    private List<int> list = new List<int>();
    private const string baseURL = "https://app5.timelog.com/mercurisandbox/api/v1/";
    private const string PatToken = "49066ACD09FD0662E89B287CB345028FA6009BB22C37BC2DA570ED83E3EC0AA3-1";
    private readonly ITableStorageHelperService _dataService;
    public TimeLogApiService(ITableStorageHelperService dataService)
    {
        _dataService = dataService;
    }

    public async Task GetProjectExternalByProjectAsync()
    {
        int pageCount = 0;
        int page = 1;
        do
        {
            var baseURL = "https://app5.timelog.com/mercurisandbox/api/v1/project/get-all?version=1";
            var client = new RestClient(baseURL);
            var request = new RestRequest();
            request.Method = Method.Get;
            request.AddHeader("Content-Type", "application/json");
            request.AddHeader("Authorization", "Bearer 49066ACD09FD0662E89B287CB345028FA6009BB22C37BC2DA570ED83E3EC0AA3-1");
            request.AddParameter("version", "1");
            request.AddParameter("page", $"{page}");
            request.AddParameter("pageSize", "10");
            var response = await client.ExecuteAsync(request);
            var projectsJsonData = JsonConvert.DeserializeObject(response.Content!)!.ToString();
            var jsonObj = JObject.Parse(projectsJsonData!);
            var entities = jsonObj["Entities"];
            var records = Convert.ToInt32(jsonObj["Properties"]["TotalRecord"].ToString());
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
                        var projectId = Convert.ToInt32(obj["ProjectID"]!.ToString());
                        if (!list.Contains(projectId))
                        {
                            await GetProjectExpenseAsync(projectId);
                        }
                    }
                }
            }

            pageCount = records - (page*10);
            page++;
        } while (page < pageCount);
    }

    public async Task SaveProjectExpenseAsync(ProjectExpenseCommand command)
    {
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

            await _dataService.SaveProjectExpenseAsync(new List<ProjectExpense> { expense });
        }
    }

    public async Task<IEnumerable<ProjectExpense>> GetProjectExpenseAsync(int projectId)
    {
        var expenses = new List<ProjectExpense>();
        var url = $"{baseURL}project-expense?version=1";
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
                        var dateTime = DateTime.SpecifyKind(DateTime.UtcNow, DateTimeKind.Utc);

                        var obj = JObject.Parse(value.ToString());
                        var expense = JsonConvert.DeserializeObject<ProjectExpense>(obj.ToString());
                        expense!.Id = Guid.NewGuid();
                        expense.PartitionKey = "ProjectExpenseKey";
                        expense.RowKey = expense!.Id.ToString();
                        expense.Timestamp = dateTime;
                        expense.CreatedOn = dateTime;
                        expense.ModifiedOn = dateTime;
                        expense.PurchaseDate = DateTime.SpecifyKind(expense.PurchaseDate, DateTimeKind.Utc);
                        expenses.Add(expense!);
                    }
                }
            }
        }
        else
        {
            Console.WriteLine($"{projectId} is not found");
        }

        return expenses;
    }

    private async Task GetProjectExternalAsync(int projectId)
    {
        var baseURL = $"https://app5.timelog.com/mercurisandbox/api/v1/project/{projectId}/external-keys?version=1";
        var client = new RestClient(baseURL);
        var request = new RestRequest();
        request.Method = Method.Get;
        request.AddHeader("Content-Type", "application/json");
        request.AddHeader("Authorization", "Bearer 49066ACD09FD0662E89B287CB345028FA6009BB22C37BC2DA570ED83E3EC0AA3-1");
        request.AddParameter("projectID", $"{projectId}");
        request.AddParameter("version", "1");
        var response = await client.ExecuteAsync(request);
        var externalJsonData = JsonConvert.DeserializeObject(response.Content!)!.ToString();
        var externalJsonObj = JObject.Parse(externalJsonData!);
        if (response.StatusCode == System.Net.HttpStatusCode.OK)
        {
            var totalRecord = Convert.ToInt32(externalJsonObj["Properties"]!["TotalRecord"]!.ToString());
            if (totalRecord > 0)
            {
                var data = externalJsonData;
            }
            Console.WriteLine($"{projectId} is ok");
        }else
        {
            Console.WriteLine($"{projectId} is not ok");
        }
        Console.WriteLine($"Project count: {count}");
        count++;
        list.Add(projectId);
    }

    public async Task PostProjectContractsAsync()
    {
        /*string baseURL = "https://app5.timelog.com/mercurisandbox_specifications/service.asmx";
        string basePath = Directory.GetParent(@"./")!.FullName;
        XNamespace instanceSpace = (XNamespace)"http://www.w3.org/2001/XMLSchema-instance";
        XNamespace schemaSpace = (XNamespace)"http://www.w3.org/2001/XMLSchema";
        XNamespace envelopeSpace = (XNamespace)"http://schemas.xmlsoap.org/soap/envelope/";
        XNamespace extendRawSpace = (XNamespace)"http://www.timelog.com/ws/tlp/v4_4";

        var _path = Path.Combine(basePath, "Soap\\TimeLog.xml");
        var doc = XDocument.Load(_path);
        //var root = doc.Elements(instanceSpace + "Envelope");
        //var rootChild = root.Descendants();

        // doc.Save(_path);
        var client = new RestClient(baseURL);
        var request = new RestRequest();
        request.AddHeader("Content-Type", "text/xml");
        request.AddHeader("SOAPaction", "http://www.timelog.com/ws/tlp/v4_4/GetContractsExtendedRaw");
        request.AddParameter("text/xml", doc.ToString(), ParameterType.RequestBody);
        var response = await client.ExecutePostAsync(request);
        var xmlToJson = JsonService.XmlToJSON(response.Content!).ToString();
        var jsonData = JsonConvert.DeserializeObject(xmlToJson)!.ToString();
        var jsonObj = JObject.Parse(jsonData!.ToString()!);
        var projectContractsObj = jsonObj["soap:Envelope"]["soap:Body"]["GetContractsExtendedRawResponse"]["GetContractsExtendedRawResult"]["tlp:ProjectContracts"]["tlp:ProjectContract"];
        var projectContractsDto = JsonConvert.DeserializeObject<IEnumerable<ProjectContractDTO>>(projectContractsObj.ToString())!;
        var projectContracts = projectContractsDto.Select(i => new ProjectContract
        {
            Id = i.UniqueId,
            ProjectContractId = i.Id,
            ProjectID = i.ProjectID,
            Name = i.Name,
            ProjectContractStatus = i.ProjectContractStatus,
            ContractModelType = i.ContractModelType,
            ContractTypeID = i.ContractTypeID,
            ContractTypeName = i.ContractTypeName,
            CurrencyID = i.CurrencyID,
            CurrencyISO = i.CurrencyISO,
            CustomerID = i.CustomerID,
            ExpenseBudgetAmount = i.ExpenseBudgetAmount,
            ExpenseRevenueShareAmount = i.ExpenseRevenueShareAmount,
            ExternalWorkRevenueShareAmount = i.ExternalWorkRevenueShareAmount,
            GoodsRevenueShareAmount = i.GoodsRevenueShareAmount,
            PaymentAmount = i.PaymentAmount,
            RiskRevenueShareAmount = i.RiskRevenueShareAmount,
            TravelBudgetAmount = i.TravelBudgetAmount,
            TravelRevenueShareAmount = i.TravelRevenueShareAmount,
            WorkBudgetHours = i.WorkBudgetHours,
            WorkRevenueShareAmount = i.WorkRevenueShareAmount,
            LastModified = i.LastModified
        }).ToList();*/
    }
}
