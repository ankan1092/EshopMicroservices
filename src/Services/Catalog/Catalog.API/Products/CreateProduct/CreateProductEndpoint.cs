using Carter;
using MediatR;

namespace Catalog.API.Products.CreateProduct
{
    public class CreateProductEndpoint : ICarterModule
    {
        public void AddRoutes(IEndpointRouteBuilder app)
        {
            app.MapPost("/products",
                async (ProductDto Product, ISender sender) =>
                {
                    var request = new CreateProductCommand(Product);
                    var result = await sender.Send(request);

                    return Results.Created($"/products/{result.Id}", result);
                })
                .WithName("CreateProduct")
                .Produces<CreateProductResult>(StatusCodes.Status201Created)
                .WithSummary("Create Product")
                .WithDescription("Create Product");
        }
    }
}   
