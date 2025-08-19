using APIServiceCenterPH.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Data;
using System.Data.SqlClient;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class TicketsController : ControllerBase
{
    private readonly IConfiguration _config;
    private readonly string _connectionString;

    public TicketsController(IConfiguration config)
    {
        _config = config;
        _connectionString = _config.GetConnectionString("DefaultConnection");
    }

    [HttpPost]
    [Authorize]
    public async Task<IActionResult> Post([FromBody] CreateTicketDto dto)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        string numTicket;

        using (var conn = new SqlConnection(_connectionString))
        using (var cmd = new SqlCommand("CrearTicketBasico", conn))
        {
            cmd.CommandType = CommandType.StoredProcedure;
            cmd.Parameters.AddWithValue("@Nombre", dto.Nombre);
            cmd.Parameters.AddWithValue("@Titulo", dto.Titulo);
            cmd.Parameters.AddWithValue("@Descripcion", dto.Descripcion);
            cmd.Parameters.AddWithValue("@Categoria", dto.Categoria);

            // salida: el procedimiento devuelve el NumTicket en el resultset
            conn.Open();
            var reader = await cmd.ExecuteReaderAsync();
            if (await reader.ReadAsync())
                numTicket = reader.GetString(0);
            else
                return StatusCode(500, "No se devolvió NumTicket");
        }

        return CreatedAtAction(
            nameof(GetByNumTicket),
            new { numTicket },
            new { NumTicket = numTicket }
        );
    }

    [HttpGet("{numTicket}")]
    [Authorize]
    public async Task<IActionResult> GetByNumTicket(string numTicket) 
    {
        using var conn = new SqlConnection(_connectionString);
        using var cmd = new SqlCommand(
            "SELECT * FROM Tickets WHERE NumTicket = @nt", conn);
        cmd.Parameters.AddWithValue("@nt", numTicket);
        conn.Open();
        using var reader = await cmd.ExecuteReaderAsync();
        if (!reader.Read()) return NotFound();
        return Ok(new
        {
            NumTicket = reader["NumTicket"],
            Titulo = reader["Titulo"],
            Descripcion = reader["Descripcion"],
            Categoria = reader["Categoria"],
            FechaCreacion = reader["FechaCreacion"],
            Estado = reader["Estado"]
        });
    }
}
