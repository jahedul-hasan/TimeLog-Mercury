namespace MercuryTimeLog.Function.Features.Models;

public record ProjectExpenseDTO
{
    public Guid Id { get; set; }
    public int ProjectExpenseID { get; set; }
    public string? ExpenseNo { get; set; }
    public string? Comment { get; set; }
    public int ProjectID { get; set; }
    public DateTime PurchaseDate { get; set; }
    public int PaymentMethodID { get; set; }
    public int ExpenseTypeID { get; set; }
    public int EmployeeID { get; set; }
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
}
