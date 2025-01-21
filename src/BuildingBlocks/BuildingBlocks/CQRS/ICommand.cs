using MediatR;
namespace BuildingBlocks.CQRS
{
    public interface ICommand : ICommand<Unit>
    {
        // For commands not producing any response
        // Unit is the default void type in MediatR
    }

    public interface ICommand<out TResponse> : IRequest<TResponse>
    {
        // For commands producing a response
    }
}
