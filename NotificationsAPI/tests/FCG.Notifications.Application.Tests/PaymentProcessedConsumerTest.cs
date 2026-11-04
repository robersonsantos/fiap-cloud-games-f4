using FCG.Notifications.Application.Consumers;
using FCG.Payments.Application.Events;
using FCG.Notifications.Application.Services;
using MassTransit;
using Microsoft.Extensions.Logging;
using Moq;
using NUnit.Framework;

namespace FCG.Notifications.Application.Tests;

[TestFixture]
public class PaymentProcessedConsumerTest
{
    [Test]
    public async Task Consume_Approved_DeveEnviarConfirmacao()
    {
        var notificationServiceMock = new Mock<INotificationService>();
        var loggerMock = new Mock<ILogger<PaymentProcessedConsumer>>();
        var consumer = new PaymentProcessedConsumer(notificationServiceMock.Object, loggerMock.Object);

        var evt = new PaymentProcessedEvent(Guid.NewGuid(), 1, 1, "Jogo Teste", "test@test.com", "Approved", DateTime.UtcNow);
        var contextMock = new Mock<ConsumeContext<PaymentProcessedEvent>>();
        contextMock.Setup(x => x.Message).Returns(evt);

        await consumer.Consume(contextMock.Object);

        notificationServiceMock.Verify(x => x.EnviarConfirmacaoCompraAsync("test@test.com", "Jogo Teste", null), Times.Once);
    }

    [Test]
    public async Task Consume_Rejected_DeveEnviarRejeicao()
    {
        var notificationServiceMock = new Mock<INotificationService>();
        var loggerMock = new Mock<ILogger<PaymentProcessedConsumer>>();
        var consumer = new PaymentProcessedConsumer(notificationServiceMock.Object, loggerMock.Object);

        var evt = new PaymentProcessedEvent(Guid.NewGuid(), 1, 1, "Jogo Teste", "test@test.com", "Rejected", DateTime.UtcNow);
        var contextMock = new Mock<ConsumeContext<PaymentProcessedEvent>>();
        contextMock.Setup(x => x.Message).Returns(evt);

        await consumer.Consume(contextMock.Object);

        notificationServiceMock.Verify(x => x.EnviarRejeicaoCompraAsync("test@test.com", "Jogo Teste"), Times.Once);
    }

    [Test]
    public async Task Consume_QuandoServicoLancaExcecao_NaoDevePropagar()
    {
        var notificationServiceMock = new Mock<INotificationService>();
        notificationServiceMock
            .Setup(x => x.EnviarConfirmacaoCompraAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<decimal?>()))
            .ThrowsAsync(new InvalidOperationException("Erro simulado"));

        var loggerMock = new Mock<ILogger<PaymentProcessedConsumer>>();
        var consumer = new PaymentProcessedConsumer(notificationServiceMock.Object, loggerMock.Object);

        var evt = new PaymentProcessedEvent(Guid.NewGuid(), 1, 1, "Jogo Teste", "test@test.com", "Approved", DateTime.UtcNow);
        var contextMock = new Mock<ConsumeContext<PaymentProcessedEvent>>();
        contextMock.Setup(x => x.Message).Returns(evt);

        Assert.DoesNotThrowAsync(() => consumer.Consume(contextMock.Object));
    }
}
