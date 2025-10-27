using GateKeep.Api.Contracts.Usuarios;
using GateKeep.Api.Domain.Entities;
using GateKeep.Api.Domain.Enums;
using GateKeep.Api.Application.Usuarios;
public class UsuarioFactory : IUsuarioFactory
{
  public Usuario CrearUsuario(UsuarioDto dto)
  {
    return dto.Rol switch
    {
      Rol.Estudiante => new Estudiante(dto.Id, dto.Email, dto.Nombre, dto.Apellido, dto.Contrasenia, dto.Telefono, dto.FechaAlta, dto.Credencial),
      Rol.Funcionario => new Funcionario(dto.Id, dto.Email, dto.Nombre, dto.Apellido, dto.Contrasenia, dto.Telefono, dto.FechaAlta, dto.Credencial),
      _ => throw new InvalidOperationException($"Rol no soportado: {dto.Rol}")
    };
  }

  public bool TryCrearUsuario(UsuarioDto dto, out Usuario? usuario)
  {
    usuario = null;
    if (!EsRolValido(dto.Rol)) return false;

    usuario = CrearUsuario(dto);
    return true;
  }

  public bool EsRolValido(Rol rol) =>
    rol == Rol.Estudiante || rol == Rol.Funcionario;
}
