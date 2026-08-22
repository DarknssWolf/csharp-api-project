using CSharpAPIProject.Application.Common.Behaviors;
using System.Reflection;

namespace Microsoft.Extensions.DependencyInjection;


public static class DependencyInjection
{
	public static IServiceCollection AddApiApplicationServices(this IServiceCollection services)
	{
		// Scan the assembly for Automapper Profiles
		// https://docs.automapper.org/en/stable/Configuration.html#profile-instances
		services.AddAutoMapper(Assembly.GetExecutingAssembly());

		// Scan the assembly for FluentValidation Validators
		services.AddValidatorsFromAssembly(Assembly.GetExecutingAssembly());

		// Register MediatR and behaviours
		services.AddMediatR(cfg =>
		{
			cfg.RegisterServicesFromAssembly(Assembly.GetExecutingAssembly());

			// To better understand behaviors in MediatR have a look at https://github.com/jbogard/MediatR/wiki/Behaviors
			// They work similar to how filters work in ASP.NET Core MVC
			cfg.AddBehavior(typeof(IPipelineBehavior<,>), typeof(AuthorizationBehaviour<,>));
			cfg.AddBehavior(typeof(IPipelineBehavior<,>), typeof(ValidationBehaviour<,>));
			cfg.AddBehavior(typeof(IPipelineBehavior<,>), typeof(PerformanceBehaviour<,>));
			cfg.AddBehavior(typeof(IPipelineBehavior<,>), typeof(ChangeLogBehaviour<,>));

            // The final behavior in the pipeline is the ConsumerTriggerBehaviour
            // So we can trigger anything that relies on the response of a command
            cfg.AddBehavior(typeof(IPipelineBehavior<,>), typeof(ConsumerTriggerBehaviour<,>));
        });

		return services;
	}
}

