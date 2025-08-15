using Basket.API.Models;
using Microsoft.AspNetCore.Mvc.TagHelpers.Cache;
using Microsoft.Extensions.Caching.Distributed;
using System.Text.Json;

namespace Basket.API.Data
{
    public class CachedBasketRepository
        (IBasketRepository repository,IDistributedCache cache ) 
        : IBasketRepository
    {
        

        public async Task<ShoppingCart> GetBasket(string userName, CancellationToken cancellationToken = default)
        {
            // Check cache first
            var cachedBasket = await cache.GetStringAsync(userName, cancellationToken);

            // If cache is not empty, return cached basket
            if (!string.IsNullOrEmpty(cachedBasket))
            {
                return JsonSerializer.Deserialize<ShoppingCart>(cachedBasket);
            }

            // If cache is empty, get from repository and store in cache
            var basket = await repository.GetBasket(userName, cancellationToken);

            await cache.SetStringAsync(userName,JsonSerializer.Serialize(basket), cancellationToken);
            return basket;  
        }

        public async Task<ShoppingCart> StoreBasket(ShoppingCart basket, CancellationToken cancellationToken = default)
        {
            // Store in cache
            await cache.SetStringAsync(basket.UserName, JsonSerializer.Serialize(basket), cancellationToken);
            return await repository.StoreBasket(basket, cancellationToken);
        }

        public async Task<bool> DeleteBasket(string userName, CancellationToken cancellationToken = default)
        {
            // Remove from cache
            await cache.RemoveAsync(userName,cancellationToken);
            return await  repository.DeleteBasket(userName, cancellationToken);
        }
    }
}
