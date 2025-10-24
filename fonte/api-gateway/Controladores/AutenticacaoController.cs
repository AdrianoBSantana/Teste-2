using Compartilhado.Seguranca;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ApiGateway.Controladores;

/// <summary>
/// Controlador responsável por autenticar usuários e emitir tokens JWT (apenas para demonstração).
/// </summary>
[ApiController]
[Route("autenticacao")]
public class AutenticacaoController : ControllerBase
{
    private readonly ServicoJwt _servicoJwt;

    public AutenticacaoController(ServicoJwt servicoJwt)
    {
        _servicoJwt = servicoJwt;
    }

    /// <summary>
    /// Emite um token JWT simples a partir de um usuário/senha estáticos (apenas DEV).
    /// </summary>
    [HttpPost("token")]
    [AllowAnonymous]
    public ActionResult<object> GerarToken([FromBody] RequisicaoLogin requisicao)
    {
        // Validação extremamente simples apenas para demonstração
        if (string.Equals(requisicao.Usuario, "admin", StringComparison.OrdinalIgnoreCase)
            && requisicao.Senha == "admin")
        {
            var token = _servicoJwt.GerarToken(requisicao.Usuario, new[] { "usuario" });
            return Ok(new { token });
        }

        return Unauthorized(new { mensagem = "Usuário ou senha inválidos." });
    }
}

/// <summary>
/// Modelo da requisição de login.
/// </summary>
public class RequisicaoLogin
{
    /// <summary>
    /// Nome do usuário.
    /// </summary>
    public string Usuario { get; set; } = string.Empty;

    /// <summary>
    /// Senha do usuário.
    /// </summary>
    public string Senha { get; set; } = string.Empty;
}
