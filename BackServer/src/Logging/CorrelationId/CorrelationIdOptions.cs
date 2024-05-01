public class CorrelationIdOptions
{
    private const string defaultHeader = "X-Correlation-Id";

    public bool Enabled { get; set; } = true;
    public string Header { get; set; } = defaultHeader;
    public string PropertyName { get; set; } = "CorrelationId";
}