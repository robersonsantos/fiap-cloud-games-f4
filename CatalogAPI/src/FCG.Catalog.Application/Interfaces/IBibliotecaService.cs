using FCG.Catalog.Application.DTOs;

namespace FCG.Catalog.Application.Interfaces;

public interface IBibliotecaService
{
    Task<CompraResponseDto> IniciarCompraAsync(int jogoId, int userId, string userEmail);
    Task AdicionarJogoBibliotecaAsync(int userId, int jogoId, decimal valorPago, Guid transacaoId);
    Task<IEnumerable<JogoBibliotecaDto>> ListarBibliotecaAsync(int userId);
}
