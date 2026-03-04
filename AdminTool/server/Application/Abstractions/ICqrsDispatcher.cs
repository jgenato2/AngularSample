namespace Server.Application.Abstractions;

public interface ICqrsDispatcher
{
    Task<TResult> ExecuteCommand<TCommand, TResult>(TCommand command, CancellationToken cancellationToken = default)
        where TCommand : ICommand<TResult>;

    Task<TResult> ExecuteQuery<TQuery, TResult>(TQuery query, CancellationToken cancellationToken = default)
        where TQuery : IQuery<TResult>;
}
