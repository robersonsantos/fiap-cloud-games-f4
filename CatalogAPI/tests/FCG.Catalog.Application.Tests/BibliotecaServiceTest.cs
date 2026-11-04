using FCG.Catalog.Application.Events;
using FCG.Catalog.Application.Interfaces;
using FCG.Catalog.Application.Services;
using FCG.Catalog.Domain.Entities;
using Microsoft.Extensions.Logging;
using Moq;
using NUnit.Framework;

namespace FCG.Catalog.Application.Tests;

[TestFixture]
public class BibliotecaServiceTest
{
    private Mock<IBibliotecaRepository> _bibliotecaRepositoryMock = null!;
    private Mock<IJogoRepository> _jogoRepositoryMock = null!;
    private Mock<IEventPublisher> _eventPublisherMock = null!;
    private Mock<ILogger<BibliotecaService>> _loggerMock = null!;
    private BibliotecaService _service = null!;

    [SetUp]
    public void SetUp()
    {
        _bibliotecaRepositoryMock = new Mock<IBibliotecaRepository>();
        _jogoRepositoryMock = new Mock<IJogoRepository>();
        _eventPublisherMock = new Mock<IEventPublisher>();
        _loggerMock = new Mock<ILogger<BibliotecaService>>();

        _service = new BibliotecaService(
            _bibliotecaRepositoryMock.Object,
            _jogoRepositoryMock.Object,
            _eventPublisherMock.Object,
            _loggerMock.Object);
    }

    [Test]
    public async Task IniciarCompraAsync_JogoJaNaBiblioteca_DeveLancarInvalidOperationException()
    {
        var jogo = new Jogo { Id = 1, Nome = "Jogo X", Ativo = true, Visivel = true, Preco = 49.90m };

        _jogoRepositoryMock.Setup(r => r.BuscarPorIdAsync(1)).ReturnsAsync(jogo);
        _bibliotecaRepositoryMock.Setup(r => r.JogoJaAdquiridoAsync(10, 1)).ReturnsAsync(true);

        Assert.ThrowsAsync<InvalidOperationException>(() =>
            _service.IniciarCompraAsync(1, 10, "user@test.com"));
    }

    [Test]
    public async Task IniciarCompraAsync_JogoInexistente_DeveLancarKeyNotFoundException()
    {
        _jogoRepositoryMock.Setup(r => r.BuscarPorIdAsync(99)).ReturnsAsync((Jogo?)null);

        Assert.ThrowsAsync<KeyNotFoundException>(() =>
            _service.IniciarCompraAsync(99, 10, "user@test.com"));
    }

    [Test]
    public async Task IniciarCompraAsync_JogoIndisponivel_DeveLancarInvalidOperationException()
    {
        var jogo = new Jogo { Id = 1, Nome = "Jogo X", Ativo = false, Visivel = true, Preco = 49.90m };

        _jogoRepositoryMock.Setup(r => r.BuscarPorIdAsync(1)).ReturnsAsync(jogo);
        _bibliotecaRepositoryMock.Setup(r => r.JogoJaAdquiridoAsync(10, 1)).ReturnsAsync(false);

        Assert.ThrowsAsync<InvalidOperationException>(() =>
            _service.IniciarCompraAsync(1, 10, "user@test.com"));
    }

    [Test]
    public async Task IniciarCompraAsync_Sucesso_DevePublicarOrderPlacedEventERetornarCompraResponseDto()
    {
        var jogo = new Jogo { Id = 1, Nome = "Jogo Sucesso", Ativo = true, Visivel = true, Preco = 79.90m };

        _jogoRepositoryMock.Setup(r => r.BuscarPorIdAsync(1)).ReturnsAsync(jogo);
        _bibliotecaRepositoryMock.Setup(r => r.JogoJaAdquiridoAsync(5, 1)).ReturnsAsync(false);
        _eventPublisherMock.Setup(p => p.PublicarAsync(It.IsAny<OrderPlacedEvent>())).Returns(Task.CompletedTask);

        var resultado = await _service.IniciarCompraAsync(1, 5, "comprador@test.com");

        Assert.Multiple(() =>
        {
            Assert.That(resultado, Is.Not.Null);
            Assert.That(resultado.OrderId, Is.Not.EqualTo(Guid.Empty));
            Assert.That(resultado.Mensagem, Does.Contain(resultado.OrderId.ToString()));
        });

        _eventPublisherMock.Verify(p => p.PublicarAsync(It.Is<OrderPlacedEvent>(e =>
            e.UserId == 5 &&
            e.GameId == 1 &&
            e.GameName == "Jogo Sucesso" &&
            e.Price == 79.90m &&
            e.UserEmail == "comprador@test.com")), Times.Once);
    }

    [Test]
    public async Task AdicionarJogoBibliotecaAsync_Sucesso_DeveAdicionarJogo()
    {
        var transacaoId = Guid.NewGuid();
        _bibliotecaRepositoryMock
            .Setup(r => r.AdicionarAsync(It.IsAny<JogoAdquirido>()))
            .ReturnsAsync((JogoAdquirido ja) => ja);

        await _service.AdicionarJogoBibliotecaAsync(10, 1, 49.90m, transacaoId);

        _bibliotecaRepositoryMock.Verify(r => r.AdicionarAsync(It.Is<JogoAdquirido>(ja =>
            ja.UsuarioId == 10 &&
            ja.JogoId == 1 &&
            ja.ValorPago == 49.90m &&
            ja.TransacaoId == transacaoId)), Times.Once);
    }

    [Test]
    public async Task ListarBibliotecaAsync_DeveRetornarJogosDoUsuario()
    {
        var categoria = new Categoria { Id = 1, Nome = "Aventura" };
        var jogo = new Jogo { Id = 2, Nome = "Aventura RPG", Descricao = "RPG aventura", Preco = 59.90m, Categoria = categoria };
        var jogosAdquiridos = new List<JogoAdquirido>
        {
            new() { Id = 1, UsuarioId = 7, JogoId = 2, Jogo = jogo, ValorPago = 59.90m, DataAquisicao = DateTime.UtcNow, TransacaoId = Guid.NewGuid() }
        };

        _bibliotecaRepositoryMock
            .Setup(r => r.ListarPorUsuarioAsync(7))
            .ReturnsAsync(jogosAdquiridos);

        var resultado = await _service.ListarBibliotecaAsync(7);

        Assert.That(resultado.Count(), Is.EqualTo(1));
        Assert.That(resultado.First().Nome, Is.EqualTo("Aventura RPG"));
    }
}
