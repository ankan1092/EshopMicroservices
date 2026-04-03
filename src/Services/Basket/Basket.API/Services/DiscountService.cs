using Basket.API.Contracts;
using Basket.API.Services.Resilience;
using Discount.Grpc;
using Grpc.Core;
using Microsoft.Extensions.Options;
using Polly;
using Polly.CircuitBreaker;

namespace Basket.API.Services
{
    public class DiscountService : IDiscountService
    {
        private readonly DiscountProtoService.DiscountProtoServiceClient _discountProtoClient;
        private readonly IAsyncPolicy<CouponModel> _resiliencePipeline;
        private readonly ILogger<DiscountService> _logger;

        public DiscountService(
            DiscountProtoService.DiscountProtoServiceClient discountProtoClient,
            IOptions<ResiliencePoliciesConfig> resilienceOptions,
            IAsyncPolicy<CouponModel> resiliencePipeline,
            ILogger<DiscountService> logger
        )
        {
            _discountProtoClient = discountProtoClient;
            _logger = logger;
            _resiliencePipeline = resiliencePipeline;
        }

        public async Task<DiscountResult> GetDiscountAsync(
            string productName,
            CancellationToken cancellationToken
        )
        {
            _logger.LogInformation("Fetching discount for product: {ProductName}", productName);

            try
            {
                var coupon = await _resiliencePipeline.ExecuteAsync(async () =>
                {
                    return await _discountProtoClient.GetDiscountAsync(
                        new GetDiscountRequest { ProductName = productName },
                        cancellationToken: cancellationToken
                    );
                });

                _logger.LogInformation(
                    "Discount fetched successfully for product: {ProductName}, Amount: {Amount}",
                    productName,
                    coupon.Amount
                );

                return new DiscountResult(coupon.Amount, true);
            }
            catch (BrokenCircuitException ex)
            {
                _logger.LogError(
                    ex,
                    "Circuit breaker is open for product: {ProductName}",
                    productName
                );
                return new DiscountResult(
                    0,
                    false,
                    "Discount service temporarily unavailable: Circuit breaker is open"
                );
            }
            catch (RpcException rpcEx)
            {
                _logger.LogError(
                    rpcEx,
                    "gRPC error after retries for product: {ProductName}",
                    productName
                );
                return new DiscountResult(
                    0,
                    false,
                    $"Discount service unavailable: {rpcEx.Message}"
                );
            }
            catch (Exception ex)
            {
                _logger.LogError(
                    ex,
                    "Unexpected error after retries for product: {ProductName}",
                    productName
                );
                return new DiscountResult(0, false, $"Error retrieving discount: {ex.Message}");
            }
        }
    }
}
