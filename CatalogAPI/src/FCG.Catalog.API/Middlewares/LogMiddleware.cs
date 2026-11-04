using System.Diagnostics;

namespace FCG.Catalog.API.Middlewares;

public class LogMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<LogMiddleware> _logger;

    public LogMiddleware(RequestDelegate next, ILogger<LogMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        var sw = Stopwatch.StartNew();
        var requestId = Guid.NewGuid().ToString("N")[..8];

        _logger.LogInformation("[CATALOG] [{RequestId}] {Method} {Path}{QueryString} iniciado",
            requestId, context.Request.Method, context.Request.Path, context.Request.QueryString);

        // Tempo de processamento exposto ao cliente: evidencia o ganho do cache Redis (HIT x MISS).
        context.Response.OnStarting(() =>
        {
            context.Response.Headers["X-Response-Time-Ms"] = sw.Elapsed.TotalMilliseconds.ToString("F1", System.Globalization.CultureInfo.InvariantCulture);
            return Task.CompletedTask;
        });

        await _next(context);

        sw.Stop();
        _logger.LogInformation("[CATALOG] [{RequestId}] {Method} {Path} → {StatusCode} em {ElapsedMs}ms",
            requestId, context.Request.Method, context.Request.Path, context.Response.StatusCode, sw.ElapsedMilliseconds);
    }
}
