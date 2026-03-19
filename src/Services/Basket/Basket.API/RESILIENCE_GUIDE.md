## Generic Retry Policy Implementation Guide

### Overview
The `RetryPolicyFactory` provides reusable, configurable retry policies with exponential backoff that can be used across all microservices.

### Features
- ✅ Generic retry policy factory (`CreateRetryPolicy`)
- ✅ gRPC-specific retry policy (`CreateGrpcRetryPolicy`)
- ✅ Configurable retry count, initial delay, and backoff multiplier
- ✅ Exponential backoff support
- ✅ Centralized configuration via `appsettings.json`
- ✅ Per-service configuration (Discount, Catalog, etc.)

---

### Configuration (appsettings.json)

```json
{
  "ResiliencePolicies": {
    "Discount": {
      "RetryCount": 3,
      "InitialDelaySeconds": 1,
      "BackoffMultiplier": 2,
      "CircuitBreakerFailureThreshold": 5,
      "CircuitBreakerTimeoutSeconds": 30
    },
    "Catalog": {
      "RetryCount": 3,
      "InitialDelaySeconds": 1,
      "BackoffMultiplier": 2,
      "CircuitBreakerFailureThreshold": 5,
      "CircuitBreakerTimeoutSeconds": 30
    }
  }
}
```

---

### Usage Examples

#### 1. **Using Default Retry Policy (Generic)**

```csharp
public class MyService
{
    private readonly IAsyncPolicy<MyResult> _retryPolicy = 
        RetryPolicyFactory.CreateRetryPolicy<MyResult>(
            retryCount: 3,
            initialDelaySeconds: 1,
            backoffMultiplier: 2,
            policyName: "MyService");

    public async Task<MyResult> ExecuteAsync(CancellationToken cancellationToken)
    {
        return await _retryPolicy.ExecuteAsync(async () =>
        {
            // Your async operation here
            return await SomeAsyncOperation();
        });
    }
}
```

#### 2. **Using gRPC Retry Policy (Current Implementation)**

```csharp
public class DiscountService(
    DiscountProtoService.DiscountProtoServiceClient discountProtoClient,
    IOptions<ResiliencePoliciesConfig> resilienceOptions) : IDiscountService
{
    private readonly IAsyncPolicy<DiscountResult> _retryPolicy = 
        RetryPolicyFactory.CreateGrpcRetryPolicy<DiscountResult>(
            retryCount: resilienceOptions.Value.Discount.RetryCount,
            initialDelaySeconds: resilienceOptions.Value.Discount.InitialDelaySeconds,
            policyName: "DiscountService");

    public async Task<DiscountResult> GetDiscountAsync(string productName, CancellationToken cancellationToken)
    {
        return await _retryPolicy.ExecuteAsync(async () =>
        {
            try
            {
                var coupon = await discountProtoClient.GetDiscountAsync(
                    new GetDiscountRequest { ProductName = productName },
                    cancellationToken: cancellationToken);
                
                return new DiscountResult(coupon.Amount, true);
            }
            catch (RpcException rpcEx)
            {
                return new DiscountResult(0, false, $"Service unavailable: {rpcEx.Message}");
            }
        });
    }
}
```

#### 3. **Using Configuration-Based Retry Policy**

```csharp
public class CatalogService(
    CatalogApiClient catalogClient,
    IOptions<ResiliencePoliciesConfig> resilienceOptions) : ICatalogService
{
    private readonly IAsyncPolicy<CatalogResult> _retryPolicy = 
        RetryPolicyFactory.CreateRetryPolicy<CatalogResult>(
            retryCount: resilienceOptions.Value.Catalog.RetryCount,
            initialDelaySeconds: resilienceOptions.Value.Catalog.InitialDelaySeconds,
            backoffMultiplier: resilienceOptions.Value.Catalog.BackoffMultiplier,
            policyName: "CatalogService");

    public async Task<CatalogResult> GetProductAsync(int productId, CancellationToken cancellationToken)
    {
        return await _retryPolicy.ExecuteAsync(async () =>
        {
            return await catalogClient.GetProductAsync(productId, cancellationToken);
        });
    }
}
```

---

### Retry Behavior

**With default settings (RetryCount=3, InitialDelaySeconds=1, BackoffMultiplier=2):**

```
Attempt 1: Fails → Wait 1 second
Attempt 2: Fails → Wait 2 seconds
Attempt 3: Fails → Wait 4 seconds
Attempt 4: Final attempt (no wait)
```

**Log output:**
```
[DiscountService] Retry 1/3 after 1.0s due to: The remote server is unavailable
[DiscountService] Retry 2/3 after 2.0s due to: The remote server is unavailable
[DiscountService] Retry 3/3 after 4.0s due: The remote server is unavailable
```

---

### Dependency Injection Setup (Program.cs)

```csharp
// Register resilience configuration
builder.Services.Configure<ResiliencePoliciesConfig>(
    builder.Configuration.GetSection("ResiliencePolicies"));

// Register your service
builder.Services.AddScoped<IDiscountService, DiscountService>();
```

---

### Exception Handling

#### gRPC Transient Errors (Automatically Retried)
- `StatusCode.Unavailable` - Service temporarily down
- `StatusCode.DeadlineExceeded` - Request timeout
- `StatusCode.ResourceExhausted` - Service overloaded

#### Non-Transient Errors (Not Retried)
- `StatusCode.NotFound` - Resource doesn't exist
- `StatusCode.InvalidArgument` - Bad request
- `StatusCode.PermissionDenied` - Authorization failure

---

### Customization

#### Change retry count per service:

**appsettings.json:**
```json
{
  "ResiliencePolicies": {
    "Discount": {
      "RetryCount": 5  // More retries for critical service
    },
    "Catalog": {
      "RetryCount": 2  // Fewer retries for non-critical service
    }
  }
}
```

#### Use different policy names for logging:

```csharp
var policy = RetryPolicyFactory.CreateGrpcRetryPolicy<MyResult>(
    retryCount: 3,
    initialDelaySeconds: 1,
    policyName: "CustomServiceRetry");
```

---

### Best Practices

1. **Use configuration-based settings** for flexibility across environments
2. **Use gRPC policy** when calling gRPC services
3. **Use generic policy** for HTTP/REST calls
4. **Include meaningful policy names** for better logging
5. **Adjust retry count** based on service criticality
6. **Monitor logs** to identify frequently failing services

---

### Testing

```csharp
[TestFixture]
public class DiscountServiceTests
{
    [Test]
    public async Task GetDiscount_ShouldRetry_WhenServiceUnavailable()
    {
        // Arrange
        var mockClient = new Mock<DiscountProtoService.DiscountProtoServiceClient>();
        var options = Options.Create(new ResiliencePoliciesConfig
        {
            Discount = new ResilienceSettings { RetryCount = 3 }
        });
        
        mockClient
            .Setup(x => x.GetDiscountAsync(It.IsAny<GetDiscountRequest>(), null, null, default))
            .ThrowsAsync(new RpcException(new Status(StatusCode.Unavailable, "Service down")));

        var service = new DiscountService(mockClient.Object, options);

        // Act
        var result = await service.GetDiscountAsync("Product1", CancellationToken.None);

        // Assert
        Assert.IsFalse(result.IsSuccessful);
        mockClient.Verify(
            x => x.GetDiscountAsync(It.IsAny<GetDiscountRequest>(), null, null, default),
            Times.Exactly(4)); // 3 retries + 1 initial = 4 calls
    }
}
```

---

### Related Files

- `Services/Basket/Basket.API/Services/Resilience/RetryPolicyFactory.cs` - Policy factory
- `Services/Basket/Basket.API/Services/Resilience/ResilienceSettings.cs` - Configuration models
- `Services/Basket/Basket.API/Services/DiscountService.cs` - Example implementation
- `Services/Basket/Basket.API/appsettings.json` - Configuration

---
