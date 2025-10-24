using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ServicoEstoque.Controladores;

/// <summary>
/// Controlador para verificação de saúde do Serviço de Estoque.
/// Fornece um endpoint simples para monitoramento e health checks.
/// </summary>
[ApiController]
[Route("healthz")]
[AllowAnonymous]
public class SaudeController : ControllerBase
{
    /// <summary>
    /// Retorna o status de saúde do serviço.
    /// Endpoint usado por ferramentas de monitoramento para verificar se o serviço de estoque está operacional.
    /// </summary>
    /// <returns>Status "ok" se o serviço estiver saudável.</returns>
    [HttpGet]
    public IActionResult Get() => Ok(new { status = "ok" });
}
