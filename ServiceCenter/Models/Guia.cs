using PH_ServiceCenter.Models;
using System.Collections.Generic;
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
        public int Version { get; set; }

        [Required]
        public string Contenido { get; set; }

        [StringLength(500)]
        public string Keywords { get; set; }

        // Relación a Categoría
        [Display(Name = "Categoría")]
        public int CategoriaId { get; set; }

        [ForeignKey("CategoriaId")]
        public virtual Categoria Categoria { get; set; }
    
        public virtual ICollection<GuiaInteraccion> Interacciones { get; set; }
    }
}
