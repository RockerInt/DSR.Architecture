using Dsr.Architecture.Application.UseCases;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Dsr.Architecture.Application;

public static class DependencyInjection
{
    /// <summary>
    /// Add to services for MediatR by Dependecy Injection
    /// </summary>
    /// <param name="services"></param>
    /// <returns></returns>
    public static IServiceCollection AddArchitectureApplication(this IServiceCollection services)
        => services.AddMediatR(config =>
        {
            config.RegisterServicesFromAssembly(typeof(UseCaseBehavior<,>).Assembly);
            config.AddOpenBehavior(typeof(UseCaseBehavior<,>));
        });
}
