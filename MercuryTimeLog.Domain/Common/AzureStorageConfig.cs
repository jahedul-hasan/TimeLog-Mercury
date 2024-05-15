namespace MercuryTimeLog.Domain.Common;

public record AzureStorageConfig
{
    public string? AccountName { get; set; }
    public string? AccountKey { get; set; }
    public string? ConnectionString { get; set; }
    public string? ContainerName { get; set; }
}
