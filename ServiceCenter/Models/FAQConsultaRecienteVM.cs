using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ServiceCenter.Models
{
    public class FAQConsultaRecienteVM
    {
        public int PreguntaId { get; set; }
        public string Titulo { get; set; }
        public DateTime Fecha { get; set; }
        public int PreguntaFAQsId { get; set; }

    }
}




