using System;
using System.ComponentModel.DataAnnotations;

namespace ServiceCenter.Models
{
    public enum EstadoSugerencia
    {
        Pendiente = 0,
        Aprobada = 1,
        Rechazada = 2
    }

    public class SugerenciaPreguntasFAQ
    {
        public int Id { get; set; }

        [Required]
        [StringLength(500)]
        [Display(Name = "Pregunta sugerida")]

        public string Pregunta { get; set; }

        [StringLength(1000)]
        [Display(Name = "Respuesta sugerida")]
        public string Respuesta { get; set; }

        [Required]
        [Display(Name = "Categoría")]
        public int CategoriaFAQId { get; set; } // FK

        public virtual CategoriaFAQ CategoriaFAQ { get; set; } 

        [Display(Name = "Usuario que sugiere")]
        public string UsuarioId { get; set; }

        [Display(Name = "Fecha de sugerencia")]
        public DateTime FechaSugerencia { get; set; }

        [Display(Name = "Estado")]
        public EstadoSugerencia Estado { get; set; }

        [Display(Name = "Usuario que revisa")]
        public string UsuarioRevisorId { get; set; }

        [Display(Name = "Fecha de revisión")]
        public DateTime? FechaRevision { get; set; }

        [StringLength(500)]
        [Display(Name = "Motivo de rechazo/modificación")]
        public string Motivo { get; set; }
    }
}
