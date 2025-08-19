using ServiceCenter.Models;
using System.Collections.Generic;

namespace ServiceCenter.ViewModels
{
    public class PreguntaViewModel
    {
        public int Id { get; set; }
        public string Titulo { get; set; }
        public string Respuesta { get; set; }
        public string Categoria { get; set; }

    }

    public class FAQIndexViewModel
    {
        /// La lista de preguntas 
        public List<PreguntaViewModel> Preguntas { get; set; }

        /// Todas las categorías posibles (para generar el menú lateral).
        public List<string> Categorias { get; set; }

        /// La categoría actualmente seleccionada (o cadena vacía para "Todas"). 
        public string CurrentCategory { get; set; }
        public string CurrentFilter { get; set; }
        public List<ConsultaRecienteViewModel> ConsultasRecientes { get; set; }
    }
}

