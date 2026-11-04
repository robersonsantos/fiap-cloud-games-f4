using FCG.Notifications.Application.Services;
using Microsoft.Extensions.Logging;
using Moq;
using NUnit.Framework;

namespace FCG.Notifications.Application.Tests;

[TestFixture]
public class NotificationServiceTest
{
    private Mock<ILogger<NotificationService>> _loggerMock = null!;
    private NotificationService _service = null!;

    [SetUp]
    public void Setup()
    {
        _loggerMock = new Mock<ILogger<NotificationService>>();
        _service = new NotificationService(_loggerMock.Object);
    }

    [Test]
    public async Task EnviarBoasVindasAsync_DeveLogarMensagem()
    {
        await _service.EnviarBoasVindasAsync("test@test.com", "Test User");
        Assert.Pass();
    }

    [Test]
    public async Task EnviarConfirmacaoCompraAsync_DeveLogarMensagem()
    {
        await _service.EnviarConfirmacaoCompraAsync("test@test.com", "Jogo Teste");
        Assert.Pass();
    }

    [Test]
    public async Task EnviarRejeicaoCompraAsync_DeveLogarMensagem()
    {
        await _service.EnviarRejeicaoCompraAsync("test@test.com", "Jogo Teste");
        Assert.Pass();
    }
}
