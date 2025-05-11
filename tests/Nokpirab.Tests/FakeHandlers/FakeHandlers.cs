namespace Nokpirab.Tests.FakeHandlers;

public record CreateWithoutReturningResultCommand : ICommand;
	
public class CreateWithoutReturningResultCommandHandler : ICommandHandler<CreateWithoutReturningResultCommand>
{
	public Task HandleAsync(CreateWithoutReturningResultCommand command, CancellationToken cancellationToken = default)
	{
		return Task.CompletedTask;
	}
}

public record CreateStringResultCommand : ICommand<string>;

public class CreateStringResultCommandHandler : ICommandHandler<CreateStringResultCommand, string>
{
	public Task<string> HandleAsync(CreateStringResultCommand command, CancellationToken cancellationToken = default)
	{
		return Task.FromResult("Success");
	}
}

public record GetStringResultQuery : IQuery<string>;

public class GetStringResultQueryHandler : IQueryHandler<GetStringResultQuery, string>
{
	public Task<string> HandleAsync(GetStringResultQuery query, CancellationToken cancellationToken = default)
	{
		return Task.FromResult("Success");
	}
}