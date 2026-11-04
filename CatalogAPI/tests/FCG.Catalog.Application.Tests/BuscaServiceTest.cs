using FCG.Catalog.Application.DTOs;
using FCG.Catalog.Application.Interfaces;
using FCG.Catalog.Application.Services;
using FCG.Catalog.Domain.Entities;
using Microsoft.Extensions.Logging;
using Moq;
using NUnit.Framework;

namespace FCG.Catalog.Application.Tests;

[TestFixture]
public class BuscaServiceTest
{
    private Mock<IJogoSearchIndex> _searchIndexMock = null!;
    private Mock<IJogoRepository> _jogoRepositoryMock = null!;
    private BuscaService _service = null!;

    [SetUp]
    public void SetUp()
    {
        _searchIndexMock = new Mock<IJogoSearchIndex>();
        _jogoRepositoryMock = new Mock<IJogoRepository>();
        _service = new BuscaService(_searchIndexMock.Object, _jogoRepositoryMock.Object,
            new Mock<ILogger<BuscaService>>().Object);
    }

    [TestCase(null)]
    [TestCase("")]
    [TestCase("   ")]
    public void BuscarAsync_SemTermo_DeveLancarArgumentException(string? termo)
    {
        Assert.ThrowsAsync<ArgumentException>(() => _service.BuscarAsync(termo, null));
        _searchIndexMock.VerifyNoOtherCalls();
    }

    [Test]
    public async Task BuscarAsync_DeveRepassarTermoNormalizadoERetornarResultadosDoIndice()
    {
        var hits = new List<JogoBuscaResultadoDto>
        {
            new(1, "The Legend of Zelda", "Aventura", 299m, "Aventura", 7.5),
            new(2, "Zelda II", "Aventura", 99m, "Aventura", 3.1)
        };
        _searchIndexMock
            .Setup(s => s.BuscarAsync("zelad", BuscaService.TamanhoPadrao, It.IsAny<CancellationToken>()))
            .ReturnsAsync(hits);

        var resposta = await _service.BuscarAsync("  zelad ", null);

        Assert.Multiple(() =>
        {
            Assert.That(resposta.Termo, Is.EqualTo("zelad"));
            Assert.That(resposta.Total, Is.EqualTo(2));
            Assert.That(resposta.Resultados[0].Relevancia, Is.GreaterThan(resposta.Resultados[1].Relevancia));
        });
    }

    [TestCase(0, 1)]
    [TestCase(500, BuscaService.TamanhoMaximo)]
    [TestCase(20, 20)]
    public async Task BuscarAsync_DeveLimitarTamanho(int solicitado, int esperado)
    {
        _searchIndexMock
            .Setup(s => s.BuscarAsync(It.IsAny<string>(), It.IsAny<int>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<JogoBuscaResultadoDto>());

        await _service.BuscarAsync("mario", solicitado);

        _searchIndexMock.Verify(s => s.BuscarAsync("mario", esperado, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Test]
    public async Task ReindexarAsync_DeveIndexarTodosOsJogosDoBancoIncluindoIndisponiveis()
    {
        var categoria = new Categoria { Id = 1, Nome = "Ação" };
        var jogos = new List<Jogo>
        {
            new() { Id = 1, Nome = "Ativo", Ativo = true, Visivel = true, Categoria = categoria },
            new() { Id = 2, Nome = "Inativo", Ativo = false, Visivel = true, Categoria = categoria }
        };
        _jogoRepositoryMock.Setup(r => r.ListarAsync(null, null, false)).ReturnsAsync(jogos);

        IReadOnlyCollection<JogoDocumentoBusca>? enviados = null;
        _searchIndexMock
            .Setup(s => s.IndexarLoteAsync(It.IsAny<IReadOnlyCollection<JogoDocumentoBusca>>(), It.IsAny<CancellationToken>()))
            .Callback<IReadOnlyCollection<JogoDocumentoBusca>, CancellationToken>((docs, _) => enviados = docs)
            .Returns(Task.CompletedTask);

        var resultado = await _service.ReindexarAsync();

        Assert.That(resultado.JogosIndexados, Is.EqualTo(2));
        _searchIndexMock.Verify(s => s.GarantirIndiceAsync(It.IsAny<CancellationToken>()), Times.Once);
        Assert.That(enviados!.Single(d => d.Id == 2).Disponivel, Is.False);
        Assert.That(enviados!.Single(d => d.Id == 1).Categoria, Is.EqualTo("Ação"));
    }
}
