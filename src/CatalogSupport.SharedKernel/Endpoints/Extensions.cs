﻿using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace CatalogSupport.SharedKernel.Endpoints;

public static class Extensions
{
    public static IServiceCollection AddEndpoints(this IServiceCollection services, Type type)
    {
        var serviceDescriptors = type
            .Assembly.DefinedTypes.Where(typeInfo =>
                typeInfo is { IsAbstract: false, IsInterface: false }
                && typeInfo.IsAssignableTo(typeof(IEndpoint))
            )
            .Select(implementationType =>
                ServiceDescriptor.Transient(typeof(IEndpoint), implementationType)
            )
            .ToArray();

        services.TryAddEnumerable(serviceDescriptors);

        return services;
    }

    public static IApplicationBuilder MapEndpoints(this WebApplication app)
    {
        var scope = app.Services.CreateScope();

        var endpoints = scope.ServiceProvider.GetRequiredService<IEnumerable<IEndpoint>>();

        IEndpointRouteBuilder builder = app.MapGroup("/api");

        foreach (var endpoint in endpoints)
        {
            endpoint.MapEndpoint(builder);
        }

        return app;
    }
}
