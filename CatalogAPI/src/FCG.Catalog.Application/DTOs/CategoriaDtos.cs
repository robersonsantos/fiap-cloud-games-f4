namespace FCG.Catalog.Application.DTOs;

public record CategoriaNovaDto(string Nome, string Descricao);

public record CategoriaResponseDto(int Id, string Nome, string Descricao, bool Ativo);
