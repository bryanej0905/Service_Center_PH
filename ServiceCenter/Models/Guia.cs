using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ServiceCenter.Models
{
    public class Guia
    {
        [Key]
        public int Id { get; set; }

        [Required, StringLength(200)]
        public string Titulo { get; set; }

        [Required]
        public string Contenido { get; set; }

        [StringLength(500)]
        public string Keywords { get; set; }

        // Relación a Categoría (1‐a‐muchos)
        [Display(Name = "Categoría")]
        public int CategoriaId { get; set; }

        [ForeignKey("CategoriaId")]
        public virtual Categoria Categoria { get; set; }

        // Contadores de calificaciones
        public int UsefulCount { get; set; }
        public int NotUsefulCount { get; set; }
    }
}
