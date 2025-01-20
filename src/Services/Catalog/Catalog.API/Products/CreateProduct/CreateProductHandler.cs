using BuildingBlocks.CQRS;  
using Catalog.API.Models;

namespace Catalog.API.Products.CreateProduct
{
    // Using record to define the CreateProductCommand, with Product as the input type
    // Using the ICommand and ICommandHandler interfaces from the BuildingBlocks class library
    public record CreateProductCommand(Product Product) : ICommand<CreateProductResult>;

    // Using record to define the CreateProductResult, with Guid as the output type
    public record CreateProductResult(Guid Id);

    internal class CreateProductCommandHandler : ICommandHandler<CreateProductCommand, CreateProductResult>
    {
        // Implementing the Handle method to handle the CreateProductCommand
        public async Task<CreateProductResult> Handle(CreateProductCommand command, CancellationToken cancellationToken)
        {
            // Command contains the Product details, coming from UI side
            // save the Product details in DB
            // return Result


            //save to DB

            //return Result

            return  new CreateProductResult(Guid.NewGuid());
        }
    }
}
