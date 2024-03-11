using Npgsql;
using System.Data.Common;

namespace Crebitos;

internal sealed class ClienteRepositorioLogicaAplicacao : IClienteRepositorio
{

    public async ValueTask<IResult> 
        ConsultarExtratoAsync(
            int id,
            DbConnection connection,
            CancellationToken ct)
    {
        var saldo = await ObterSaldoPorClienteIdAsync(id, connection, ct);
        var transacoes = await ObterUltimasTransacoesPorClienteIdAsync(id, connection, ct);
        return Results.Ok(new ExtratoResponse(saldo, transacoes));
    }


    private async ValueTask<SaldoResponse>
        ObterSaldoPorClienteIdAsync(
            int id,
            DbConnection conn,
            CancellationToken ct)
    {
        using var connection = new NpgsqlConnection(conn.ConnectionString);
        await connection.OpenAsync(ct);

        using var cmd = connection.CreateCommand();
        cmd.CommandText = "select valor, limite from saldos where cliente_id = $1";
        cmd.Parameters.AddWithValue(id);

        using var reader = await cmd.ExecuteReaderAsync(ct);
        await reader.ReadAsync(ct);
        var dataSaldo = DateTime.Now;
        var saldo = reader.GetInt32(0);
        var limite = reader.GetInt32(1);

        reader.Close();
        connection.Close();

        return new(dataSaldo, saldo, limite);
}

    private async ValueTask<IList<ListaTransacoesResponse>>
        ObterUltimasTransacoesPorClienteIdAsync(
            int id,
            DbConnection conn,
            CancellationToken ct)
    {
        using var connection = new NpgsqlConnection(conn.ConnectionString);
        await connection.OpenAsync(ct);

        using var cmd = connection.CreateCommand();
        cmd.CommandText =
            @"select realizada_em, descricao, valor, tipo
                from transacoes
                where cliente_id = $1 
                order by id desc 
                limit 10";
        cmd.Parameters.AddWithValue(id);

        using var reader = await cmd.ExecuteReaderAsync(ct);

        IList<ListaTransacoesResponse> transacoes = new List<ListaTransacoesResponse>();

        while (await reader.ReadAsync(ct))
            transacoes.Add(
                new(reader.GetDateTime(0),
                    reader.GetString(1),
                    reader.GetInt32(2),
                    reader.GetChar(3)));

        reader.Close();
        connection.Close();

        return transacoes;
    }






    public async ValueTask<IResult> MovimentarContaAsync(
        int id,
        TransacaoRequest transacao,
        DbConnection conn,
        CancellationToken ct)
    {
        if (transacao.Tipo == 'c')
            return await CashInAsync(
                id,
                transacao,
                conn,
                ct);
        else
            return await CashOutAsync(
                id,
                transacao,
                conn,
                ct);
    }

    private async ValueTask<IResult> CashInAsync(
        int id,
        TransacaoRequest transacao,
        DbConnection conn,
        CancellationToken ct)
    {
        using var connection = new NpgsqlConnection(conn.ConnectionString);
        await connection.OpenAsync(ct);
        using var transaction = await connection.BeginTransactionAsync(ct);

        using var cmdSaldo = connection.CreateCommand();
        cmdSaldo.CommandText =
            @"UPDATE saldos
                SET valor = valor + @valor
                WHERE cliente_id = @cliente_id
                RETURNING valor, limite;";

        cmdSaldo.Parameters.AddWithValue("@cliente_id", id);
        cmdSaldo.Parameters.AddWithValue("@valor", transacao.Valor);

        using var resultSaldo = await cmdSaldo.ExecuteReaderAsync(ct);

        if (await resultSaldo.ReadAsync(ct))
        {
            var saldo = resultSaldo.GetInt32(0);
            var limite = resultSaldo.GetInt32(1);
            resultSaldo.Close();

            using var cmdMovimento = connection.CreateCommand();
            cmdMovimento.CommandText =
                @"INSERT INTO transacoes
                  VALUES(DEFAULT, @cliente_id, @valor, 'c', @descricao, NOW());";
            cmdMovimento.Parameters.AddWithValue("@cliente_id", id);
            cmdMovimento.Parameters.AddWithValue("@valor", transacao.Valor);
            cmdMovimento.Parameters.AddWithValue("@descricao", transacao.Descricao);

            var resultMovimento = await cmdMovimento.ExecuteNonQueryAsync(ct);

            if (resultMovimento == 1)
            {
                await transaction.CommitAsync(ct);
                return Results.Ok(new TransacaoResponse(saldo, limite));
            }
            else
            {
                await transaction.RollbackAsync(ct);
                return Results.Problem("Credito não realizado");
            }
        }
        else
        {
            await transaction.RollbackAsync(ct);
            return Results.Problem("Credito não realizado");
        }
    }



    private async ValueTask<IResult> CashOutAsync(
        int id,
        TransacaoRequest transacao,
        DbConnection conn,
        CancellationToken ct)
    {
        using var connection = new NpgsqlConnection(conn.ConnectionString);
        await connection.OpenAsync(ct);
        using var transaction = await connection.BeginTransactionAsync(ct);

        using var cmdSaldo = connection.CreateCommand();
        cmdSaldo.CommandText = @"
            UPDATE saldos
            SET valor = CASE
                            WHEN valor - @valor >= -limite THEN valor - @valor
                            ELSE valor
                        END
            WHERE id = @cliente_id
            RETURNING valor, 
                      limite, 
                      (CASE WHEN valor - @valor >= -limite THEN true ELSE false END) as alterado;";

        cmdSaldo.Parameters.AddWithValue("@cliente_id", id);
        cmdSaldo.Parameters.AddWithValue("@valor", transacao.Valor);

        using var resultSaldo = await cmdSaldo.ExecuteReaderAsync(ct);

        if (await resultSaldo.ReadAsync(ct))
        {
            var saldo = resultSaldo.GetInt32(0);
            var limite = resultSaldo.GetInt32(1);
            var alterado = resultSaldo.GetBoolean(2);
            resultSaldo.Close();

            if (!alterado)
            {
                await transaction.RollbackAsync(ct);
                return Results.UnprocessableEntity("Débito não realizado, saldo insuficiente");
            }

            using var cmdMovimento = connection.CreateCommand();
            cmdMovimento.CommandText =
                @"INSERT INTO transacoes
                  VALUES(DEFAULT, @cliente_id, @valor, 'd', @descricao, NOW());";
            cmdMovimento.Parameters.AddWithValue("@cliente_id", id);
            cmdMovimento.Parameters.AddWithValue("@valor", transacao.Valor);
            cmdMovimento.Parameters.AddWithValue("@descricao", transacao.Descricao);

            var resultMovimento = await cmdMovimento.ExecuteNonQueryAsync(ct);

            if (resultMovimento == 1)
            {
                await transaction.CommitAsync(ct);
                return Results.Ok(new TransacaoResponse(saldo, limite));
            }
            else
            {
                await transaction.RollbackAsync(ct);
                return Results.Problem("Débito não realizado");
            }
        }
        else
        {
            await transaction.RollbackAsync(ct);
            return Results.Problem("Débito não realizado");
        }
    }
}
