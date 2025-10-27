using GateKeep.Api.Contracts.Usuarios;
using GateKeep.Api.Domain.Entities;

namespace GateKeep.Api.Application.Usuarios;

public interface IUsuarioFactory
{
  Usuario CrearEstudiante(UsuarioDto dto);
  Usuario CrearFuncionario(UsuarioDto dto);
  Usuario CrearAdmin(UsuarioDto dto);
}
