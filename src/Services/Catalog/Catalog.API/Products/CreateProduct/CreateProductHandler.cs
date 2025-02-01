using BuildingBlocks.CQRS;  
using Catalog.API.Models;
using Mapster;
using Marten;

namespace Catalog.API.Products.CreateProduct
{
    // Using record to define the CreateProductCommand, with Product as the input type
    // Using the ICommand and ICommandHandler interfaces from the BuildingBlocks class library
    public record CreateProductCommand(ProductDto Product) : ICommand<CreateProductResult>;

    // Using record to define the CreateProductResult, with Guid as the output type
    public record CreateProductResult(Guid Id);

    internal class CreateProductCommandHandler : ICommandHandler<CreateProductCommand, CreateProductResult>
    {
        private readonly IDocumentSession _session;

        public CreateProductCommandHandler(IDocumentSession session)
        {
            //Inject Marten Document Session 
            _session = session;
        }
        // Implementing the Handle method to handle the CreateProductCommand
        public async Task<CreateProductResult> Handle(CreateProductCommand command, CancellationToken cancellationToken)
        {
            // Convert DTO to Entity Model
            var product = command.Product.Adapt<Product>();
            
            // Save to DB

            _session.Store(product);
            await _session.SaveChangesAsync(cancellationToken);

            // Return result
            return  new CreateProductResult(product.Id);
        }
    }
}
