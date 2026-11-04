using System.Net;
using System.Text.Json;
using FCG.Catalog.Application.Exceptions;

namespace FCG.Catalog.API.Middlewares;

public class ErrorHandlingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<ErrorHandlingMiddleware> _logger;

    public ErrorHandlingMiddleware(RequestDelegate next, ILogger<ErrorHandlingMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (KeyNotFoundException ex)
        {
            _logger.LogWarning(ex, "Recurso não encontrado: {Message}", ex.Message);
            await EscreverRespostaErro(context, HttpStatusCode.NotFound, ex.Message);
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogWarning(ex, "Operação inválida: {Message}", ex.Message);
            await EscreverRespostaErro(context, HttpStatusCode.BadRequest, ex.Message);
        }
        catch (ArgumentException ex)
        {
            _logger.LogWarning(ex, "Argumento inválido: {Message}", ex.Message);
            await EscreverRespostaErro(context, HttpStatusCode.BadRequest, ex.Message);
        }
        catch (UnauthorizedAccessException ex)
        {
            _logger.LogWarning(ex, "Acesso não autorizado: {Message}", ex.Message);
            await EscreverRespostaErro(context, HttpStatusCode.Unauthorized, ex.Message);
        }
        catch (BuscaIndisponivelException ex)
        {
            _logger.LogError(ex, "Motor de busca indisponível: {Message}", ex.Message);
            await EscreverRespostaErro(context, HttpStatusCode.ServiceUnavailable, "Serviço de busca temporariamente indisponível.");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro interno não tratado.");
            await EscreverRespostaErro(context, HttpStatusCode.InternalServerError, "Ocorreu um erro interno. Tente novamente mais tarde.");
        }
    }

    private static async Task EscreverRespostaErro(HttpContext context, HttpStatusCode statusCode, string mensagem)
    {
        context.Response.ContentType = "application/json";
        context.Response.StatusCode = (int)statusCode;

        var resposta = JsonSerializer.Serialize(new
        {
            status = (int)statusCode,
            mensagem,
            timestamp = DateTime.UtcNow
        });

        await context.Response.WriteAsync(resposta);
    }
}
