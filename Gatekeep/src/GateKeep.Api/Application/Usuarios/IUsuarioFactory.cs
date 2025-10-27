using GateKeep.Api.Contracts.Usuarios;
using GateKeep.Api.Domain.Entities;
using GateKeep.Api.Domain.Enums;

namespace GateKeep.Api.Application.Usuarios;

public interface IUsuarioFactory
{
  Usuario CrearUsuario(UsuarioDto dto);
  bool TryCrearUsuario(UsuarioDto dto, out Usuario? usuario);
  bool EsRolValido(Rol rol);
}
