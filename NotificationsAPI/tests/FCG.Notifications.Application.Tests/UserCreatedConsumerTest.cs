using FCG.Notifications.Application.Consumers;
using FCG.Users.Application.Events;
using FCG.Notifications.Application.Services;
using MassTransit;
using Microsoft.Extensions.Logging;
using Moq;
using NUnit.Framework;

namespace FCG.Notifications.Application.Tests;

[TestFixture]
public class UserCreatedConsumerTest
{
    [Test]
    public async Task Consume_DeveEnviarBoasVindas()
    {
        var notificationServiceMock = new Mock<INotificationService>();
        var loggerMock = new Mock<ILogger<UserCreatedConsumer>>();
        var consumer = new UserCreatedConsumer(notificationServiceMock.Object, loggerMock.Object);

        var evt = new UserCreatedEvent(1, "Test User", "test@test.com", DateTime.UtcNow);
        var contextMock = new Mock<ConsumeContext<UserCreatedEvent>>();
        contextMock.Setup(x => x.Message).Returns(evt);

        await consumer.Consume(contextMock.Object);

        notificationServiceMock.Verify(x => x.EnviarBoasVindasAsync("test@test.com", "Test User"), Times.Once);
    }

    [Test]
    public async Task Consume_QuandoServicoLancaExcecao_NaoDevePropagar()
    {
        var notificationServiceMock = new Mock<INotificationService>();
        notificationServiceMock
            .Setup(x => x.EnviarBoasVindasAsync(It.IsAny<string>(), It.IsAny<string>()))
            .ThrowsAsync(new InvalidOperationException("Erro simulado"));

        var loggerMock = new Mock<ILogger<UserCreatedConsumer>>();
        var consumer = new UserCreatedConsumer(notificationServiceMock.Object, loggerMock.Object);

        var evt = new UserCreatedEvent(1, "Test User", "test@test.com", DateTime.UtcNow);
        var contextMock = new Mock<ConsumeContext<UserCreatedEvent>>();
        contextMock.Setup(x => x.Message).Returns(evt);

        Assert.DoesNotThrowAsync(() => consumer.Consume(contextMock.Object));
    }
}
