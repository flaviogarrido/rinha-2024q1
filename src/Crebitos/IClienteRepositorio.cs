using System.Data.Common;

namespace Crebitos;

internal interface IClienteRepositorio
{
    ValueTask<IResult> MovimentarContaAsync(
        int id,
        TransacaoRequest transacao,
        DbConnection conn,
        CancellationToken ct);


    ValueTask<IResult> ConsultarExtratoAsync(
        int id,
        DbConnection conn,
        CancellationToken ct);
}
