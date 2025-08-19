using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ServiceCenter.Models
{
    public class PreguntaReporteViewModel
    {
        public int PreguntaId { get; set; }
        public string Titulo { get; set; }
        public string Categoria { get; set; }
        public int TotalPositivos { get; set; }
        public int TotalNegativos { get; set; }
    }
}
