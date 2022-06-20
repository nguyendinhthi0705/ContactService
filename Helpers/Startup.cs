using Microsoft.Extensions.Diagnostics.HealthChecks;

[assembly: FunctionsStartup(typeof(Startup))]
namespace ContactService.Helpers
{
    public class Startup : FunctionsStartup
    {
        public override void ConfigureAppConfiguration(IFunctionsConfigurationBuilder builder)
        {
            if (builder == null) throw new ArgumentNullException(nameof(builder));

            var context = builder.GetContext();

            builder.ConfigurationBuilder
                .AddAppsettingsFile(context)
                .AddEnvironmentVariables();
        }
        public override void Configure(IFunctionsHostBuilder builder)
        {
            builder.Services.AddLogging(loggingBuilder =>
            {
                loggingBuilder.AddFilter(level => true);
            });

            var connectionString = builder.GetContext().Configuration[Settings.COSMOS_DB_CONNECTION_STRING];

            builder.AddSwashBuckle(Assembly.GetExecutingAssembly());
            builder.Services.AddSingleton((s) =>
            {
                CosmosClientBuilder cosmosClientBuilder = new CosmosClientBuilder(connectionString);
                return cosmosClientBuilder.WithConnectionModeDirect()
                    .Build();
            });

            builder.Services
                .AddHealthChecks()
                .AddCosmosDbCollection(connectionString);

        }
    }

    public static class ConfigurationBuilderExtensions
    {
        public static IConfigurationBuilder AddAppsettingsFile(
            this IConfigurationBuilder configurationBuilder,
            FunctionsHostBuilderContext context,
            bool useEnvironment = false
        )
        {
            if (context == null) throw new ArgumentNullException(nameof(context));

            var environmentSection = string.Empty;

            if (useEnvironment)
            {
                environmentSection = $".{context.EnvironmentName}";
            }

            configurationBuilder.AddJsonFile(
                path: Path.Combine(context.ApplicationRootPath, "appsettings.json"),
                optional: true,
                reloadOnChange: false);

            return configurationBuilder;
        }
    }
}


