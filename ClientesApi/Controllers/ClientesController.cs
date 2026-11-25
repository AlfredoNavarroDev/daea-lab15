using ClientesApi.Modelos;
using ClientesApi.Repositorios;
using Microsoft.AspNetCore.Mvc;

namespace ClientesApi.Controllers;

[ApiController]
[Route("api/clientes")]
public class ClientesController : ControllerBase
{
    private readonly IClienteRepository _clienteRepository;

    public ClientesController(IClienteRepository clienteRepository)
    {
        _clienteRepository = clienteRepository;
    }

    // GET api/clientes
    [HttpGet]
    public ActionResult<List<Cliente>> ObtenerTodos()
    {
        return Ok(_clienteRepository.ObtenerTodos());
    }

    // GET api/clientes/5
    [HttpGet("{id:int}")]
    public ActionResult<Cliente> ObtenerPorId(int id)
    {
        var cliente = _clienteRepository.ObtenerPorId(id);
        if (cliente is null)
        {
            return NotFound();
        }

        return Ok(cliente);
    }
}
