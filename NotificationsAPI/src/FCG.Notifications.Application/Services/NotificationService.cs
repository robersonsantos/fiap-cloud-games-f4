using Microsoft.Extensions.Logging;

namespace FCG.Notifications.Application.Services;

public class NotificationService : INotificationService
{
    private readonly ILogger<NotificationService> _logger;

    public NotificationService(ILogger<NotificationService> logger)
    {
        _logger = logger;
    }

    public async Task EnviarBoasVindasAsync(string email, string nome)
    {
        await Task.Delay(100);
        _logger.LogInformation(
            "[NOTIFICATION] ✉️  E-mail de BOAS-VINDAS enviado para {Email} | Usuário: {Nome} | Assunto: Bem-vindo à FIAP Cloud Games!",
            email, nome);
    }

    public async Task EnviarConfirmacaoCompraAsync(string email, string gameName, decimal? valor = null)
    {
        await Task.Delay(100);
        _logger.LogInformation(
            "[NOTIFICATION] ✉️  E-mail de CONFIRMAÇÃO DE COMPRA enviado para {Email} | Jogo: {GameName} | Assunto: Sua compra foi aprovada!",
            email, gameName);
    }

    public async Task EnviarRejeicaoCompraAsync(string email, string gameName)
    {
        await Task.Delay(100);
        _logger.LogInformation(
            "[NOTIFICATION] ✉️  E-mail de REJEIÇÃO DE PAGAMENTO enviado para {Email} | Jogo: {GameName} | Assunto: Não foi possível processar seu pagamento.",
            email, gameName);
    }
}
