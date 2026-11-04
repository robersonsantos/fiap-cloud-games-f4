using FCG.Catalog.Domain.Entities;
using NUnit.Framework;

namespace FCG.Catalog.Domain.Tests;

[TestFixture]
public class JogoDomainTest
{
    [Test]
    public void Jogo_CriadoComValoresCorretos_DeveInicializarPropiedades()
    {
        var jogo = new Jogo
        {
            Id = 1,
            Nome = "Jogo Teste",
            Descricao = "Descrição teste",
            Preco = 49.90m,
            CategoriaId = 1,
            Ativo = true,
            Visivel = true
        };

        Assert.Multiple(() =>
        {
            Assert.That(jogo.Id, Is.EqualTo(1));
            Assert.That(jogo.Nome, Is.EqualTo("Jogo Teste"));
            Assert.That(jogo.Preco, Is.EqualTo(49.90m));
            Assert.That(jogo.Ativo, Is.True);
            Assert.That(jogo.Visivel, Is.True);
        });
    }

    [Test]
    public void Jogo_Desativar_DeveDefinirAtivoComoFalse()
    {
        var jogo = new Jogo { Ativo = true, Visivel = true };

        jogo.Desativar();

        Assert.That(jogo.Ativo, Is.False);
    }

    [Test]
    public void Jogo_EstaDisponivel_QuandoAtivoEVisivel_DeveRetornarTrue()
    {
        var jogo = new Jogo { Ativo = true, Visivel = true };

        Assert.That(jogo.EstaDisponivel(), Is.True);
    }

    [Test]
    public void Jogo_EstaDisponivel_QuandoInativo_DeveRetornarFalse()
    {
        var jogo = new Jogo { Ativo = false, Visivel = true };

        Assert.That(jogo.EstaDisponivel(), Is.False);
    }

    [Test]
    public void Jogo_EstaDisponivel_QuandoInvisivel_DeveRetornarFalse()
    {
        var jogo = new Jogo { Ativo = true, Visivel = false };

        Assert.That(jogo.EstaDisponivel(), Is.False);
    }

    [Test]
    public void Jogo_Atualizar_ComDadosValidos_DeveAtualizarPropiedades()
    {
        var jogo = new Jogo { Nome = "Nome Antigo", Preco = 10m, CategoriaId = 1, Visivel = false };

        jogo.Atualizar("Nome Novo", "Nova descrição", 59.90m, 2, true);

        Assert.Multiple(() =>
        {
            Assert.That(jogo.Nome, Is.EqualTo("Nome Novo"));
            Assert.That(jogo.Descricao, Is.EqualTo("Nova descrição"));
            Assert.That(jogo.Preco, Is.EqualTo(59.90m));
            Assert.That(jogo.CategoriaId, Is.EqualTo(2));
            Assert.That(jogo.Visivel, Is.True);
        });
    }

    [Test]
    public void Jogo_Atualizar_ComNomeVazio_DeveLancarArgumentException()
    {
        var jogo = new Jogo { Nome = "Nome", Preco = 10m };

        Assert.Throws<ArgumentException>(() => jogo.Atualizar("", "desc", 10m, 1, true));
    }

    [Test]
    public void Jogo_Atualizar_ComPrecoNegativo_DeveLancarArgumentException()
    {
        var jogo = new Jogo { Nome = "Nome", Preco = 10m };

        Assert.Throws<ArgumentException>(() => jogo.Atualizar("Nome", "desc", -1m, 1, true));
    }

    [Test]
    public void Categoria_Desativar_DeveDefinirAtivoComoFalse()
    {
        var categoria = new Categoria { Ativo = true };

        categoria.Desativar();

        Assert.That(categoria.Ativo, Is.False);
    }

    [Test]
    public void Categoria_Atualizar_ComNomeVazio_DeveLancarArgumentException()
    {
        var categoria = new Categoria { Nome = "Nome", Descricao = "Desc" };

        Assert.Throws<ArgumentException>(() => categoria.Atualizar("", "nova desc"));
    }

    [Test]
    public void Categoria_Atualizar_ComDadosValidos_DeveAtualizarPropiedades()
    {
        var categoria = new Categoria { Nome = "Antigo", Descricao = "Antiga desc" };

        categoria.Atualizar("Novo Nome", "Nova descrição");

        Assert.Multiple(() =>
        {
            Assert.That(categoria.Nome, Is.EqualTo("Novo Nome"));
            Assert.That(categoria.Descricao, Is.EqualTo("Nova descrição"));
        });
    }
}
