using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace ServiceCenter.Models
{
    public class EmailDestinatario
    {
        [Key]
        public int Id { get; set; }

        [StringLength(100)]
        public string Nombre { get; set; }

        [Required, StringLength(256)]
        [Index(IsUnique = true)]
        public string Correo { get; set; }

        public bool Activo { get; set; } = true;

        public DateTime FechaCreacion { get; set; } = DateTime.UtcNow;
    }
}