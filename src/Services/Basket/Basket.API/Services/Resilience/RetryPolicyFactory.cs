using Grpc.Core;
using Microsoft.Extensions.Logging;
using Polly;
using Polly.CircuitBreaker;

namespace Basket.API.Services.Resilience;

/// <summary>
/// Factory for creating reusable resilience policies with retry, circuit breaker, and exponential backoff.
/// </summary>
public static class RetryPolicyFactory
{
    /// <summary>
    /// Creates a generic async retry policy with exponential backoff.
    /// </summary>
    /// <typeparam name="TResult">The result type of the policy</typeparam>
    /// <param name="logger">Logger instance for retry events</param>
    /// <param name="retryCount">Number of retry attempts (default: 3)</param>
    /// <param name="initialDelaySeconds">Initial delay in seconds (default: 1)</param>
    /// <param name="backoffMultiplier">Exponential backoff multiplier (default: 2)</param>
    /// <param name="policyName">Name for logging purposes</param>
    /// <returns>Configured async retry policy</returns>
    private static IAsyncPolicy<TResult> CreateRetryPolicy<TResult>(
        ILogger logger,
        int retryCount = 3,
        double initialDelaySeconds = 1,
        double backoffMultiplier = 2,
        double jitterFactorSeconds = 0.5,
        string policyName = "RetryPolicy"
    )
        where TResult : class
    {
        return Policy
            .Handle<Exception>()
            .OrResult<TResult>(r => r == null)
            .WaitAndRetryAsync<TResult>(
                retryCount: retryCount,
                sleepDurationProvider: attempt =>
                {
                    var exponentialDelay =
                        initialDelaySeconds * Math.Pow(backoffMultiplier, attempt - 1);
                    var jitter = Random.Shared.NextDouble() * jitterFactorSeconds;
                    return TimeSpan.FromSeconds(exponentialDelay + jitter);
                },
                onRetry: (outcome, timespan, retryAttempt, context) =>
                {
                    var errorMsg = outcome.Exception?.Message ?? "Request failed";
                    logger.LogWarning(
                        "[{PolicyName}] Retry attempt {RetryAttempt}/{RetryCount} after {DelaySeconds:F1}s due to: {ErrorMessage}",
                        policyName,
                        retryAttempt,
                        retryCount,
                        timespan.TotalSeconds,
                        errorMsg
                    );
                }
            );
    }

    /// <summary>
    /// Creates a gRPC-specific retry policy that handles RpcException and transient failures.
    /// </summary>
    /// <typeparam name="TResult">The result type of the policy</typeparam>
    /// <param name="logger">Logger instance for retry events</param>
    /// <param name="retryCount">Number of retry attempts (default: 3)</param>
    /// <param name="initialDelaySeconds">Initial delay in seconds (default: 1)</param>
    /// <param name="policyName">Name for logging purposes</param>
    /// <returns>Configured async retry policy for gRPC calls</returns>
    private static IAsyncPolicy<TResult> CreateGrpcRetryPolicy<TResult>(
        ILogger logger,
        int retryCount = 3,
        double initialDelaySeconds = 1,
        double backoffMultiplier = 2,
        double jitterFactorSeconds = 0.5,
        string policyName = "GrpcRetryPolicy"
    )
        where TResult : class
    {
        return Policy
            .Handle<RpcException>(ex =>
                // Retry on transient gRPC errors
                ex.StatusCode == StatusCode.Unavailable
                || ex.StatusCode == StatusCode.DeadlineExceeded
                || ex.StatusCode == StatusCode.ResourceExhausted
            )
            .Or<HttpRequestException>()
            .OrResult<TResult>(r => r == null)
            .WaitAndRetryAsync<TResult>(
                retryCount: retryCount,
                sleepDurationProvider: attempt =>
                {
                    var exponentialDelay =
                        initialDelaySeconds * Math.Pow(backoffMultiplier, attempt - 1);
                    var jitter = Random.Shared.NextDouble() * jitterFactorSeconds;
                    return TimeSpan.FromSeconds(exponentialDelay + jitter);
                },
                onRetry: (outcome, timespan, retryAttempt, context) =>
                {
                    var errorMsg = outcome.Exception?.Message ?? "gRPC request failed";
                    var statusCode =
                        (outcome.Exception as RpcException)?.StatusCode.ToString() ?? "Unknown";

                    logger.LogWarning(
                        "[{PolicyName}] Retry attempt {RetryAttempt}/{RetryCount} after {DelaySeconds:F1}s | Status: {StatusCode}",
                        policyName,
                        retryAttempt,
                        retryCount,
                        timespan.TotalSeconds,
                        statusCode
                    );
                }
            );
    }

    /// <summary>
    /// Creates a generic async circuit breaker policy that opens after consecutive failures.
    /// </summary>
    /// <typeparam name="TResult">The result type of the policy</typeparam>
    /// <param name="logger">Logger instance for circuit breaker events</param>
    /// <param name="failureThreshold">Number of consecutive failures before opening (default: 5)</param>
    /// <param name="samplingDurationSeconds">Duration for evaluating failures (default: 30)</param>
    /// <param name="breakDurationSeconds">Duration the circuit remains open (default: 15)</param>
    /// <param name="policyName">Name for logging purposes</param>
    /// <returns>Configured async circuit breaker policy</returns>
    private static IAsyncPolicy<TResult> CreateCircuitBreakerPolicy<TResult>(
        ILogger logger,
        int failureThreshold = 5,
        int breakDurationSeconds = 15,
        string policyName = "CircuitBreakerPolicy"
    )
        where TResult : class
    {
        return Policy
            .Handle<Exception>()
            .OrResult<TResult>(r => r == null)
            .CircuitBreakerAsync<TResult>(
                handledEventsAllowedBeforeBreaking: failureThreshold,
                durationOfBreak: TimeSpan.FromSeconds(breakDurationSeconds),
                onBreak: (outcome, duration) =>
                {
                    var errorMsg = outcome.Exception?.Message ?? "Request failed";
                    logger.LogError(
                        "[{PolicyName}] Circuit breaker opened for {DurationSeconds}s after {FailureThreshold} consecutive failures.",
                        policyName,
                        duration.TotalSeconds,
                        failureThreshold
                    );
                },
                onReset: () =>
                {
                    logger.LogInformation(
                        "[{PolicyName}] Circuit breaker reset - service recovered",
                        policyName
                    );
                },
                onHalfOpen: () =>
                {
                    logger.LogInformation(
                        "[{PolicyName}] Circuit breaker half-open - testing service recovery",
                        policyName
                    );
                }
            );
    }

    /// <summary>
    /// Creates a gRPC-specific circuit breaker policy that handles RpcException and transient failures.
    /// </summary>
    /// <typeparam name="TResult">The result type of the policy</typeparam>
    /// <param name="logger">Logger instance for circuit breaker events</param>
    /// <param name="failureThreshold">Number of consecutive failures before opening (default: 5)</param>
    /// <param name="breakDurationSeconds">Duration the circuit remains open (default: 15)</param>
    /// <param name="policyName">Name for logging purposes</param>
    /// <returns>Configured async circuit breaker policy for gRPC calls</returns>
    private static IAsyncPolicy<TResult> CreateGrpcCircuitBreakerPolicy<TResult>(
        ILogger logger,
        int failureThreshold = 5,
        int breakDurationSeconds = 15,
        string policyName = "GrpcCircuitBreakerPolicy"
    )
        where TResult : class
    {
        return Policy
            .Handle<RpcException>(ex =>
                // Circuit break on transient gRPC errors
                ex.StatusCode == StatusCode.Unavailable
                || ex.StatusCode == StatusCode.DeadlineExceeded
                || ex.StatusCode == StatusCode.ResourceExhausted
            )
            .Or<HttpRequestException>()
            .OrResult<TResult>(r => r == null)
            .CircuitBreakerAsync<TResult>(
                handledEventsAllowedBeforeBreaking: failureThreshold,
                durationOfBreak: TimeSpan.FromSeconds(breakDurationSeconds),
                onBreak: (outcome, duration) =>
                {
                    var errorMsg = outcome.Exception?.Message ?? "gRPC request failed";
                    var statusCode =
                        (outcome.Exception as RpcException)?.StatusCode.ToString() ?? "Unknown";

                    logger.LogError(
                        "[{PolicyName}] Circuit breaker opened for {DurationSeconds}s after {FailureThreshold} consecutive failures | Status: {StatusCode}",
                        policyName,
                        duration.TotalSeconds,
                        failureThreshold,
                        statusCode
                    );
                },
                onReset: () =>
                {
                    logger.LogInformation(
                        "[{PolicyName}] gRPC Circuit breaker reset - service recovered",
                        policyName
                    );
                },
                onHalfOpen: () =>
                {
                    logger.LogInformation(
                        "[{PolicyName}] gRPC Circuit breaker half-open - testing service recovery",
                        policyName
                    );
                }
            );
    }

    private static IAsyncPolicy<TResult> CreateBulkheadPolicy<TResult>(
        ILogger logger,
        int maxConcurrentRequests = 10,
        int maxQueueingActions = 20,
        string policyName = "BulkheadPolicy"
    )
        where TResult : class
    {
        return Policy.BulkheadAsync<TResult>(
            maxParallelization: maxConcurrentRequests,
            maxQueuingActions: maxQueueingActions,
            onBulkheadRejectedAsync: context =>
            {
                logger.LogWarning(
                    "[{PolicyName}] Bulkhead rejected request because capacity is full (parallelizations={Parallel}, queue={Queue}).",
                    policyName,
                    maxConcurrentRequests,
                    maxQueueingActions
                );
                return Task.CompletedTask;
            }
        );
    }

    private static IAsyncPolicy<TResult> CreateRateLimitPolicy<TResult>(
        ILogger logger,
        int permitCount = 20,
        int perSeconds = 1,
        string policyName = "RateLimitPolicy"
    )
        where TResult : class
    {
        return Policy.RateLimitAsync<TResult>(
            permitCount,
            TimeSpan.FromSeconds(perSeconds)
        );
    }

    public static IAsyncPolicy<TResult> CreateResiliencePolicy<TResult>(
        ILogger logger,
        int retryCount,
        double initialDelaySeconds,
        double backoffMultiplier,
        double jitterFactorSeconds,
        int failureThreshold,
        int breakDurationSeconds,
        int maxConcurrentRequests,
        int maxQueueingActions,
        int rateLimitPermitCount,
        int rateLimitPerSeconds,
        string policyName
    )
        where TResult : class
    {
        var retry = CreateGrpcRetryPolicy<TResult>(
            logger,
            retryCount,
            initialDelaySeconds,
            backoffMultiplier,
            jitterFactorSeconds,
            policyName
        );

        var circuit = CreateGrpcCircuitBreakerPolicy<TResult>(
            logger,
            failureThreshold,
            breakDurationSeconds,
            policyName
        );

        var bulkhead = CreateBulkheadPolicy<TResult>(
            logger,
            maxConcurrentRequests,
            maxQueueingActions,
            policyName
        );

        var rateLimit = CreateRateLimitPolicy<TResult>(
            logger,
            rateLimitPermitCount,
            rateLimitPerSeconds,
            policyName
        );

        
        return Policy.WrapAsync(rateLimit, bulkhead, circuit, retry);
    }
}
