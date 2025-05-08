using AutoFixture;
using AutoFixture.AutoNSubstitute;
using AutoFixture.Xunit2;
using Microsoft.Extensions.DependencyInjection;
using Nokpirab.Tests.FakeHandlers;
using NSubstitute;
using Shouldly;

namespace Nokpirab.Tests;

public class NokpirabTests
{
	[Theory, AutoNSubstituteData]
	public async Task SendAsync_CommandWithoutReturningResult_ShouldInvokeHandler(
		CreateWithoutReturningResultCommand command,
		ICommandHandler<CreateWithoutReturningResultCommand> handler,
		IServiceProvider serviceProvider)
	{
		// Arrange
		handler.HandleAsync(command, Arg.Any<CancellationToken>())
			   .Returns(Task.CompletedTask);
		
		var sut = new Nokpirab(serviceProvider);

		// Act
		await sut.SendAsync(command);

		// Assert
		await handler.Received(1).HandleAsync(command, Arg.Any<CancellationToken>());
	}

	[Theory, AutoNSubstituteData]
	public async Task SendAsync_CommandWithReturningResult_ShouldInvokeHandler(
		CreateStringResultCommand command, 
		ICommandHandler<CreateStringResultCommand, string> handler,
		string expectedResult,
		IServiceProvider serviceProvider)
	{
		// Arrange
		handler.HandleAsync(command, Arg.Any<CancellationToken>())
			   .Returns(Task.FromResult(expectedResult));
		
		var sut = new Nokpirab(serviceProvider);

		// Act
		_ = await sut.SendAsync(command);

		// Assert
		await handler.Received(1).HandleAsync(command, Arg.Any<CancellationToken>());
	}
	
	[Theory, AutoNSubstituteData]
	public async Task SendAsync_CommandWithReturningResult_ShouldReturnExpectedResult(
		CreateStringResultCommand command, 
		ICommandHandler<CreateStringResultCommand, string> handler,
		string expectedResult,
		IServiceProvider serviceProvider)
	{
		// Arrange
		handler.HandleAsync(command, Arg.Any<CancellationToken>())
			   .Returns(Task.FromResult(expectedResult));
		
		var sut = new Nokpirab(serviceProvider);

		// Act
		var result = await sut.SendAsync(command);

		// Assert
		result.ShouldBe(expectedResult);
	}
	
	[Theory, AutoNSubstituteData]
	public async Task SendAsync_Query_ShouldInvokeHandler(
		GetStringResultQuery query, 
		IQueryHandler<GetStringResultQuery, string> handler,
		string expectedResult,
		IServiceProvider serviceProvider)
	{
		// Arrange
		handler.HandleAsync(query, Arg.Any<CancellationToken>())
			   .Returns(Task.FromResult(expectedResult));
		
		var sut = new Nokpirab(serviceProvider);

		// Act
		_ = await sut.SendAsync(query);

		// Assert
		await handler.Received(1).HandleAsync(query, Arg.Any<CancellationToken>());
	}
	
	[Theory, AutoNSubstituteData]
	public async Task SendAsync_Query_ShouldReturnExpectedResult(
		GetStringResultQuery query, 
		IQueryHandler<GetStringResultQuery, string> handler,
		string expectedResult,
		IServiceProvider serviceProvider)
	{
		// Arrange
		handler.HandleAsync(query, Arg.Any<CancellationToken>())
			   .Returns(Task.FromResult(expectedResult));
		
		var sut = new Nokpirab(serviceProvider);

		// Act
		var result = await sut.SendAsync(query);

		// Assert
		result.ShouldBe(expectedResult);
	}
	
	[Theory, AutoNSubstituteData]
	public async Task SendAsync_CommandWithoutReturningResult_HandlerNotFound_ShouldThrowHandlerNotFoundException(
		FakeCommandWithoutHandler command,
		IServiceProvider serviceProvider)
	{
		// Arrange
		var sut = new Nokpirab(serviceProvider);

		// Act & Assert
		var exception = await Should.ThrowAsync<HandlerNotFoundException>(() => sut.SendAsync(command));

		exception.Message.ShouldContain(nameof(FakeCommandWithoutHandler));
	}
	
	[Theory, AutoNSubstituteData]
	public async Task SendAsync_Query_HandlerNotFound_ShouldThrowHandlerNotFoundException(
		FakeQueryWithoutHandler query,
		IServiceProvider serviceProvider)
	{
		// Arrange
		var sut = new Nokpirab(serviceProvider);

		// Act & Assert
		var exception = await Should.ThrowAsync<HandlerNotFoundException>(() => sut.SendAsync(query));

		exception.Message.ShouldContain(nameof(FakeQueryWithoutHandler));
	}

	public record FakeCommandWithoutHandler : ICommand;
	
	public record FakeQueryWithoutHandler : IQuery<string>;

	private class AutoNSubstituteDataAttribute : AutoDataAttribute
	{
		public AutoNSubstituteDataAttribute() 
			: base(() => new Fixture().Customize(new Customization()))
		{
		}
	}

	private class Customization : ICustomization
	{
		public void Customize(IFixture fixture)
		{
			fixture.Customize(new AutoNSubstituteCustomization());
			
			var createWithoutReturningResultCommandHandler = fixture.Freeze<ICommandHandler<CreateWithoutReturningResultCommand>>();
			var createStringResultCommandHandler = fixture.Freeze<ICommandHandler<CreateStringResultCommand, string>>();
			var getStringResultQueryHandler = fixture.Freeze<IQueryHandler<GetStringResultQuery, string>>();
			
			var services = new ServiceCollection();
			services.AddSingleton(createWithoutReturningResultCommandHandler);
			services.AddSingleton(createStringResultCommandHandler);
			services.AddSingleton(getStringResultQueryHandler);

			IServiceProvider serviceProvider = services.BuildServiceProvider();
			
			fixture.Inject(serviceProvider);
		}
	}
}
