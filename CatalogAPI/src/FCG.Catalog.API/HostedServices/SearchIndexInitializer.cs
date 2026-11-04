using FCG.Catalog.Application.Interfaces;

namespace FCG.Catalog.API.HostedServices;

/// <summary>
/// Na subida do pod, garante que o índice exista e, se estiver vazio, popula-o
/// a partir do SQL Server. Roda em segundo plano para não atrasar o readiness.
/// </summary>
public class SearchIndexInitializer : BackgroundService
{
    private static readonly TimeSpan IntervaloTentativa = TimeSpan.FromSeconds(15);
    private const int MaxTentativas = 20;

    private readonly IServiceScopeFactory _scopeFactory;
    private readonly ILogger<SearchIndexInitializer> _logger;

    public SearchIndexInitializer(IServiceScopeFactory scopeFactory, ILogger<SearchIndexInitializer> logger)
    {
        _scopeFactory = scopeFactory;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        for (var tentativa = 1; tentativa <= MaxTentativas && !stoppingToken.IsCancellationRequested; tentativa++)
        {
            try
            {
                using var scope = _scopeFactory.CreateScope();
                var indice = scope.ServiceProvider.GetRequiredService<IJogoSearchIndex>();
                var busca = scope.ServiceProvider.GetRequiredService<IBuscaService>();

                await indice.GarantirIndiceAsync(stoppingToken);
                var total = await indice.ContarAsync(stoppingToken);

                if (total == 0)
                {
                    var resultado = await busca.ReindexarAsync(stoppingToken);
                    _logger.LogInformation("[CATALOG] [SEARCH] Índice inicial criado com {Total} jogo(s).", resultado.JogosIndexados);
                }
                else
                {
                    _logger.LogInformation("[CATALOG] [SEARCH] Índice já possui {Total} documento(s).", total);
                }

                return;
            }
            catch (Exception ex) when (!stoppingToken.IsCancellationRequested)
            {
                _logger.LogWarning(ex, "[CATALOG] [SEARCH] OpenSearch indisponível (tentativa {Tentativa}/{Max}).",
                    tentativa, MaxTentativas);
                await Task.Delay(IntervaloTentativa, stoppingToken);
            }
        }
    }
}
