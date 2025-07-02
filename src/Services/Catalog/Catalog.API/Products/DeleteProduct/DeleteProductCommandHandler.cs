using BuildingBlocks.CQRS;
using Catalog.API.Models;
using Catalog.API.Products.GetproductById;
using Catalog.API.Products.UpdateProduct;
using FluentValidation;
using Marten;

namespace Catalog.API.Products.DeleteProduct
{
    public record DeleteProductCommandRequest(Guid id) : ICommand<DeleteProductCommandResult>;
    public record DeleteProductCommandResult(Product product);
    public class CreateProductCommandValidator : AbstractValidator<GetProductByIdQuery>
    {
        public CreateProductCommandValidator()
        {
            RuleFor(x => x.Id).NotEmpty().NotNull().WithMessage("Product Id is required");
        }
    }


    internal class DeleteProductCommandHandler : ICommandHandler<DeleteProductCommandRequest, DeleteProductCommandResult>
    {
        private readonly IDocumentSession _session;
        private readonly IValidator<UpdateProductCommand> _validator;
        private readonly ILogger<DeleteProductCommandHandler> _logger;
        public DeleteProductCommandHandler(IDocumentSession session, IValidator<UpdateProductCommand> validator,ILogger<DeleteProductCommandHandler> logger)
        {
            _session = session;
            _validator = validator;
            _logger = logger;
        }
        public async Task<DeleteProductCommandResult> Handle(DeleteProductCommandRequest request, CancellationToken cancellationToken)
        {
            var dbProduct = await _session.LoadAsync<Product>(request.id, cancellationToken);
            if (dbProduct == null)
            {
                throw new KeyNotFoundException($"Product with ID '{request.id}' not found.");
            }

            _session.Delete(dbProduct);
            await _session.SaveChangesAsync(cancellationToken);

            return new DeleteProductCommandResult(dbProduct);
        }
    }
}
