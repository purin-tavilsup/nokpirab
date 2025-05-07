using System.Runtime.CompilerServices;
using Microsoft.Extensions.DependencyInjection;

[assembly: InternalsVisibleTo("Nokpirab.Tests")]
namespace Nokpirab;

internal class Nokpirab : INokpirab
{
    private readonly IServiceProvider _serviceProvider;

    public Nokpirab(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
    }

    public async Task SendAsync(ICommand command, CancellationToken cancellationToken = default)
    {
        var commandType = command.GetType();
        var handlerType = typeof(ICommandHandler<>).MakeGenericType(commandType);
        dynamic handler = _serviceProvider.GetRequiredService(handlerType);
        
        await handler.HandleAsync((dynamic)command, cancellationToken);
    }

    public async Task<TResult> SendAsync<TResult>(ICommand<TResult> command, CancellationToken cancellationToken = default)
    {
        var commandType = command.GetType();
        var handlerType = typeof(ICommandHandler<,>).MakeGenericType(commandType, typeof(TResult));
        dynamic handler = _serviceProvider.GetRequiredService(handlerType);
        
        return await handler.HandleAsync((dynamic)command, cancellationToken);
    }

    public async Task<TResult> SendAsync<TResult>(IQuery<TResult> query, CancellationToken cancellationToken = default)
    {
        var queryType = query.GetType();
        var handlerType = typeof(IQueryHandler<,>).MakeGenericType(queryType, typeof(TResult));
        dynamic handler = _serviceProvider.GetRequiredService(handlerType);
        
        return await handler.HandleAsync((dynamic)query, cancellationToken);
    }
}