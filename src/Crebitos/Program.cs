namespace Crebitos;
public class Program
{
    public static void Main(string[] args)
    {
        var builder = WebApplication.CreateSlimBuilder(args);

        builder.Services.ConfigureHttpJsonOptions(options =>
        {
            options.SerializerOptions.TypeInfoResolverChain.Insert(0, AppJsonSerializerContext.Default);
        });

        builder.Logging.AddConsole();

        builder.Services.ConfigureDatabase();

        var app = builder.Build();
        app.RegisterClienteEndpoints();
        app.Run();
    }

}
