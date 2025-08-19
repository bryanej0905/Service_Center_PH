using ServiceCenter.Models;
using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace PH_ServiceCenter.Models
{
    public class GuiaInteraccion
    {
        public int Id { get; set; }
        public int? GuiaId { get; set; }       // Puede ser nulo si es artículo
        public int? ArticuloId { get; set; }   // Nuevo: Puede ser nulo si es guía
        [MaxLength(128)]
        public string UserId { get; set; }
        public bool EsUtil { get; set; }
        public DateTime FechaInteraccion { get; set; }

        // Opcional: para saber el tipo de interacción
        public string Tipo { get; set; } // "Guia" o "Articulo"

        public virtual Guia Guia { get; set; }
        public virtual Biblioteca_Items_New Articulo { get; set; }

        
    }
}
