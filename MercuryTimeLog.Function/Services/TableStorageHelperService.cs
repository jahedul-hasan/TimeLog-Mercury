using Azure.Data.Tables;
using MercuryTimeLog.Domain.Common;
using MercuryTimeLog.Domain.Entities;

namespace MercuryTimeLog.Function.Services;

public interface ITableStorageHelperService
{
    Task SaveProjectExpenseAsync(IList<ProjectExpense> projectExpenses);
}

public class TableStorageHelperService : ITableStorageHelperService
{
    private readonly AzureStorageConfig _config;
    public TableStorageHelperService(AzureStorageConfig config)
    {
        _config = config;
    }

    public async Task SaveProjectExpenseAsync(IList<ProjectExpense> projectExpenses)
    {
        string tableName = "ProjectExpense";

        var tableServiceClient = new TableServiceClient(_config.ConnectionString);

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
