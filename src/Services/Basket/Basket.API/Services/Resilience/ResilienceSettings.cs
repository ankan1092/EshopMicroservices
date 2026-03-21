namespace Basket.API.Services.Resilience;

/// <summary>
/// Configuration for resilience policies (retry, circuit breaker, etc.)
/// </summary>
public class ResilienceSettings
{
    public int RetryCount { get; set; } = 3;
    public double InitialDelaySeconds { get; set; } = 1;
    public double BackoffMultiplier { get; set; } = 2;
    public int CircuitBreakerFailureThreshold { get; set; } = 5;
    public int CircuitBreakerTimeoutSeconds { get; set; } = 30;
}

/// <summary>
/// Configuration for specific service resilience policies
/// </summary>
public class ResiliencePoliciesConfig
{
    public ResilienceSettings Discount { get; set; } = new();
    public ResilienceSettings Catalog { get; set; } = new();
    public ResilienceSettings Default { get; set; } = new();
}
