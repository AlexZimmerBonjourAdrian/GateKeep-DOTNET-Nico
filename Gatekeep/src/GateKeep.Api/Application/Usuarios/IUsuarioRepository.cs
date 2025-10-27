using GateKeep.Api.Contracts.Usuarios;
using GateKeep.Api.Domain.Entities;
using GateKeep.Api.Domain.Enums;

namespace GateKeep.Api.Application.Usuarios;

public interface IUsuarioRepository
{
  Task<List<Usuario>> GetAllAsync();
  Task<Usuario?> GetByIdAsync(long id);
  Task AddAsync(Usuario usuario);
  Task UpdateAsync(Usuario usuario);
  Task DeleteAsync(long id);
}
