using MediatR;

namespace BuildingBlocks.CQRS
{
    public interface IQuery<out TResponse> : IRequest<TResponse>
        where TResponse : notnull
    {
        // Interface for queries that return a response.
        // TResponse is the type of the response, which must be notnull.
    }
}
