using System.Data.Common;

namespace Crebitos;

public static class ClienteEndpoints
{
    public static void RegisterClienteEndpoints(this WebApplication app)
    {
        var routeGroup = app.MapGroup("/clientes");
        routeGroup.MapGet("{id:int}/extrato", ObterExtratoPorIdCliente);
        routeGroup.MapPost("{id:int}/transacoes", MovimentarContaPorIdCliente);
    }

    private static async Task<IResult> ObterExtratoPorIdCliente(
        int id,
        IClienteRepositorio repo,
        DbConnection conn,
        ILoggerFactory loggerFactory,
        CancellationToken ct)
    {
        var logger = loggerFactory.CreateLogger("Crebitos");
        logger.LogInformation($"Obtendo extrato do cliente {id}");

        // Verifica a existência do cliente
        if (id < 1 || id > 5)
            return Results.NotFound();

        // Obtém o extrato
        var result = await repo.ConsultarExtratoAsync(id, conn, ct);

        // Retorna os dados conforme contrato
        return result;
    }

    private static async Task<IResult> MovimentarContaPorIdCliente(
        int id,
        TransacaoRequest transacao,
        IClienteRepositorio repo,
        DbConnection conn,
        ILoggerFactory loggerFactory,
        CancellationToken ct)
    {
        var logger = loggerFactory.CreateLogger("Crebitos.ClienteEndpoints");
        logger.LogInformation($"Movimentando a conta do cliente {id}");

        // Aplica a regras de contrato sobre os dados de entrada
        var resultadoValidacao = ClienteContratos.Valida(transacao);
        if (resultadoValidacao != Results.Ok())
            return resultadoValidacao;

        // Verifica a existência do cliente
        if (id < 1 || id > 5)
            return Results.NotFound();

        // Registra a movimentação
        var result = await repo.MovimentarContaAsync(id, transacao, conn, ct);

        // Retorna os dados conforme contrato
        return result;

    }
}
