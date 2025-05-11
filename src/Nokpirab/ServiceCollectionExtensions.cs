using System.Reflection;
using Microsoft.Extensions.DependencyInjection;

namespace Nokpirab;

public static class ServiceCollectionExtensions
{
	public static IServiceCollection AddNokpirabFromAssembly(this IServiceCollection services, Assembly assembly)
	{
		services.AddTransient<INokpirab, Nokpirab>();

		var allTypes = assembly.GetTypes().ToList();

		RegisterHandlers(services, allTypes, typeof(ICommandHandler<>));
		RegisterHandlers(services, allTypes, typeof(ICommandHandler<,>));
		RegisterHandlers(services, allTypes, typeof(IQueryHandler<,>));

		return services;
	}

	private static void RegisterHandlers(IServiceCollection services, List<Type> allTypes, Type handlerInterfaceType)
	{
		var handlers = allTypes.Where(t => t.GetInterfaces().Any(i => i.IsGenericType && i.GetGenericTypeDefinition() == handlerInterfaceType));

		foreach (var handler in handlers)
		{
			var interfaces = handler.GetInterfaces()
									.Where(i => i.IsGenericType && i.GetGenericTypeDefinition() == handlerInterfaceType);

			foreach (var @interface in interfaces)
			{
				services.AddTransient(@interface, handler);
			}
		}
	}
}