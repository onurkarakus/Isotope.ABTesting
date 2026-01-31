# 🧪 Isotope.ABTesting

**A Modern, High-Performance .NET A/B Testing Library with Fluent API Support**

**Isotope.ABTesting** is a flexible, extensible, and production-ready A/B testing solution designed for .NET applications. Built with microservice architectures in mind, it features **Redis** integration, **MurmurHash3**-based deterministic distribution, and strict **Fail-Open** principles to ensure your application remains resilient under any circumstance.

## 🚀 Features

* **Fluent Builder API:** Define experiments easily with readable, chained methods.
* **Multiple Distribution Strategies:**
    * 🎲 **Weighted Random:** Classic probability-based distribution.
    * 🧮 **Deterministic Hash (MurmurHash3):** Ensures the same user always sees the same variant without requiring storage (stateless sticky sessions).
* **Robust State Management:**
    * 📦 **InMemory:** Default, zero-setup local caching.
    * 🔴 **Redis:** High-performance distributed caching support via `StackExchange.Redis`.
* **Fail-Open Architecture:** The system is designed to never crash your application. If Redis fails or timeouts, the library gracefully degrades to algorithmic calculation.
* **Advanced Fallback Mechanisms:** Define default behaviors or custom actions when errors occur.
* **High Performance:** Optimized with `ValueTask` and `Span<T>` to minimize memory allocation (targeting zero-allocation paths).

---

## 📦 Installation

* Install the package via NuGet:

```bash
dotnet add package Isotope.ABTesting
```

## 🏁 Quick Start
1. Service Registration
Register the service in your **Program.cs**:
```C#
var builder = WebApplication.CreateBuilder(args);

// Initializes with default settings and InMemory Store
builder.Services.AddABTesting();

// OR Initialize with custom options
builder.Services.AddABTesting(options => 
{
    options.ServiceName = "OrderService";
    options.DefaultTtl = TimeSpan.FromHours(1);
});
```
2. Defining and Using Experiments
Inject **IABTestClient** to define your experiment and allocate a user to a variant:

```C#
public class OrderController : ControllerBase
{
    private readonly IABTestClient _abTestClient;

    public OrderController(IABTestClient abTestClient)
    {
        _abTestClient = abTestClient;
    }

    [HttpGet("checkout")]
    public async Task<IActionResult> Checkout([FromQuery] string userId)
    {
        // Define the experiment
        var allocation = await _abTestClient.Experiment("checkout-button-color")
            .WithVariants(
                ("Blue", 80),  // 80% Blue
                ("Red", 20)    // 20% Red
            )
            .UseAlgorithm<DeterministicHashStrategy>() // Ensures consistency for the same user
            .OnFailure(new DefaultVariantFallbackPolicy("Blue")) // Fallback to Blue on error
            .GetVariantAsync(userId);

        // Use the result
        if (allocation.Variant.Name == "Red")
        {
            return View("CheckoutRed");
        }

        return View("CheckoutBlue");
    }
}
```
## ⚙️ Configuration
You can manage the library settings via appsettings.json. The system automatically switches to Redis mode if a connection string is detected and enabled.

```JSON
{
  "ABTesting": {
    "ServiceName": "MyECommerceApp",
    "DefaultTtl": "01:00:00", 
    "Redis": {
      "Enabled": true,
      "ConnectionString": "localhost:6379,abortConnect=false",
      "KeyPrefix": "isotope:",
      "ConnectTimeout": 5000,
      "SyncTimeout": 5000
    }
  }
}
```

| Setting | Description | Default |
| :--- | :--- | :--- |
| `ServiceName` | Name of the service used in logs and cache keys. | `Required` |
| `DefaultTtl` | Time-To-Live for the allocation result in the cache. | `1 Hour` |
| `Redis:Enabled` | Enables or disables Redis usage. | `false` |
| `Redis:ConnectionString` | The connection string for the Redis server. | `Required` |
| `Redis:KeyPrefix` | Prefix added to all Redis keys. | `isotope:` |

## 🧠 Architecture & Strategies
Isotope.ABTesting is built upon the Strategy Pattern, allowing you to swap allocation algorithms as needed.

**1. Deterministic Hash (Recommended)**
Uses the MurmurHash3 algorithm. It hashes the User ID and Experiment Name to generate a normalized value between 0-100.

Advantage: Does not require state (Database/Cache) to maintain consistency. Provides "Sticky Sessions" out of the box. A user returning a year later will still see the same variant.

**Usage:** ```.UseAlgorithm<DeterministicHashStrategy>()```

**2. Weighted Random**
Classic random distribution. The dice are rolled on every request.

**Usage:** ```.UseAlgorithm<WeightedRandomStrategy>()```

Note: Recommended to use with .RequirePersistence() or Redis if you want the user to stay in the assigned variant.

**3. Custom (Delegate)**
You can inject your own logic inline.

```C#
.UseAlgorithm((context, cancellationToken) => {
    // Custom logic: VIP users always see "A"
    if (context.SubjectKey.StartsWith("VIP")) 
        return new ValueTask<AllocationResult>(...);
    
    // Default calculation for others...
})
```
| Strategy | Sticky / Consistent? | Requires Storage? | Best For |
| :--- | :--- | :--- | :--- |
DeterministicHashStrategy | Yes | No | User-consistent experience (recommended)
WeightedRandomStrategy | No | Yes (for stickiness) | Pure random / short experiments
Custom delegate | Flexible | Flexible | "VIP rules, segments, business logic"

## 🛡️ Error Handling & Fail-Open
In distributed systems, failures are inevitable. Isotope.ABTesting adopts a Fail-Open approach.

**State Store Failure (e.g., Redis Timeout):** The system does not throw an exception. It logs the error and proceeds to calculate the variant using the selected Algorithm. The user experience is not interrupted.

**Algorithm Failure:** If a calculation error occurs within the strategy, the ```OnFailure``` policy is triggered.

```C#
// Example: Fallback to "Base" variant in worst-case scenario
.OnFailure(new DefaultVariantFallbackPolicy("Base"))

// Example: Log a custom error and return default
.OnFailure(async (ctx, ct) => {
    _logger.LogError(ctx.OriginalException, "AB Test crashed!");
    return AllocationResult.From(ctx.Variants[0], ...);
})
```

## 🧪 Testability
The library is developed with mostly test coverage and is designed to be easily mocked in your own unit tests.

```C#
// Example using NSubstitute
var mockClient = Substitute.For<IABTestClient>();
// ... mock setup ...
```
## 📄 License
This project is licensed under the MIT License.
