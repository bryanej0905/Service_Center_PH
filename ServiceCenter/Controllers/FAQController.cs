using System.Collections.Generic;
using System.Data.Entity;    // o Microsoft.EntityFrameworkCore si usas EF Core
using System.Linq;
using System.Web.Mvc;
using ServiceCenter.Models;      // Aquí están PreguntaFAQ y CategoriaFAQ
using ServiceCenter.ViewModels; // Aquí están FAQIndexViewModel y PreguntaViewModel

namespace ServiceCenter.Controllers
{
    public class FAQController : Controller
    {
        private readonly ApplicationDbContext _context = new ApplicationDbContext();

        public ActionResult Index(string category, string searchString)
        {
            // 1) Consulta base: incluimos p.Categoria para poder filtrar por p.Categoria.Nombre
            var query = _context.PreguntasFAQ
                                .Include(p => p.Categoria)
                                .AsQueryable();

            // 2) Filtro por categoría, si hubo clic en alguna
            if (!string.IsNullOrEmpty(category))
            {
                query = query.Where(p => p.Categoria.Nombre == category);
            }

            // 3) Filtro por texto (searchString), si el usuario buscó algo
            if (!string.IsNullOrWhiteSpace(searchString))
            {
                var lowerSearch = searchString.ToLower();
                query = query.Where(p =>
                    p.Titulo.ToLower().Contains(lowerSearch) ||
                    p.Respuesta.ToLower().Contains(lowerSearch));
            }

            // 4) Proyectamos a tu PreguntaViewModel
            var listaPreguntas = query
                .OrderBy(p => p.Id)
                .Select(p => new PreguntaViewModel
                {
                    Id = p.Id,
                    Titulo = p.Titulo,
                    Respuesta = p.Respuesta,
                    Categoria = p.Categoria.Nombre
                })
                .ToList();

            // 5) <<< Aquí viene el cambio importante: 
            //     obtenemos las categorías directamente de la tabla CategoriaFAQ >>>
            var categorias = _context.CategoriasFAQ
                                .OrderBy(c => c.Nombre)
                                .Select(c => c.Nombre)
                                .ToList();

            // 6) Armamos el ViewModel, incluyendo CurrentFilter y CurrentCategory
            var vm = new FAQIndexViewModel
            {
                Preguntas = listaPreguntas,
                Categorias = categorias,
                CurrentCategory = category ?? "",
                CurrentFilter = searchString ?? ""
            };

            return View(vm);
        }

    }
}