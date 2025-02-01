using MediatR;

namespace BuildingBlocks.CQRS
{
    // Interface for handling queries that produce a response.
    // TCommand is the type of the query command, which must implement ICommand<TResponse>.
    // TResponse is the type of the response.
    // The handler processes the query and returns a response of type TResponse.
    public interface IQueryHandler<in TQuerry, TResponse> : IRequestHandler<TQuerry, TResponse>
        where TQuerry : IQuery<TResponse>
        where TResponse : notnull
    {
    }
}
