using System;
using CodeBasics.Command.Implementation;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace CodeBasics.Command
{
  public static class CommandServiceExtensions
  {
    public static IServiceCollection AddCommand(this IServiceCollection services, Action<CommandOptions> configure = null)
    {
      services.AddLogging();

      var optionsBuilder = services.AddOptions<CommandOptions>()
                                   .Configure(CommandOptions.DefaultSettings);

      if (configure != null)
      {
        optionsBuilder.Configure(configure);
      }

      services.TryAddTransient<ICommandFactory, CommandFactory>();
      services.TryAddSingleton(typeof(IValidator<>), typeof(DataAnnotationsValidator<>));
      services.TryAddSingleton(typeof(IInputValidator<>), typeof(DataAnnotationsValidator<>));
      services.TryAddSingleton(typeof(IOutputValidator<>), typeof(DataAnnotationsValidator<>));

      return services;
    }
  }
}
