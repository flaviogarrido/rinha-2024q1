using System.Text.Json.Serialization;

namespace Crebitos;

public class ClienteContratos
{
    public static IResult Valida(TransacaoRequest transacao)
    {
        if (transacao.Valor <= 0)
            return Results.BadRequest("O valor deve ser um número inteiro positivo.");

        if (transacao.Tipo != 'c' && transacao.Tipo != 'd')
            return Results.BadRequest("O tipo deve ser 'c' para crédito ou 'd' para débito.");

        if (string.IsNullOrEmpty(transacao.Descricao) || transacao.Descricao.Length > 10)
            return Results.BadRequest("A descrição deve ter entre 1 e 10 caracteres.");

        return Results.Ok();
    }
}


[JsonSerializable(typeof(TransacaoRequest))]
[JsonSerializable(typeof(TransacaoResponse))]
[JsonSerializable(typeof(ExtratoResponse))]
[JsonSerializable(typeof(SaldoResponse))]
[JsonSerializable(typeof(IList<ListaTransacoesResponse>))]
[JsonSerializable(typeof(ILogger))]
public partial class AppJsonSerializerContext : JsonSerializerContext { }

public record TransacaoRequest(int Valor, char Tipo, string Descricao);

public record TransacaoResponse(int Saldo, int Limite);

public record ExtratoResponse(SaldoResponse Saldo, IList<ListaTransacoesResponse> Ultimas_transacoes);

public record SaldoResponse(DateTime Data_extrato, int Total, int Limite);

public record ListaTransacoesResponse(DateTime Realizada_em, string Descricao, int Valor, char Tipo);
