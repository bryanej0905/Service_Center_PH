using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ServiceCenter.Models
{
    public class IncidenteReporte
{
    public int Id { get; set; }
    public int ArticuloId { get; set; }
    public string Descripcion { get; set; }
    public string Usuario { get; set; } // Opcional: puedes guardar el email/nombre
    public DateTime FechaReporte { get; set; }
    public string Estado { get; set; } // Ej: "Pendiente", "Revisado"
}
}

