using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;

namespace ServicoVendas.Servicos;

/// <summary>
/// Cliente HTTP para consultar o serviço de estoque.
/// </summary>
public class ClienteEstoque : IClienteEstoque
{
    private readonly HttpClient _http;

    public ClienteEstoque(HttpClient http)
    {
        _http = http;
    }

    public record ProdutoEstoqueDto(Guid Id, string Nome, string? Descricao, decimal Preco, int QuantidadeEmEstoque);

    public async Task<ProdutoEstoqueDto?> ObterProdutoAsync(Guid produtoId, string? bearerToken, CancellationToken ct)
    {
        using var req = new HttpRequestMessage(HttpMethod.Get, $"estoque/produtos/{produtoId}");
        if (!string.IsNullOrWhiteSpace(bearerToken))
        {
            req.Headers.Authorization = new AuthenticationHeaderValue("Bearer", bearerToken);
        }

        var resp = await _http.SendAsync(req, ct);

        if (resp.StatusCode == HttpStatusCode.NotFound)
            return null;

        resp.EnsureSuccessStatusCode();
        var dto = await resp.Content.ReadFromJsonAsync<ProdutoEstoqueDto>(cancellationToken: ct);
        return dto;
    }
}
