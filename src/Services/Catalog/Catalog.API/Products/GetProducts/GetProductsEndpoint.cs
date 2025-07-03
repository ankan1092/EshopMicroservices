using Carter;
using Catalog.API.Models;
using Catalog.API.Products.CreateProduct;
using Mapster;
using MediatR;

namespace Catalog.API.Products.GetProducts
{
    // Response Object
    public record GetProductResponse(IEnumerable<Product> Products);
    public class GetProductsEndpoint:ICarterModule
    {
        public void AddRoutes(IEndpointRouteBuilder app)
        {
            app.MapGet("/products",
                async (int pageNumber,int pageSize,ISender sender) =>
                {
                    var request = new GetProductsQuery(pageNumber,pageSize);
                    var result = await sender.Send(request);

                    var response = result.Adapt<GetProductResponse>();
                    return Results.Ok(response);
                })
                .WithName("GetProducts")
                .Produces<CreateProductResult>(StatusCodes.Status201Created)
                .WithSummary("Get Product")
                .WithDescription("Get Product List");
        }
    }
}
