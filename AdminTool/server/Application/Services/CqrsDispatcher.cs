using Server.Application.Abstractions;

namespace Server.Application.Services;

public sealed class CqrsDispatcher(IServiceProvider serviceProvider) : ICqrsDispatcher
{
    public Task<TResult> ExecuteCommand<TCommand, TResult>(TCommand command, CancellationToken cancellationToken = default)
        where TCommand : ICommand<TResult>
    {
        var handler = serviceProvider.GetRequiredService<ICommandHandler<TCommand, TResult>>();
        return handler.Handle(command, cancellationToken);
    }

    public Task<TResult> ExecuteQuery<TQuery, TResult>(TQuery query, CancellationToken cancellationToken = default)
        where TQuery : IQuery<TResult>
    {
        var handler = serviceProvider.GetRequiredService<IQueryHandler<TQuery, TResult>>();
        return handler.Handle(query, cancellationToken);
    }
}
