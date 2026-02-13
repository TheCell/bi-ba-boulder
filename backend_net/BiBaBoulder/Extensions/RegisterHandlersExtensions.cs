using System.Linq;
using System.Reflection;
using Microsoft.Extensions.DependencyInjection;
using Thecell.Bibaboulder.Common.Commands;
using Thecell.Bibaboulder.Common.Queries;
using Thecell.Bibaboulder.Outdoor.Handler;
using Thecell.Bibaboulder.Spraywall.Testing;

namespace Thecell.Bibaboulder.BiBaBoulder.Extensions;

public static class RegisterHandlersExtensions
{
    public static void RegisterCqrsAndControllerAssemblies(this IServiceCollection services)
    {
        services.AddControllers().AddApplicationPart(typeof(Controllers.SectorsController).Assembly);
        services.AddCqrsHandlers([typeof(Program).Assembly, typeof(GetTestingQueryHandler).Assembly]);
        services.AddCqrsHandlers([typeof(Program).Assembly, typeof(GetSectorQueryHandler).Assembly]);
    }

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
