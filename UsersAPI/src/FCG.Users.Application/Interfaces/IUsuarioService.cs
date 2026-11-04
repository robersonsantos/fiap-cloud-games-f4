using FCG.Users.Application.DTOs;

namespace FCG.Users.Application.Interfaces;

public interface IUsuarioService
{
    Task<UsuarioResponseDto> CadastrarAsync(UsuarioNovoDto dto);
    Task<UsuarioResponseDto?> BuscarPorIdAsync(int id);
    Task<IEnumerable<UsuarioResponseDto>> ListarTodosAsync();
    Task<UsuarioResponseDto> AtualizarAsync(int id, UsuarioAtualizarDto dto);
    Task DesativarAsync(int id);
    Task ConcederAdminAsync(int id);
    Task RevogarAdminAsync(int id);

    /// <summary>
    /// Garante a existência do administrador inicial (credenciais vindas de Secret).
    /// Idempotente: cria o usuário ou apenas promove um existente.
    /// </summary>
    Task GarantirAdministradorInicialAsync(string nome, string email, string senha);
}
