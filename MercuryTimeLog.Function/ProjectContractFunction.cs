using MercuryTimeLog.Domain.Common;
using MercuryTimeLog.Domain.Entities;
using MercuryTimeLog.Function.Features.Models;
using Microsoft.Azure.Functions.Worker;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json;
using RestSharp;
using System.Xml.Linq;
using Azure.Data.Tables;

namespace MercuryTimeLog.Function;

public static class ProjectContractFunction
{
    [Function("StartProjectContractsSync")]
    public static async Task StartProjectContractsSync([TimerTrigger("*/1 * * * *")] TimerInfo myTimer)
    {
        var projectContracts = await PostProjectContractsAsync();

        await SaveProjectContractsAsync(projectContracts.ToList());
    }

    private static async Task<IEnumerable<ProjectContract>> PostProjectContractsAsync()
    {
        string baseURL = "https://app5.timelog.com/mercurisandbox_specifications/service.asmx";
        string basePath = Directory.GetParent(@"./")!.FullName;
        XNamespace instanceSpace = (XNamespace)"http://www.w3.org/2001/XMLSchema-instance";
        XNamespace schemaSpace = (XNamespace)"http://www.w3.org/2001/XMLSchema";
        XNamespace envelopeSpace = (XNamespace)"http://schemas.xmlsoap.org/soap/envelope/";
        XNamespace extendRawSpace = (XNamespace)"http://www.timelog.com/ws/tlp/v4_4";

        var previousDay = (DateTime.Now.AddDays(-1)).Date.ToString("yyyy-MM-dd");

        var doc = $@"<?xml version=""1.0"" encoding=""utf-8""?>
        <soap:Envelope xmlns:xsi=""http://www.w3.org/2001/XMLSchema-instance"" xmlns:xsd=""http://www.w3.org/2001/XMLSchema"" xmlns:soap=""http://schemas.xmlsoap.org/soap/envelope/"">
	        <soap:Body>
		        <GetContractsExtendedRaw xmlns=""http://www.timelog.com/ws/tlp/v4_4"">
			        <siteCode>ad7330a8d9d741afb8e1f138f90250d2</siteCode>
			        <apiID>TimeLog</apiID>
			        <apiPassword>Mercuri1960</apiPassword>
			        <customerID>0</customerID>
			        <projectID>0</projectID>
			        <contractModelType>0</contractModelType>
			        <lastModifiedSince>2024-04-02T12:19:47.003</lastModifiedSince>
		        </GetContractsExtendedRaw>
	        </soap:Body>
        </soap:Envelope>";

        try
        {
            //var doc = XDocument.Load(_path);
            var client = new RestClient(baseURL);
            var request = new RestRequest();
            request.AddHeader("Content-Type", "text/xml");
            request.AddHeader("SOAPaction", "http://www.timelog.com/ws/tlp/v4_4/GetContractsExtendedRaw");
            request.AddParameter("text/xml", doc.ToString(), ParameterType.RequestBody);
            var response = await client.ExecutePostAsync(request);

            if (response.StatusCode == System.Net.HttpStatusCode.OK)
            {
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
                    LastModified = DateTime.SpecifyKind(i.LastModified!.Value, DateTimeKind.Utc),
                    PartitionKey = "ProjectContractKey",
                    RowKey = i.UniqueId.ToString(),
                    Timestamp = DateTime.SpecifyKind(DateTime.UtcNow, DateTimeKind.Utc),
                    CreatedOn = DateTime.SpecifyKind(DateTime.UtcNow, DateTimeKind.Utc),
                    ModifiedOn = DateTime.SpecifyKind(DateTime.UtcNow, DateTimeKind.Utc)
                }).ToList();

                return projectContracts;
            }
        }
        catch (Exception exp)
        {
            Console.WriteLine($"{exp.Message}");
        }
        return Enumerable.Empty<ProjectContract>();
    }

    private static async Task SaveProjectContractsAsync(IList<ProjectContract> projectContracts)
    {
        string connectionString = Environment.GetEnvironmentVariable("ConnectionString")!;

        string tableName = "ProjectContract";

        var tableServiceclient = new TableServiceClient(connectionString);

        var tableClient = tableServiceclient.GetTableClient(tableName);

        await tableClient.CreateIfNotExistsAsync();

        var partitionKey = projectContracts.Select(i => i.PartitionKey).FirstOrDefault();

        var projectContractsNew = new List<ProjectContract>();

        foreach (var projectContract in projectContracts)
        {
            var queries = tableClient.Query<ProjectContract>(x=>x.RowKey.Equals(projectContract.RowKey)).ToList();

            if (!queries.Any())
            {
                projectContractsNew.Add(projectContract);
            }
        }

        var insertTasks = new List<Task>();

        foreach (var entity in projectContractsNew)
        {
            insertTasks.Add(tableClient.UpsertEntityAsync(entity));
        }

        await Task.WhenAll(insertTasks);

        await Task.CompletedTask;
    }
}
