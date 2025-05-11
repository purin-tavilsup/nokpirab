using System.Reflection;
using AutoFixture;
using AutoFixture.AutoNSubstitute;
using AutoFixture.Xunit2;
using Microsoft.Extensions.DependencyInjection;
using Nokpirab.Tests.FakeHandlers;
using Shouldly;

namespace Nokpirab.Tests;

public class ServiceCollectionExtensionsTests
{
	[Theory, AutoNSubstituteData]
	public void AddNokpirab_ShouldRegisterINokpirab(IServiceCollection sut)
	{
		// Act
		sut.AddNokpirabFromAssembly(Assembly.GetExecutingAssembly());
		
		var provider = sut.BuildServiceProvider();

		// Assert
		var nokpirab = provider.GetRequiredService<INokpirab>();
		nokpirab.ShouldNotBeNull();
	}
	
	[Theory]
	[InlineAutoNSubstituteData(typeof(ICommandHandler<CreateWithoutReturningResultCommand>), typeof(CreateWithoutReturningResultCommandHandler))]
	[InlineAutoNSubstituteData(typeof(ICommandHandler<CreateStringResultCommand, string>), typeof(CreateStringResultCommandHandler))]
	[InlineAutoNSubstituteData(typeof(IQueryHandler<GetStringResultQuery, string>), typeof(GetStringResultQueryHandler))]
	public void AddNokpirab_ShouldRegisterCorrectHandlers(Type handlerInterface, 
														  Type expectedHandlerImplementation, 
														  IServiceCollection sut)
	{
		// Act
		sut.AddNokpirabFromAssembly(Assembly.GetExecutingAssembly());
		
		var provider = sut.BuildServiceProvider();

		// Assert
		var handler = provider.GetRequiredService(handlerInterface);
		handler.ShouldNotBeNull();
		handler.ShouldBeOfType(expectedHandlerImplementation);
	}
	
	private class AutoNSubstituteDataAttribute : AutoDataAttribute
	{
		public AutoNSubstituteDataAttribute() 
			: base(() => new Fixture().Customize(new Customization()))
		{
		}
	}

	private class InlineAutoNSubstituteDataAttribute : CompositeDataAttribute
	{
		public InlineAutoNSubstituteDataAttribute(params object[] values)
			: base(new InlineDataAttribute(values), new AutoNSubstituteDataAttribute())
		{
		}
	}

	private class Customization : ICustomization
	{
		public void Customize(IFixture fixture)
		{
			fixture.Customize(new AutoNSubstituteCustomization());
			
			IServiceCollection services = new ServiceCollection();
			
			fixture.Inject(services);
		}
	}
}