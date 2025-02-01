using BuildingBlocks.CQRS;
using Catalog.API.Models;
using Marten;


namespace Catalog.API.Products.GetProducts
{
    public record GetProductsQuery(): IQuery<GetProductResult>;
    public record GetProductResult(IEnumerable<Product> Products);
    internal class GetProductsHandler(IDocumentSession session,ILogger<GetProductsHandler>logger) : IQueryHandler<GetProductsQuery, GetProductResult>
    {
        public async Task<GetProductResult> Handle(GetProductsQuery request, CancellationToken cancellationToken)
        {
            logger.LogInformation("Getting Products");

            //get products list from db

            var products = await session.Query<Product>().ToListAsync(cancellationToken);

            //return the List of products
            return new GetProductResult(products);
        }
    }
}
