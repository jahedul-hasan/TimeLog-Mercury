namespace MercuryTimeLog.Function.Features.Models;

public record ProjectExpenseCommand
{
    public string? Comment { get; init; }
    public int ProjectID { get; init; }
    public DateTime PurchaseDate { get; init; }
    public int PaymentMethodID { get; init; }
    public int ExpenseTypeID { get; init; }
    public double AmountIncludingVatExpenseCurrency { get; init; }
    public double SalesPriceAmountProjectCurrency { get; init; }
    public bool IsBillable { get; init; }
    public string? ExpenseNo { get; init; }
    public double VatAmountExpenseCurrency { get; init; }
    public string? ExpenseCurrencyISO { get; init; }
    public string? ExternalID { get; init; }
    public int ProjectSubContractID { get; init; }
    public double ProjectExpenseExchangeRate { get; init; }
    public int SupplierID { get; init; }
    public string? SupplierInvoiceNo { get; init; }
    public double ProfitRatio { get; init; }
}
