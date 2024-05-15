using Azure;
using Azure.Data.Tables;

namespace MercuryTimeLog.Domain.Entities;

public class ProjectContract : ITableEntity
{
    public Guid Id { get; set; }
    public int ProjectContractId { get; set; }

    public int ContractModelType { get; set; }

    public int ContractTypeID { get; set; }

    public string? ContractTypeName { get; set; }

    public int CurrencyID { get; set; }

    public string? CurrencyISO { get; set; }

    public int CustomerID { get; set; }

    public decimal ExpenseBudgetAmount { get; set; }

    public decimal ExpenseRevenueShareAmount { get; set; }

    public decimal ExternalWorkRevenueShareAmount { get; set; }

    public decimal GoodsRevenueShareAmount { get; set; }

    public DateTime? LastModified { get; set; }

    public string? Name { get; set; }

    public decimal PaymentAmount { get; set; }

    public int ProjectContractStatus { get; set; }

    public int ProjectID { get; set; }

    public decimal RiskRevenueShareAmount { get; set; }

    public decimal TravelBudgetAmount { get; set; }

    public decimal TravelRevenueShareAmount { get; set; }

    public decimal WorkBudgetHours { get; set; }

    public decimal WorkRevenueShareAmount { get; set; }

    public DateTime CreatedOn { get; set; }
    public DateTime? ModifiedOn { get; set; }
    public string? CreatedBy { get; set; }
    public string? ModifiedBy { get; set; }
    public required string PartitionKey { get; set; }
    public required string RowKey { get; set; }
    public DateTimeOffset? Timestamp { get; set; }
    public ETag ETag { get; set; }
}
