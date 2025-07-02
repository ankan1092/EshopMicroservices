using BuildingBlocks.CQRS;  
using Catalog.API.Models;
using FluentValidation;
using Mapster;
using Marten;

namespace Catalog.API.Products.CreateProduct
{
    // Using record to define the CreateProductCommand, with Product as the input type
    // Using the ICommand and ICommandHandler interfaces from the BuildingBlocks class library
    public record CreateProductCommand(ProductDto Product) : ICommand<CreateProductResult>;

    // Using record to define the CreateProductResult, with Guid as the output type
    public record CreateProductResult(Guid Id);

    //validator
    public class CreateProductCommandValidator : AbstractValidator<CreateProductCommand>
    {
        public CreateProductCommandValidator()
        {
            RuleFor(x => x.Product.Name).NotEmpty().WithMessage("Name is required");
            RuleFor(x => x.Product.Category).NotEmpty().WithMessage("Category is required");
            RuleFor(x => x.Product.ImageFile).NotEmpty().WithMessage("Image file is required");
            RuleFor(x => x.Product.Price).GreaterThan(0).WithMessage("Price must be greater than 0");
        }
    }

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
           
            var product = command.Product.Adapt<Product>();
            _session.Store(product);
            await _session.SaveChangesAsync(cancellationToken);

            return  new CreateProductResult(product.Id);
        }
    }
}
