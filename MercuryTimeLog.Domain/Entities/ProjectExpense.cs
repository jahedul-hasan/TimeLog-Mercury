using Azure;
using Azure.Data.Tables;

namespace MercuryTimeLog.Domain.Entities;

public class ProjectExpense : ITableEntity
{
    public Guid Id { get; set; }
    public int ProjectExpenseID { get; set; }
    public string? ExpenseNo { get; set; }
    public int ProjectID { get; set; }
    public int EmployeeID { get; set; }
    public DateTime PurchaseDate { get; set; }
    public string? InvoiceNo { get; set; }
    public int CustomerID { get; set; }
    public string? ExpenseCurrencyISO { get; set; }
    public double ExpenseDirectProjectExchangeRate { get; set; }
    public double AmountExpenseCurrency { get; set; }
    public double VatExpenseCurrency { get; set; }
    public string? ProjectCurrencyISO { get; set; }
    public double SalesPriceProjectCurrency { get; set; }
    public double SalesPriceSystemCurrency { get; set; }
    public double SalesPriceProjectExchangeRate { get; set; }
    public double ExpenseCostProjectCurrency { get; set; }
    public double ExpenseCostSystemCurrency { get; set; }
    public double ProfitRatio { get; set; }
    public double InvoicedAmountSystemCurrency { get; set; }
    public double InvoicedAmountProjectCurrency { get; set; }
    public string? Comment { get; set; }
    public int PaymentMethodID { get; set; }
    public int ExpenseTypeID { get; set; }
    public bool IsBillable { get; set; }
    public bool IsApproved { get; set; }
    public int ApprovedBy { get; set; }
    public int InvoiceLineID { get; set; }
    public int UnitType { get; set; }
    public string? ProductNo { get; set; }
    public string? DisbursementGuid { get; set; }
    public string? ExternalID { get; set; }
    public int ProjectSubContractID { get; set; }
    public int InstallmentID { get; set; }
    public int LegalEntityID { get; set; }
    public DateTime CreatedOn { get; set; }
    public DateTime? ModifiedOn { get; set; }
    public string? CreatedBy { get; set; }
    public string? ModifiedBy { get; set; }
    public required string PartitionKey { get; set; }
    public required string RowKey { get; set; }
    public DateTimeOffset? Timestamp { get; set; }
    public ETag ETag { get; set; }
}
