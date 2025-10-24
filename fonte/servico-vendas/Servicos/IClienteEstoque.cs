namespace ServicoVendas.Servicos;

/// <summary>
/// Contrato do cliente HTTP para consultar o serviço de estoque.
/// Usado para facilitar testes (mock) e inversão de dependência.
/// </summary>
public interface IClienteEstoque
{
    Task<ClienteEstoque.ProdutoEstoqueDto?> ObterProdutoAsync(Guid produtoId, string? bearerToken, CancellationToken ct);
}
