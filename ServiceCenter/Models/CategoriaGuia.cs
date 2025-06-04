using System.ComponentModel.DataAnnotations;
using System.Collections.Generic;

namespace ServiceCenter.Models
{
    public class Categoria
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [StringLength(100)]
        public string Nombre { get; set; }

        // Una categoría puede tener varias guías
        public virtual ICollection<Guia> Guias { get; set; }
    }
}
