using Microsoft.AspNet.Identity;
using ServiceCenter.Models;
using ServiceCenter.ViewModels;
using System;
using System.Data.Entity;
using System.Linq;
using System.Threading.Tasks;
using System.Web.Mvc;

namespace ServiceCenter.Controllers
{
    [Authorize]
    public class FAQController : Controller
    {
        private readonly ApplicationDbContext _context = new ApplicationDbContext();

        public ActionResult Index(string category, string searchString)
        {
            string userId = User.Identity.GetUserId();

            
            var query = _context.PreguntasFAQ
                                .Include(p => p.Categoria)
                                .AsQueryable();

            
            if (!string.IsNullOrEmpty(category))
            {
                query = query.Where(p => p.Categoria.Nombre == category);
            }

            
            if (!string.IsNullOrWhiteSpace(searchString))
            {
                var lowerSearch = searchString.ToLower();
                query = query.Where(p =>
                    p.Titulo.ToLower().Contains(lowerSearch) ||
                    p.Respuesta.ToLower().Contains(lowerSearch));
            }

            
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

            
            var categorias = _context.CategoriasFAQ
                                .OrderBy(c => c.Nombre)
                                .Select(c => c.Nombre)
                                .ToList();

            
            var consultasRecientes = _context.ConsultasRecientesFAQ
                .Where(c => c.UsuarioId == userId)
                .OrderByDescending(c => c.Fecha)
                .Take(5)
                .Select(c => new ConsultaRecienteViewModel
                {
                    Titulo = c.PreguntaFAQ.Titulo,
                    PreguntaFAQId = c.PreguntaFAQId
                })
                .ToList();

            
            var vm = new FAQIndexViewModel
            {
                Preguntas = listaPreguntas,
                Categorias = categorias,
                CurrentCategory = category ?? "",
                CurrentFilter = searchString ?? "",
                ConsultasRecientes = consultasRecientes   
            };

            return View(vm);
        }

        [HttpPost]
        public async Task<ActionResult> ConsultarPregunta(int id)
        {
            string userId = User.Identity.GetUserId();

            
            var consulta = new ConsultaRecienteFAQ
            {
                PreguntaFAQId = id,
                UsuarioId = userId,
                Fecha = DateTime.Now
            };

            var recientes = _context.ConsultasRecientesFAQ
                              .Where(c => c.UsuarioId == userId)
                              .OrderByDescending(c => c.Fecha)
                              .ToList();

            // Si ya tiene 5, eliminar la más antigua
            if (recientes.Count >= 5)
            {
                var ultima = recientes.Last();
                _context.ConsultasRecientesFAQ.Remove(ultima);
            }

            _context.ConsultasRecientesFAQ.Add(consulta);
            await _context.SaveChangesAsync();

            
            return new HttpStatusCodeResult(200);
        }
        [HttpPost]
        public async Task<ActionResult> Votar(int preguntaId, bool esUtil)
        {
            var userId = User.Identity.GetUserId();

            var yaVoto = await _context.InteraccionesFAQ
                .AnyAsync(i => i.PreguntaId == preguntaId && i.UserId == userId);

            if (!yaVoto)
            {
                var interaccion = new InteraccionFAQ
                {
                    PreguntaId = preguntaId,
                    UserId = userId,
                    EsUtil = esUtil,
                    Fecha = DateTime.Now
                };

                _context.InteraccionesFAQ.Add(interaccion);
                await _context.SaveChangesAsync();
            }

            return Json(new { success = true });
        }


        [HttpGet]
        public ActionResult GetConsultasRecientes()
        {
            string userId = User.Identity.GetUserId();
            var consultas = _context.ConsultasRecientesFAQ
                .Where(c => c.UsuarioId == userId)
                .OrderByDescending(c => c.Fecha)
                .Take(5)
                .Select(c => new ConsultaRecienteViewModel
                {
                    Titulo = c.PreguntaFAQ.Titulo,
                    PreguntaFAQId = c.PreguntaFAQId
                })
                .ToList();

            return PartialView("_ConsultasRecientesPartial", consultas);
        }
        public async Task<ActionResult> Reportes()
        {
            var interacciones = await _context.InteraccionesFAQ
        .Include(i => i.Pregunta)
        .Include(i => i.Usuario)
        .Select(i => new PreguntaInteraccionViewModel
        {
            PreguntaTitulo = i.Pregunta.Titulo,
            Categoria = i.Pregunta.Categoria.Nombre,
            UsuarioNombre = i.Usuario.UserName,
            EsUtil = i.EsUtil,
            Fecha = i.Fecha
        })
        .OrderByDescending(i => i.Fecha)
        .ToListAsync();

            return View(interacciones);
        }

    }
}