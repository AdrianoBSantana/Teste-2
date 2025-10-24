using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ApiGateway.Controladores;

/// <summary>
/// Controlador para verificação de saúde do API Gateway.
/// Fornece um endpoint simples para monitoramento e health checks.
/// </summary>
[ApiController]
[Route("healthz")]
[AllowAnonymous]
public class SaudeController : ControllerBase
{
    /// <summary>
    /// Retorna o status de saúde do serviço.
    /// Endpoint usado por ferramentas de monitoramento para verificar se o gateway está operacional.
    /// </summary>
    /// <returns>Status "ok" se o serviço estiver saudável.</returns>
    [HttpGet]
    public IActionResult Get() => Ok(new { status = "ok" });
}
