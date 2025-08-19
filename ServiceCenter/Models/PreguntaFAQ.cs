using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ServiceCenter.Models
{
    public class PreguntaFAQ
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [StringLength(200)]
        [Display(Name = "Pregunta")]
        public string Titulo { get; set; }

        [Required]
        [Display(Name = "Respuesta")]
        public string Respuesta { get; set; }

        // Clave foránea hacia CategoriaFAQ
        [Required]
        public int CategoriaFAQId { get; set; }

        // Propiedad de navegación
        [ForeignKey("CategoriaFAQId")]
        public virtual CategoriaFAQ Categoria { get; set; }

        // fecha de creación de la pregunta
        [Display(Name = "Fecha Creación")]
        public DateTime FechaCreacion { get; set; }

        // número de veces que se ha consultado esta pregunta
        [Display(Name = "Veces Consultada")]
        public int VecesConsultada { get; set; }
        public virtual ICollection<InteraccionFAQ> InteraccionesFAQ { get; set; }
    }
}