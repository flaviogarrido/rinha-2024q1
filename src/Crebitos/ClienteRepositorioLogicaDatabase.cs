using System.Data.Common;

namespace Crebitos;

internal class ClienteRepositorioLogicaDatabase : IClienteRepositorio
{
    public ValueTask<IResult> 
        ConsultarExtratoAsync(
            int id, DbConnection conn, 
            CancellationToken ct)
    {
        throw new NotImplementedException();
    }

    public ValueTask<IResult> 
        MovimentarContaAsync(
            int id, 
            TransacaoRequest transacao, 
            DbConnection conn, 
            CancellationToken ct)
    {
        throw new NotImplementedException();
    }
}