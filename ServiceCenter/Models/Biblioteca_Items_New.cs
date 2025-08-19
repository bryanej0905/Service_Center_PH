using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace ServiceCenter.Models
{
    public class Biblioteca_Items_New
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "El título es obligatorio")]
        [MinLength(3, ErrorMessage = "El título debe tener al menos 3 caracteres")]
        public string Titulo { get; set; }

        [Required(ErrorMessage = "El contenido es obligatorio")]
        public string Contenido { get; set; }

        [Required(ErrorMessage = "La categoría es obligatoria")]
        public string Categoria { get; set; }

        public string ImagenRuta { get; set; }

        public string Usuario { get; set; }
        public DateTime FechaCreacion { get; set; }
    }
}