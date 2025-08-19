using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace ServiceCenter.Models
{
    public class CategoriaFAQ
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [StringLength(100)]
        [Display(Name = "Categoría")]
        public string Nombre { get; set; }

        
        public virtual ICollection<PreguntaFAQ> Preguntas { get; set; }
    }
}
