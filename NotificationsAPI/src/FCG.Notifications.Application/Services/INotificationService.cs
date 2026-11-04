namespace FCG.Notifications.Application.Services;

public interface INotificationService
{
    Task EnviarBoasVindasAsync(string email, string nome);
    Task EnviarConfirmacaoCompraAsync(string email, string gameName, decimal? valor = null);
    Task EnviarRejeicaoCompraAsync(string email, string gameName);
}
