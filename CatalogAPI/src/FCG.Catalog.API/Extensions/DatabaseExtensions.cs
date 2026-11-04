using FCG.Catalog.Infrastructure.Data;
using Microsoft.AspNetCore.Builder;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace FCG.Catalog.API.Extensions;

public static class DatabaseExtensions
{
    public static IApplicationBuilder MigrateDatabase(this IApplicationBuilder app)
    {
        using var scope = app.ApplicationServices.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<CatalogDbContext>();
        var logger = scope.ServiceProvider.GetRequiredService<ILogger<CatalogDbContext>>();

        try
        {
            logger.LogInformation("[CATALOG] Aplicando migrations do banco de dados...");
            context.Database.Migrate();
            logger.LogInformation("[CATALOG] Migrations aplicadas com sucesso.");
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "[CATALOG] Erro ao aplicar migrations do banco de dados.");
            throw;
        }

        return app;
    }
}
