// Services/Basket/Basket.API/Basket/StoreBasket/IDiscountService.cs
namespace Basket.API.Basket.StoreBasket;

public interface IDiscountService
{
    Task<DiscountResult> GetDiscountAsync(string productName, CancellationToken cancellationToken);
}

public record DiscountResult(decimal Amount, bool IsSuccessful, string? ErrorMessage = null);