using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ServiceCenter.Models
{
    public class ConsultaRecienteFAQ
    {
        public int Id { get; set; }

        [Required]
        public int PreguntaFAQId { get; set; }
        [ForeignKey("PreguntaFAQId")]
        public virtual PreguntaFAQ PreguntaFAQ { get; set; }

        [Required]
        public string UsuarioId { get; set; }
        [ForeignKey("UsuarioId")]
        public virtual ApplicationUser Usuario { get; set; }

        [Required]
        public DateTime Fecha { get; set; }
    }
}
