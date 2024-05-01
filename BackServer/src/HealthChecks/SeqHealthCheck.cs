using Microsoft.Extensions.Diagnostics.HealthChecks;

public class SeqHealthCheck : IHealthCheck
{
    private readonly ILogger _logger;
    private HttpClient _httpClient;

    public SeqHealthCheck(HttpClient httpClient, ILogger<SeqHealthCheck> logger)
    {
        _httpClient = httpClient;
        _logger = logger;
    }

    public async Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken = default)
    {
        try
        {
            using var response = await _httpClient.GetAsync("/health");
            response.EnsureSuccessStatusCode();

            return HealthCheckResult.Healthy();
        }
        catch (Exception ex)
        {
            _logger.LogError($"Seq service is unhealthy {ex.Message}");

            return HealthCheckResult.Unhealthy();
        }
    }
}