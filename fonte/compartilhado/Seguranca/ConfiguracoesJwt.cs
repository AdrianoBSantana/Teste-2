namespace Compartilhado.Seguranca;

/// <summary>
/// Configurações para emissão e validação de tokens JWT.
/// Os valores são lidos da seção "Jwt" no appsettings.json.
/// </summary>
public class ConfiguracoesJwt
{
    /// <summary>
    /// Emissor do token (issuer)
    /// </summary>
    public string? Emissor { get; set; }

    /// <summary>
    /// Audiência do token (audience)
    /// </summary>
    public string? Audiencia { get; set; }

    /// <summary>
    /// Chave secreta simétrica para assinar o token
    /// </summary>
    public string? Chave { get; set; }

    /// <summary>
    /// Tempo de expiração do token em minutos
    /// </summary>
    public int ExpiracaoMinutos { get; set; } = 60;
}
