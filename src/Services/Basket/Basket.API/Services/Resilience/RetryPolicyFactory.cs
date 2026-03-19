using Microsoft.Extensions.Logging;
using Polly;
using Grpc.Core;

namespace Basket.API.Services.Resilience;

/// <summary>
/// Factory for creating reusable resilience policies with retry and exponential backoff.
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
    public static IAsyncPolicy<TResult> CreateRetryPolicy<TResult>(
        ILogger logger,
        int retryCount = 3,
        double initialDelaySeconds = 1,
        double backoffMultiplier = 2,
        string policyName = "RetryPolicy")
        where TResult : class
    {
        return Policy
            .Handle<Exception>()
            .OrResult<TResult>(r => r == null)
            .WaitAndRetryAsync<TResult>(
                retryCount: retryCount,
                sleepDurationProvider: attempt => 
                    TimeSpan.FromSeconds(initialDelaySeconds * Math.Pow(backoffMultiplier, attempt - 1)),
                onRetry: (outcome, timespan, retryAttempt, context) =>
                {
                    var errorMsg = outcome.Exception?.Message ?? "Request failed";
                    logger.LogWarning(
                        "[{PolicyName}] Retry attempt {RetryAttempt}/{RetryCount} after {DelaySeconds:F1}s due to: {ErrorMessage}",
                        policyName, retryAttempt, retryCount, timespan.TotalSeconds, errorMsg);
                });
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
    public static IAsyncPolicy<TResult> CreateGrpcRetryPolicy<TResult>(
        ILogger logger,
        int retryCount = 3,
        double initialDelaySeconds = 1,
        string policyName = "GrpcRetryPolicy")
        where TResult : class
    {
        return Policy
            .Handle<RpcException>(ex =>
                // Retry on transient gRPC errors
                ex.StatusCode == StatusCode.Unavailable ||
                ex.StatusCode == StatusCode.DeadlineExceeded ||
                ex.StatusCode == StatusCode.ResourceExhausted)
            .Or<HttpRequestException>()
            .OrResult<TResult>(r => r == null)
            .WaitAndRetryAsync<TResult>(
                retryCount: retryCount,
                sleepDurationProvider: attempt => 
                    TimeSpan.FromSeconds(initialDelaySeconds * Math.Pow(2, attempt - 1)),
                onRetry: (outcome, timespan, retryAttempt, context) =>
                {
                    var errorMsg = outcome.Exception?.Message ?? "gRPC request failed";
                    var statusCode = (outcome.Exception as RpcException)?.StatusCode.ToString() ?? "Unknown";

                    logger.LogWarning(
                        "[{PolicyName}] Retry attempt {RetryAttempt}/{RetryCount} after {DelaySeconds:F1}s | Status: {StatusCode} | Error: {ErrorMessage}",
                        policyName, retryAttempt, retryCount, timespan.TotalSeconds, statusCode, errorMsg);
                });
    }
}
