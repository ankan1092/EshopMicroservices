using MediatR;

namespace BuildingBlocks.CQRS
{
    // Interface for handling commands that do not produce a response.
    // TCommand is the type of the command, which must implement ICommand<Unit>.
    // The handler processes the command and returns a Unit value.
    public interface ICommandHandler<in TCommand> : IRequestHandler<ICommand, Unit>
        where TCommand : ICommand<Unit>
    {
    }

    // Interface for handling commands that produce a response.
    // TCommand is the type of the command, which must implement ICommand<TResponse>.
    // TResponse is the type of the response, which must be notnull.
    // The handler processes the command and returns a response of type TResponse.
    public interface ICommandHandler<in TCommand, TResponse> : IRequestHandler<TCommand, TResponse>
        where TCommand : ICommand<TResponse>
        where TResponse : notnull
    {
    }
}
