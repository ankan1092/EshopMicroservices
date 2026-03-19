namespace Basket.API.Contracts
{
    public interface IDiscountService
    {
        Task<DiscountResult> GetDiscountAsync(string productName, CancellationToken cancellationToken);
    }

    public record DiscountResult(decimal Amount, bool IsSuccessful, string? ErrorMessage = null);
}
