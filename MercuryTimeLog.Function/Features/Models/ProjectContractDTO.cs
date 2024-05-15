using Newtonsoft.Json;

namespace MercuryTimeLog.Function.Features.Models;
public record ProjectContractDTO
{
    [JsonProperty("GUID")]
    public Guid UniqueId { get; set; }

    [JsonProperty("ID")]
    public int Id { get; set; }

    [JsonProperty("tlp:ContractModelType")]
    public int ContractModelType { get; set; }

    [JsonProperty("tlp:ContractTypeID")]
    public int ContractTypeID { get; set; }

    [JsonProperty("tlp:ContractTypeName")]
    public string? ContractTypeName { get; set; }

    [JsonProperty("tlp:CurrencyID")]
    public int CurrencyID { get; set; }

    [JsonProperty("tlp:CurrencyISO")]
    public string? CurrencyISO { get; set; }

    [JsonProperty("tlp:CustomerID")]
    public int CustomerID { get; set; }

    [JsonProperty("tlp:ExpenseBudgetAmount")]
    public decimal ExpenseBudgetAmount { get; set; }

    [JsonProperty("tlp:ExpenseRevenueShareAmount")]
    public decimal ExpenseRevenueShareAmount { get; set; }

    [JsonProperty("tlp:ExternalWorkRevenueShareAmount")]
    public decimal ExternalWorkRevenueShareAmount { get; set; }

    [JsonProperty("tlp:GoodsRevenueShareAmount")]
    public decimal GoodsRevenueShareAmount { get; set; }

    [JsonProperty("tlp:LastModified")]
    public DateTime? LastModified { get; set; }

    [JsonProperty("tlp:Name")]
    public string? Name { get; set; }

    [JsonProperty("tlp:PaymentAmount")]
    public decimal PaymentAmount { get; set; }

    [JsonProperty("tlp:ProjectContractStatus")]
    public int ProjectContractStatus { get; set; }

    [JsonProperty("tlp:ProjectID")]
    public int ProjectID { get; set; }

    [JsonProperty("tlp:RiskRevenueShareAmount")]
    public decimal RiskRevenueShareAmount { get; set; }

    [JsonProperty("tlp:TravelBudgetAmount")]
    public decimal TravelBudgetAmount { get; set; }

    [JsonProperty("tlp:TravelRevenueShareAmount")]
    public decimal TravelRevenueShareAmount { get; set; }

    [JsonProperty("tlp:WorkBudgetHours")]
    public decimal WorkBudgetHours { get; set; }

    [JsonProperty("tlp:WorkRevenueShareAmount")]
    public decimal WorkRevenueShareAmount { get; set; }

}