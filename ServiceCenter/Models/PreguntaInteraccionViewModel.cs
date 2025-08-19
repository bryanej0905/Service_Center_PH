using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ServiceCenter.Models
{
    public class PreguntaInteraccionViewModel
    {
        public string PreguntaTitulo { get; set; }
        public string Categoria { get; set; }
        public string UsuarioNombre { get; set; }
        public bool EsUtil { get; set; }
        public DateTime Fecha { get; set; }
    }
}
