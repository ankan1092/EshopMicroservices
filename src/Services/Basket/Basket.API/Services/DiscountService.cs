using Basket.API.Contracts;
using Basket.API.Services.Resilience;
using Discount.Grpc;
using Grpc.Core;
using Microsoft.Extensions.Options;
using Polly;

namespace Basket.API.Services
{
    public class DiscountService : IDiscountService
    {
        private readonly DiscountProtoService.DiscountProtoServiceClient _discountProtoClient;
        private readonly IAsyncPolicy<CouponModel> _retryPolicy;
        private readonly ILogger<DiscountService> _logger;

        public DiscountService(
            DiscountProtoService.DiscountProtoServiceClient discountProtoClient,
            IOptions<ResiliencePoliciesConfig> resilienceOptions,
            ILogger<DiscountService> logger)
        {
            _discountProtoClient = discountProtoClient;
            _logger = logger;

            _retryPolicy = RetryPolicyFactory.CreateGrpcRetryPolicy<CouponModel>(
                logger: logger,
                retryCount: resilienceOptions.Value.Discount.RetryCount,
                initialDelaySeconds: resilienceOptions.Value.Discount.InitialDelaySeconds,
                policyName: "DiscountService");
        }

        public async Task<DiscountResult> GetDiscountAsync(string productName, CancellationToken cancellationToken)
        {
            _logger.LogInformation("Fetching discount for product: {ProductName}", productName);

            try
            {
                var coupon = await _retryPolicy.ExecuteAsync(async () =>
                {
                    return await _discountProtoClient.GetDiscountAsync(
                        new GetDiscountRequest { ProductName = productName },
                        cancellationToken: cancellationToken);
                });

                _logger.LogInformation("Discount fetched successfully for product: {ProductName}, Amount: {Amount}", productName, coupon.Amount);

                return new DiscountResult(coupon.Amount, true);
            }
            catch (RpcException rpcEx)
            {
                _logger.LogError(rpcEx, "gRPC error after retries for product: {ProductName}", productName);
                return new DiscountResult(0, false, $"Discount service unavailable: {rpcEx.Message}");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error after retries for product: {ProductName}", productName);
                return new DiscountResult(0, false, $"Error retrieving discount: {ex.Message}");
            }
        }
    }
}
