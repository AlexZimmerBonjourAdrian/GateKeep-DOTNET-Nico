using GateKeep.Api.Contracts.Usuarios;
using GateKeep.Api.Domain.Entities;
using GateKeep.Api.Domain.Enums;
using GateKeep.Api.Application.Usuarios;

public class UsuarioFactory : IUsuarioFactory
{
  public Usuario CrearEstudiante(UsuarioDto dto)
  {
    return new Estudiante(dto.Id, dto.Email, dto.Nombre, dto.Apellido, dto.Contrasenia, dto.Telefono, dto.FechaAlta, dto.Credencial);
  }

  public Usuario CrearFuncionario(UsuarioDto dto)
  {
    return new Funcionario(dto.Id, dto.Email, dto.Nombre, dto.Apellido, dto.Contrasenia, dto.Telefono, dto.FechaAlta, dto.Credencial);
  }

  public Usuario CrearAdmin(UsuarioDto dto)
  {
    return new Admin(dto.Id, dto.Email, dto.Nombre, dto.Apellido, dto.Contrasenia, dto.Telefono, dto.FechaAlta, dto.Credencial);
  }
}
