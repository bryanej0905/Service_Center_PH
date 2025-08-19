using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace ServiceCenter.Models
{
    public class ReporteErrorGuia
    {
        public int Id { get; set; }

        [Required]
        public int GuiaId { get; set; }

        [Required, Column(TypeName = "nvarchar(max)")]
        public string Descripcion { get; set; }

        [Required, StringLength(256)]
        public string UsuarioNombre { get; set; }

        [Required, StringLength(256)]
        public string UsuarioEmail { get; set; } 

        [Required]
        public DateTime FechaReporte { get; set; }

        [NotMapped]
        [ForeignKey("GuiaId")]
        public virtual Guia Guia { get; set; }
    }

}