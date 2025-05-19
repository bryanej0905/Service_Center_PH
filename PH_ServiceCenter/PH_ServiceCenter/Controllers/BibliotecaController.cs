using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Web.Mvc;
using ActionResult = Microsoft.AspNetCore.Mvc.ActionResult;


namespace PH_ServiceCenter.Controllers
{
    public class BibliotecaController : Microsoft.AspNetCore.Mvc.Controller
    {
        // Simulación de datos
        private static List<ArticuloGuia> articulos = new List<ArticuloGuia>
        {
            new ArticuloGuia { Id = 1, Titulo = "Cómo preparar una pizza", Descripcion = "Instrucciones generales para preparar una pizza desde cero.", Tematica = "Producción" },
            new ArticuloGuia { Id = 2, Titulo = "Cierre de caja diario", Descripcion = "Proceso general para realizar el cierre de caja al finalizar el día.", Tematica = "Finanzas" },
            new ArticuloGuia { Id = 3, Titulo = "Preparación de Pizza Suprema", Descripcion = "Ingredientes y proceso específico de la pizza suprema.", Tematica = "Productos estrella" },
            new ArticuloGuia { Id = 4, Titulo = "Políticas internas y de atención", Descripcion = "Normativas básicas de conducta y presentación.", Tematica = "Normativas" }

        };

        // GET: Biblioteca
        public Microsoft.AspNetCore.Mvc.ActionResult Index()
        {
            return View(articulos);
        }

        // GET: Biblioteca/Detalle/1
        public ActionResult Detalle(int id)
        {
            var articulo = articulos.Find(x => x.Id == id);
            if (articulo == null)
                return NotFound();

            return View(articulo);
        }

        // GET: Biblioteca/Administrar
        [System.Web.Mvc.Authorize(Roles = "Administrador")]
        public ActionResult Administrar()
        {
            return View();
        }

        // POST: Biblioteca/Guardar
        [Microsoft.AspNetCore.Mvc.HttpPost]
        [System.Web.Mvc.Authorize(Roles = "Administrador")]
        public ActionResult Guardar(ArticuloGuia model)
        {
            if (ModelState.IsValid)
            {
                model.Id = articulos.Count + 1;
                articulos.Add(model);
                return RedirectToAction("Index");
            }

            return View("Administrar");
        }
    }

    // Modelo provisional
    public class ArticuloGuia
    {
        public int Id { get; set; }
        public string Titulo { get; set; }
        public string Descripcion { get; set; }
        public string Tematica { get; set; }
    }
}