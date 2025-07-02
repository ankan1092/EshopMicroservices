using Carter;
using Catalog.API.Models;
using Catalog.API.Products.CreateProduct;
using MediatR;

namespace Catalog.API.Products.UpdateProduct
{
    public class UpdateProductEndpoint : ICarterModule
    {
        public void AddRoutes(IEndpointRouteBuilder app)
        {
            app.MapPost("/products/update",
                async (Product Product, ISender sender) =>
                {
                    var request = new UpdateProductCommand(Product);
                    var result = await sender.Send(request);

                    return Results.Ok(result.Product);
                })
                .WithName("UpdateProduct")
                .Produces<Product>(StatusCodes.Status200OK)
                .WithSummary("Update Product")
                .WithDescription("Update Product");
        }
    }
}
