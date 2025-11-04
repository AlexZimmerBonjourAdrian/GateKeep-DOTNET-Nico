using GateKeep.Api.Contracts.Usuarios;
using GateKeep.Api.Domain.Entities;
using GateKeep.Api.Domain.Enums;
using GateKeep.Api.Application.Usuarios;

public class UsuarioFactory : IUsuarioFactory
{
  public Usuario CrearUsuario(UsuarioDto dto)
  {
    return new Usuario(
      dto.Id,
      dto.Email,
      dto.Nombre,
      dto.Apellido,
      dto.Contrasenia,
      dto.Telefono,
      dto.FechaAlta,
      dto.Credencial,
      dto.Rol
    );
  }
}
