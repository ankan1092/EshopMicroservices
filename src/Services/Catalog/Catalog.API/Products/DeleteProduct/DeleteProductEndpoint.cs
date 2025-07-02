using Carter;
using Catalog.API.Models;
using MediatR;

namespace Catalog.API.Products.DeleteProduct
{
    public class DeleteProductEndpoint : ICarterModule
    {
        public void AddRoutes(IEndpointRouteBuilder app)
        {
            app.MapDelete("/products/{id}",
                async (Guid id, ISender sender) =>
                {
                    var request = new DeleteProductCommandRequest(id);
                    var result = await sender.Send(request);

                    return Results.Ok(result.product);
                })
                .WithName("DeleteeProduct")
                .Produces<Product>(StatusCodes.Status200OK)
                .WithSummary("Delete Product")
                .WithDescription("Delete Product");
        }
    }
}
