using Microsoft.Extensions.DependencyInjection;
using System.Linq;
using System.Reflection;
using Thecell.Bibaboulder.Common.Commands;
using Thecell.Bibaboulder.Common.Queries;

namespace Thecell.Bibaboulder.BiBaBoulder.Extensions;

public static class RegisterHandlersExtensions
{
    public static IServiceCollection AddCqrsHandlers(this IServiceCollection services, Assembly[] assemblies)
    {
        var queryHandlerType = typeof(IQueryHandler<,>);
        foreach (var type in assemblies.SelectMany(a => a.GetTypes()))
        {
            var interfaces = type.GetInterfaces()
                .Where(i => i.IsGenericType && i.GetGenericTypeDefinition() == queryHandlerType);

            foreach (var handlerInterface in interfaces)
            {
                services.AddScoped(handlerInterface, type);
            }
        }

        var commandHandlerType = typeof(ICommandHandler<>);
        foreach (var type in assemblies.SelectMany(a => a.GetTypes()))
        {
            var interfaces = type.GetInterfaces()
                .Where(i => i.IsGenericType && i.GetGenericTypeDefinition() == commandHandlerType);

            foreach (var handlerInterface in interfaces)
            {
                services.AddScoped(handlerInterface, type);
            }
        }

        return services;
    }
}
