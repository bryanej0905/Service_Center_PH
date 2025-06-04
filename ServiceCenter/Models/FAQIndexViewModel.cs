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
        /// <summary> La lista de preguntas a mostrar </summary>
        public List<PreguntaViewModel> Preguntas { get; set; }

        /// <summary> Todas las categorías posibles (para generar el menú lateral). </summary>
        public List<string> Categorias { get; set; }

        /// <summary> La categoría actualmente seleccionada (o cadena vacía para "Todas"). </summary>
        public string CurrentCategory { get; set; }
        public string CurrentFilter { get; set; }
    }
}
