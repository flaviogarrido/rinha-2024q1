namespace Crebitos;

internal sealed class ConfiguracaoAmbiente
{
    public string Database { get; private set; }
    public string DatabaseHost { get; private set; }
    public string DatabasePort { get; private set; }
    public string DatabaseUser { get; private set; }
    public string DatabasePassword { get; private set; }
    public object DatabaseName { get; private set; }
    public object DatabaseMinPoolSize { get; private set; }
    public object DatabaseMaxPoolSize { get; private set; }

    private static ConfiguracaoAmbiente _instance;
    private static readonly object _lockObject = new object();


    public ConfiguracaoAmbiente()
    {
        Database = Environment.GetEnvironmentVariable("DATABASE") ?? "postgresql-logica-na-aplicacao";
        DatabaseHost = Environment.GetEnvironmentVariable("DATABASE_HOST") ?? "localhost";
        DatabasePort = Environment.GetEnvironmentVariable("DATABASE_PORT") ?? "5432";
        DatabaseUser = Environment.GetEnvironmentVariable("DATABASE_USER") ?? "postgres";
        DatabasePassword = Environment.GetEnvironmentVariable("DATABASE_PASSWORD") ?? "postgres";
        DatabaseName = Environment.GetEnvironmentVariable("DATABASE_NAME") ?? "rinha2024q1";
        DatabaseMinPoolSize = Environment.GetEnvironmentVariable("DATABASE_MIX_POOL_SIZE") ?? "10";
        DatabaseMaxPoolSize = Environment.GetEnvironmentVariable("DATABASE_MAX_POOL_SIZE") ?? "10";
    }


    public static ConfiguracaoAmbiente GetInstance()
    {
        if (_instance == null)
            lock (_lockObject)
                if (_instance == null)
                    _instance = new ConfiguracaoAmbiente();

        return _instance;
    }

}
