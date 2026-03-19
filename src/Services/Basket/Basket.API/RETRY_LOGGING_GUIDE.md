## Polly Retry Logging - Troubleshooting Guide

### ✅ What Changed

The retry logging has been upgraded from `Console.WriteLine()` to proper ASP.NET Core `ILogger`, which integrates with your application's logging pipeline.

### 📋 Expected Logs

When the Discount microservice is unavailable, you should now see logs like:

```
info: Basket.API.Services.DiscountService[0]
      Fetching discount for product: Product1

warn: Basket.API.Services.Resilience.RetryPolicyFactory[0]
      [DiscountService] Retry attempt 1/3 after 1.0s | Status: Unavailable | Error: The remote server is unavailable
      
warn: Basket.API.Services.Resilience.RetryPolicyFactory[0]
      [DiscountService] Retry attempt 2/3 after 2.0s | Status: Unavailable | Error: The remote server is unavailable
      
warn: Basket.API.Services.Resilience.RetryPolicyFactory[0]
      [DiscountService] Retry attempt 3/3 after 4.0s | Status: Unavailable | Error: The remote server is unavailable

error: Basket.API.Services.DiscountService[0]
       gRPC error fetching discount for product: Product1, Status: Unavailable
```

### 🔍 How to Verify Logs are Visible

#### **Option 1: Visual Studio Debug Output**
1. Run the application in Debug mode (F5)
2. Go to `Debug` → `Windows` → `Output`
3. Select `Show output from: Debug`
4. You should see logs there

#### **Option 2: Console Output**
1. Run the application
2. Check the console window where the app is running
3. Logs will appear in real-time

#### **Option 3: Application Output Window**
1. In Visual Studio, go to `View` → `Output`
2. Select the dropdown and choose your running application
3. Watch for log messages

### ⚙️ Logging Configuration

**appsettings.json** (Production):
```json
"Logging": {
  "LogLevel": {
    "Default": "Information",
    "Microsoft.AspNetCore": "Warning",
    "Basket.API.Services": "Information"  // ← Retry logs here
  }
}
```

**appsettings.Development.json** (Development):
```json
"Logging": {
  "LogLevel": {
    "Default": "Information",
    "Microsoft.AspNetCore": "Warning",
    "Basket.API.Services": "Debug"  // ← More detailed logs in dev
  }
}
```

### 🔧 Troubleshooting

#### **Still Not Seeing Logs?**

1. **Check log level** - Ensure `Basket.API.Services` is set to `Information` or lower
2. **Verify environment** - Check if running in Development or Production mode
3. **Check Visual Studio Output** - Make sure you're looking in the right output pane
4. **Restart the application** - Changes to appsettings might require restart

#### **Logs Too Verbose?**
Change the log level:
```json
"Basket.API.Services": "Warning"  // Only show warnings and errors
```

#### **Only See Error, No Retries?**
This means:
- The exception might be non-transient (not retryable)
- The policy might not be matching the exception type
- Verify the exception is `RpcException` with `StatusCode.Unavailable`

### 📊 Log Levels Explained

| Level | Visibility | Use Case |
|-------|-----------|----------|
| Debug | Development only | Detailed diagnostics |
| Information | All environments | General operations (discount fetched) |
| Warning | All environments | Retry attempts, non-fatal issues |
| Error | All environments | Exceptions, final failures |

### 🧪 Testing Retry Logs

**Step 1:** Stop the Discount microservice
```powershell
# Stop the running discount service
```

**Step 2:** Make a request to store basket
```bash
POST http://localhost:6001/api/basket
{
  "userName": "test-user",
  "items": [
    {
      "productName": "Product1",
      "quantity": 1,
      "price": 100
    }
  ]
}
```

**Step 3:** Watch for retry logs in Output window
You should see:
- Initial request attempt
- 3 retry attempts with increasing delays
- Final error message

**Step 4:** Restart Discount microservice mid-way
- Stop discount service before the 3rd retry
- Start it during one of the retries
- You should see a successful response

### 📝 Files Modified

- ✅ `Services/Basket/Basket.API/Services/Resilience/RetryPolicyFactory.cs` - Updated to use ILogger
- ✅ `Services/Basket/Basket.API/Services/DiscountService.cs` - Added ILogger injection
- ✅ `Services/Basket/Basket.API/appsettings.json` - Added Basket.API.Services logging config
- ✅ `Services/Basket/Basket.API/appsettings.Development.json` - Enhanced dev logging

### 🔗 Related Commands

```powershell
# Run with detailed logging
# (Already configured in appsettings.Development.json)

# View logs in VS Code (if using)
# Ctrl + Shift + D → Select application → View output
```

---

**Note:** Retry logs use `LogWarning`, so they're always visible when log level is `Information` or below.
