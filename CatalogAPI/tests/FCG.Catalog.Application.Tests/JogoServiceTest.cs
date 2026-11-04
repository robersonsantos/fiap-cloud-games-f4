using FCG.Catalog.Application.DTOs;
using FCG.Catalog.Application.Interfaces;
using FCG.Catalog.Application.Services;
using FCG.Catalog.Domain.Entities;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Logging;
using Moq;
using NUnit.Framework;

namespace FCG.Catalog.Application.Tests;

[TestFixture]
public class JogoServiceTest
{
    private Mock<IJogoRepository> _jogoRepositoryMock = null!;
    private Mock<ICategoriaRepository> _categoriaRepositoryMock = null!;
    private Mock<IDistributedCache> _cacheMock = null!;
    private Mock<IJogoSearchIndex> _searchIndexMock = null!;
    private Mock<ILogger<JogoService>> _loggerMock = null!;
    private JogoService _service = null!;

    [SetUp]
    public void SetUp()
    {
        _jogoRepositoryMock = new Mock<IJogoRepository>();
        _categoriaRepositoryMock = new Mock<ICategoriaRepository>();
        // Mock de cache sempre vazio (miss): o serviço deve degradar para o repositório
        _cacheMock = new Mock<IDistributedCache>();
        _searchIndexMock = new Mock<IJogoSearchIndex>();
        _loggerMock = new Mock<ILogger<JogoService>>();
        _service = new JogoService(
            _jogoRepositoryMock.Object, _categoriaRepositoryMock.Object, _cacheMock.Object,
            _searchIndexMock.Object, _loggerMock.Object);
    }

    [Test]
    public async Task ListarAsync_DeveRetornarApenasJogosAtivosEVisiveis()
    {
        var categoria = new Categoria { Id = 1, Nome = "Ação" };
        var jogos = new List<Jogo>
        {
            new() { Id = 1, Nome = "Jogo A", Ativo = true, Visivel = true, CategoriaId = 1, Categoria = categoria },
            new() { Id = 2, Nome = "Jogo B", Ativo = true, Visivel = true, CategoriaId = 1, Categoria = categoria }
        };

        _jogoRepositoryMock
            .Setup(r => r.ListarAsync(null, null, true))
            .ReturnsAsync(jogos);

        var resultado = await _service.ListarAsync(null, null);

        Assert.That(resultado.Count(), Is.EqualTo(2));
        Assert.That(resultado.All(j => j.Ativo), Is.True);
    }

    [Test]
    public async Task ListarAsync_ComFiltro_DevePassarParametrosAoRepositorio()
    {
        var categoria = new Categoria { Id = 2, Nome = "RPG" };
        var jogos = new List<Jogo>
        {
            new() { Id = 3, Nome = "RPG Game", Ativo = true, Visivel = true, CategoriaId = 2, Categoria = categoria }
        };

        _jogoRepositoryMock
            .Setup(r => r.ListarAsync("RPG", 2, true))
            .ReturnsAsync(jogos);

        var resultado = await _service.ListarAsync("RPG", 2);

        Assert.That(resultado.Count(), Is.EqualTo(1));
        _jogoRepositoryMock.Verify(r => r.ListarAsync("RPG", 2, true), Times.Once);
    }

    [Test]
    public async Task CriarAsync_ComCategoriaInexistente_DeveLancarKeyNotFoundException()
    {
        _categoriaRepositoryMock
            .Setup(r => r.ExisteAsync(99))
            .ReturnsAsync(false);

        var dto = new JogoNovoDto("Jogo Novo", "Desc", 49.90m, 99);

        Assert.ThrowsAsync<KeyNotFoundException>(() => _service.CriarAsync(dto));
    }

    [Test]
    public async Task CriarAsync_ComDadosValidos_DeveRetornarJogoCriado()
    {
        var categoria = new Categoria { Id = 1, Nome = "Ação" };
        _categoriaRepositoryMock.Setup(r => r.ExisteAsync(1)).ReturnsAsync(true);

        _jogoRepositoryMock
            .Setup(r => r.AdicionarAsync(It.IsAny<Jogo>()))
            .ReturnsAsync((Jogo j) =>
            {
                j.Id = 10;
                j.Categoria = categoria;
                return j;
            });

        var dto = new JogoNovoDto("Novo Jogo", "Descrição", 39.90m, 1);

        var resultado = await _service.CriarAsync(dto);

        Assert.Multiple(() =>
        {
            Assert.That(resultado.Id, Is.EqualTo(10));
            Assert.That(resultado.Nome, Is.EqualTo("Novo Jogo"));
            Assert.That(resultado.Preco, Is.EqualTo(39.90m));
        });
    }

    [Test]
    public async Task RemoverAsync_ComJogoInexistente_DeveLancarKeyNotFoundException()
    {
        _jogoRepositoryMock
            .Setup(r => r.BuscarPorIdAsync(999))
            .ReturnsAsync((Jogo?)null);

        Assert.ThrowsAsync<KeyNotFoundException>(() => _service.RemoverAsync(999));
    }

    [Test]
    public async Task RemoverAsync_ComJogoExistente_DeveDesativarJogo()
    {
        var jogo = new Jogo { Id = 1, Nome = "Jogo", Ativo = true };
        _jogoRepositoryMock.Setup(r => r.BuscarPorIdAsync(1)).ReturnsAsync(jogo);
        _jogoRepositoryMock.Setup(r => r.AtualizarAsync(It.IsAny<Jogo>())).ReturnsAsync(jogo);

        await _service.RemoverAsync(1);

        Assert.That(jogo.Ativo, Is.False);
        _jogoRepositoryMock.Verify(r => r.AtualizarAsync(It.Is<Jogo>(j => !j.Ativo)), Times.Once);
    }

    [Test]
    public async Task AtualizarAsync_ComJogoInexistente_DeveLancarKeyNotFoundException()
    {
        _jogoRepositoryMock.Setup(r => r.BuscarPorIdAsync(999)).ReturnsAsync((Jogo?)null);

        var dto = new JogoAtualizarDto("Nome", "Desc", 10m, 1, true);

        Assert.ThrowsAsync<KeyNotFoundException>(() => _service.AtualizarAsync(999, dto));
    }

    // ─── Sincronização com o OpenSearch (Fase 4) ───────────────────────────────

    [Test]
    public async Task CriarAsync_DeveIndexarJogoNoOpenSearch()
    {
        var categoria = new Categoria { Id = 1, Nome = "Aventura" };
        _categoriaRepositoryMock.Setup(r => r.ExisteAsync(1)).ReturnsAsync(true);
        _jogoRepositoryMock
            .Setup(r => r.AdicionarAsync(It.IsAny<Jogo>()))
            .ReturnsAsync((Jogo j) => { j.Id = 42; j.Categoria = categoria; return j; });

        await _service.CriarAsync(new JogoNovoDto("Zelda", "Aventura épica", 199.90m, 1));

        _searchIndexMock.Verify(s => s.IndexarAsync(
            It.Is<JogoDocumentoBusca>(d => d.Id == 42 && d.Nome == "Zelda" && d.Categoria == "Aventura" && d.Disponivel),
            It.IsAny<CancellationToken>()), Times.Once);
    }

    [Test]
    public async Task AtualizarAsync_DeveReindexarJogoComDadosNovos()
    {
        var categoria = new Categoria { Id = 1, Nome = "RPG" };
        var jogo = new Jogo { Id = 5, Nome = "Antigo", Ativo = true, Visivel = true, CategoriaId = 1, Categoria = categoria };
        _jogoRepositoryMock.Setup(r => r.BuscarPorIdAsync(5)).ReturnsAsync(jogo);
        _categoriaRepositoryMock.Setup(r => r.ExisteAsync(1)).ReturnsAsync(true);
        _jogoRepositoryMock.Setup(r => r.AtualizarAsync(It.IsAny<Jogo>())).ReturnsAsync((Jogo j) => j);

        await _service.AtualizarAsync(5, new JogoAtualizarDto("Novo Nome", "Desc", 10m, 1, false));

        _searchIndexMock.Verify(s => s.IndexarAsync(
            It.Is<JogoDocumentoBusca>(d => d.Id == 5 && d.Nome == "Novo Nome" && !d.Disponivel),
            It.IsAny<CancellationToken>()), Times.Once);
    }

    [Test]
    public async Task RemoverAsync_DeveRemoverJogoDoIndice()
    {
        var jogo = new Jogo { Id = 3, Nome = "Jogo", Ativo = true };
        _jogoRepositoryMock.Setup(r => r.BuscarPorIdAsync(3)).ReturnsAsync(jogo);
        _jogoRepositoryMock.Setup(r => r.AtualizarAsync(It.IsAny<Jogo>())).ReturnsAsync(jogo);

        await _service.RemoverAsync(3);

        _searchIndexMock.Verify(s => s.RemoverAsync(3, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Test]
    public async Task CriarAsync_QuandoOpenSearchFalha_NaoDeveDesfazerAEscrita()
    {
        var categoria = new Categoria { Id = 1, Nome = "Ação" };
        _categoriaRepositoryMock.Setup(r => r.ExisteAsync(1)).ReturnsAsync(true);
        _jogoRepositoryMock
            .Setup(r => r.AdicionarAsync(It.IsAny<Jogo>()))
            .ReturnsAsync((Jogo j) => { j.Id = 11; j.Categoria = categoria; return j; });
        _searchIndexMock
            .Setup(s => s.IndexarAsync(It.IsAny<JogoDocumentoBusca>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new HttpRequestException("OpenSearch fora do ar"));

        var resultado = await _service.CriarAsync(new JogoNovoDto("Novo", "Desc", 10m, 1));

        Assert.That(resultado.Id, Is.EqualTo(11));
    }
}
