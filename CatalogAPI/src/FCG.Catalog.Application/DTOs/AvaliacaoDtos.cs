namespace FCG.Catalog.Application.DTOs;

public record AvaliacaoNovaDto(int Nota, string? Comentario);

public record AvaliacaoResponseDto(Guid Id, int JogoId, int UserId, int Nota, string? Comentario, DateTime Data);

public record AvaliacaoResumoDto(int JogoId, double MediaNotas, int TotalAvaliacoes, IEnumerable<AvaliacaoResponseDto> Avaliacoes);
