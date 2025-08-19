using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ServiceCenter.Models
{
    public class BibliotecaItemVM
    {
        public int Id { get; set; }
        public string Titulo { get; set; }
        public string Categoria { get; set; }
        public string Contenido { get; set; }
        public DateTime FechaCreacion { get; set; }
        public string Tipo { get; set; } 
    }
}
