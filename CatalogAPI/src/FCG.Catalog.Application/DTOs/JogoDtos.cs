namespace FCG.Catalog.Application.DTOs;

public record JogoNovoDto(string Nome, string Descricao, decimal Preco, int CategoriaId);

public record JogoAtualizarDto(string Nome, string Descricao, decimal Preco, int CategoriaId, bool Visivel);

public record JogoResponseDto(int Id, string Nome, string Descricao, decimal Preco, string Categoria, bool Ativo, bool Visivel);

public record CompraResponseDto(Guid OrderId, string Mensagem);

public record JogoBibliotecaDto(int Id, string Nome, string Descricao, string Categoria, decimal ValorPago, DateTime DataAquisicao);
