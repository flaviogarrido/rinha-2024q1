using Npgsql;
using System.Data.Common;

namespace Crebitos;

internal static class ConfigureDatabaseExtensions
{
    public static IServiceCollection ConfigureDatabase(this IServiceCollection serviceCollection)
    {
        var configAmbiente = ConfiguracaoAmbiente.GetInstance();

        switch (configAmbiente.Database.ToLowerInvariant())
        {
            case "common_postgres":
                serviceCollection.AddSingleton<DbConnection>(new NpgsqlConnection(
                    $"Host={configAmbiente.DatabaseHost};" +
                    $"Username={configAmbiente.DatabaseUser};" +
                    $"Password={configAmbiente.DatabasePassword};" +
                    $"Database={configAmbiente.DatabaseName};" +
                    $"Minimum Pool Size={configAmbiente.DatabaseMinPoolSize};" +
                    $"Maximum Pool Size={configAmbiente.DatabaseMaxPoolSize};" +
                    $"Multiplexing=true;"));
                break;

            case "postgresql-logica-no-banco":
            case "postgres-logica-no-banco":
                serviceCollection.AddSingleton<IClienteRepositorio>(new ClienteRepositorioLogicaDatabase());
                goto case "common_postgres";


            case "postgresql-logica-na-aplicacao":
            case "postgres-logica-na-aplicacao":
                serviceCollection.AddSingleton<IClienteRepositorio>(new ClienteRepositorioLogicaAplicacao());
                goto case "common_postgres";


            default:
                throw new InvalidOperationException("Invalid database configuration");
        }

        return serviceCollection;
    }
}
