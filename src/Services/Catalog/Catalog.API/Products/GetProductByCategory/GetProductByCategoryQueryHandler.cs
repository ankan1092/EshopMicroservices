using BuildingBlocks.CQRS;
using Catalog.API.Exceptions;
using Catalog.API.Models;
using Marten;

namespace Catalog.API.Products.GetProductByCategory
{
    public record GetProductByCategoryQuery(List<string> Categories) : IQuery<GetProductByCategoryResult>;
    public record GetProductByCategoryResult(IReadOnlyList<Product> Products);
    internal class GetProductByCategoryQueryHandler
        (IDocumentSession session, ILogger<GetProductByCategoryQueryHandler> logger)
        : IQueryHandler<GetProductByCategoryQuery, GetProductByCategoryResult>
    {
        public async Task<GetProductByCategoryResult> Handle(GetProductByCategoryQuery query, CancellationToken cancellationToken)
        {
            logger.LogInformation("GetProductBycategoryQueryHandler.Handle called with {Query}", query);

            var products = await session.Query<Product>()
                                    .Where(p => p.Category.Any(c => query.Categories.Contains(c)))
                                    .ToListAsync(cancellationToken);


            if (products.Count==0)
            {
                throw new ProductNotFoundException();
            }

            return new GetProductByCategoryResult(products);
        }
    }
}
