using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;

namespace Compartilhado.Seguranca;

/// <summary>
/// Serviço responsável por gerar tokens JWT.
/// </summary>
public class ServicoJwt
{
    private readonly ConfiguracoesJwt _config;

    public ServicoJwt(IOptions<ConfiguracoesJwt> opcoes)
    {
        _config = opcoes.Value;
    }

    /// <summary>
    /// Gera um token JWT simples para demonstração.
    /// </summary>
    /// <param name="usuario">Nome do usuário autenticado</param>
    /// <param name="perfis">Perfis/roles do usuário</param>
    public string GerarToken(string usuario, IEnumerable<string>? perfis = null)
    {
        var chave = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_config.Chave ?? "chave-dev"));
        var credenciais = new SigningCredentials(chave, SecurityAlgorithms.HmacSha256);

        var claims = new List<Claim>
        {
            new(ClaimTypes.Name, usuario)
        };

        if (perfis != null)
        {
            claims.AddRange(perfis.Select(p => new Claim(ClaimTypes.Role, p)));
        }

        var token = new JwtSecurityToken(
            issuer: _config.Emissor,
            audience: _config.Audiencia,
            claims: claims,
            expires: DateTime.UtcNow.AddMinutes(_config.ExpiracaoMinutos <= 0 ? 60 : _config.ExpiracaoMinutos),
            signingCredentials: credenciais
        );

        return new JwtSecurityTokenHandler().WriteToken(token);
    }
}
