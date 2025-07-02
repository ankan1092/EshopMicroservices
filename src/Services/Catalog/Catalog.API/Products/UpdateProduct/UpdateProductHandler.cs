using BuildingBlocks.CQRS;
using Catalog.API.Models;
using FluentValidation;
using Mapster;
using Marten;

namespace Catalog.API.Products.UpdateProduct
{
    public record UpdateProductCommand(Product Product) : ICommand<UpdateProductCommandResult>;
    public record UpdateProductCommandResult(Product Product);

    public class UpdateProductCommandValidator : AbstractValidator<UpdateProductCommand>
    {
        public UpdateProductCommandValidator()
        {
            RuleFor(x => x.Product.Id).NotEmpty().WithMessage("Product ID is required.");
            RuleFor(x => x.Product.Name).NotEmpty().WithMessage("Name is required.");
            RuleFor(x => x.Product.Category).NotEmpty().WithMessage("Category is required.");
            RuleFor(x => x.Product.ImageFile).NotEmpty().WithMessage("Image file is required.");
            RuleFor(x => x.Product.Price).GreaterThan(0).WithMessage("Price must be greater than 0.");
        }
    }

    internal class UpdateProductCommandHandler : ICommandHandler<UpdateProductCommand, UpdateProductCommandResult>
    {
        private readonly IDocumentSession _session;

        public UpdateProductCommandHandler(IDocumentSession session)
        {
            _session = session;
        }

        public async Task<UpdateProductCommandResult> Handle(UpdateProductCommand command, CancellationToken cancellationToken)
        {
            // Load product from the database
            var dbProduct = await _session.LoadAsync<Product>(command.Product.Id, cancellationToken);
            if (dbProduct == null)
            {
                throw new KeyNotFoundException($"Product with ID '{command.Product.Id}' not found.");
            }

            // Update product properties
            dbProduct.Name = command.Product.Name;
            dbProduct.Description = command.Product.Description;
            dbProduct.Price = command.Product.Price;
            dbProduct.ImageFile = command.Product.ImageFile;
            dbProduct.Category = command.Product.Category;

            // Store and save changes
            _session.Store(dbProduct);
            await _session.SaveChangesAsync(cancellationToken);

            return new UpdateProductCommandResult(dbProduct);
        }
    }
}
