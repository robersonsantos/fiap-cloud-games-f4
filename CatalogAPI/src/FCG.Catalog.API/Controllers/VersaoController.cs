using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FCG.Catalog.API.Controllers;

/// <summary>Identifica a versão em execução (usado na demonstração do Live Deploy).</summary>
[ApiController]
[Route("api/catalogo/versao")]
[Produces("application/json")]
public class VersaoController : ControllerBase
{
    // Altere esta mensagem, faça push e veja a pipeline publicar a nova versão sem downtime.
    private const string Mensagem = "Live Deploy do Grupo 94 funcionando!";

    private readonly IConfiguration _configuration;

    public VersaoController(IConfiguration configuration) => _configuration = configuration;

    [HttpGet]
    [AllowAnonymous]
    public IActionResult Obter() => Ok(new
    {
        servico = "catalog-api",
        mensagem = Mensagem,
        imagem = _configuration["APP_VERSION"] ?? "local",
        pod = Environment.MachineName,
        ambiente = _configuration["ASPNETCORE_ENVIRONMENT"] ?? "Production"
    });
}
