using FCG.Users.Application.Interfaces;
using FCG.Users.Application.Services;
using FCG.Users.Domain.Entities;
using FCG.Users.Domain.Helpers;
using Microsoft.Extensions.Logging;
using Moq;
using NUnit.Framework;

namespace FCG.Users.Application.Tests;

[TestFixture]
public class AdministradorInicialTest
{
    private Mock<IUsuarioRepository> _repositoryMock = null!;
    private Mock<IUnidadeDeTrabalho> _uowMock = null!;
    private Mock<IEventPublisher> _eventPublisherMock = null!;
    private UsuarioService _service = null!;

    [SetUp]
    public void Setup()
    {
        _repositoryMock = new Mock<IUsuarioRepository>();
        _uowMock = new Mock<IUnidadeDeTrabalho>();
        _eventPublisherMock = new Mock<IEventPublisher>();
        _service = new UsuarioService(
            _repositoryMock.Object, _uowMock.Object, _eventPublisherMock.Object,
            new Mock<ILogger<UsuarioService>>().Object);
    }

    [Test]
    public async Task GarantirAdministradorInicial_QuandoNaoExiste_DeveCriarUsuarioAdministrador()
    {
        Usuario? criado = null;
        _repositoryMock.Setup(r => r.BuscarPorEmailAsync("admin@fcg.com")).ReturnsAsync((Usuario?)null);
        _repositoryMock.Setup(r => r.AdicionarAsync(It.IsAny<Usuario>()))
            .Callback<Usuario>(u => criado = u)
            .Returns(Task.CompletedTask);

        await _service.GarantirAdministradorInicialAsync("Admin", "Admin@FCG.com", "Admin@1234");

        Assert.That(criado, Is.Not.Null);
        Assert.Multiple(() =>
        {
            Assert.That(criado!.Email, Is.EqualTo("admin@fcg.com"));
            Assert.That(criado.Administrador, Is.Not.Null);
            Assert.That(criado.HashSenha, Is.EqualTo(SenhaHelper.GerarHash("Admin@1234")));
        });
        _uowMock.Verify(u => u.SalvarAsync(), Times.Once);
        _eventPublisherMock.VerifyNoOtherCalls();
    }

    [Test]
    public async Task GarantirAdministradorInicial_QuandoUsuarioComumExiste_DevePromover()
    {
        var usuario = new Usuario { Id = 7, Email = "admin@fcg.com", Ativo = true };
        _repositoryMock.Setup(r => r.BuscarPorEmailAsync("admin@fcg.com")).ReturnsAsync(usuario);

        await _service.GarantirAdministradorInicialAsync("Admin", "admin@fcg.com", "Admin@1234");

        Assert.That(usuario.Administrador, Is.Not.Null);
        Assert.That(usuario.Administrador!.UsuarioId, Is.EqualTo(7));
        _repositoryMock.Verify(r => r.AdicionarAsync(It.IsAny<Usuario>()), Times.Never);
        _uowMock.Verify(u => u.SalvarAsync(), Times.Once);
    }

    [Test]
    public async Task GarantirAdministradorInicial_QuandoJaEhAdmin_NaoDeveAlterarNada()
    {
        var usuario = new Usuario { Id = 7, Email = "admin@fcg.com", Administrador = new Administrador { UsuarioId = 7 } };
        _repositoryMock.Setup(r => r.BuscarPorEmailAsync("admin@fcg.com")).ReturnsAsync(usuario);

        await _service.GarantirAdministradorInicialAsync("Admin", "admin@fcg.com", "Admin@1234");

        _uowMock.Verify(u => u.SalvarAsync(), Times.Never);
    }

    [Test]
    public void GarantirAdministradorInicial_ComSenhaFraca_DeveLancarArgumentException()
    {
        Assert.ThrowsAsync<ArgumentException>(() =>
            _service.GarantirAdministradorInicialAsync("Admin", "admin@fcg.com", "123"));
    }
}
