using ServiceCenter.Models;
using System;
using System.Data.Entity;
using System.Linq;
using System.Threading.Tasks;
using System.Web.Mvc;
using Microsoft.AspNet.Identity;
using ServiceCenter.ViewModels;


namespace ServiceCenter.Controllers
{
    [Authorize]
    public class SugerenciasFAQController : Controller
    {
        private readonly ApplicationDbContext _context;

        public SugerenciasFAQController()
        {
            _context = new ApplicationDbContext();
        }




        [HttpGet]
        [Authorize]
        public async Task<ActionResult> Pendientes()
        {
            var pendientes = await _context.SugerenciasPreguntasFAQ
                .Where(s => s.Estado == EstadoSugerencia.Pendiente)
                .Include(s => s.CategoriaFAQ)
                .OrderByDescending(s => s.FechaSugerencia)
                .ToListAsync();

            return View(pendientes);
        }

        [HttpGet]
        public ActionResult Sugerir()
        {
            ViewBag.Categorias = new SelectList(_context.CategoriasFAQ.ToList(), "Id", "Nombre");
            return View();
        }

        
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Sugerir(SugerenciaPreguntasFAQ model)
        {
            if (ModelState.IsValid)
            {
                model.FechaSugerencia = DateTime.Now;
                model.Estado = EstadoSugerencia.Pendiente;
                model.UsuarioId = User.Identity.GetUserId();

                _context.SugerenciasPreguntasFAQ.Add(model);
                await _context.SaveChangesAsync();
                TempData["Mensaje"] = "¡Tu sugerencia fue enviada correctamente y está pendiente de revisión!";
                return RedirectToAction("Sugerir");
            }
            // Si hay error, recarga categorías
            ViewBag.Categorias = new SelectList(_context.CategoriasFAQ.ToList(), "Id", "Nombre");
            return View(model);
        }

        
        [HttpGet]
        [Authorize]
        public async Task<ActionResult> PanelAdmin()
        {
            // Cargar sugerencias y traer el usuario revisor
            var sugerenciasRaw = await _context.SugerenciasPreguntasFAQ
                .Where(s => s.Estado != EstadoSugerencia.Pendiente)
                .OrderByDescending(s => s.FechaSugerencia)
                .ToListAsync();

            // Traer los usuarios revisores
            var revisoresIds = sugerenciasRaw
                .Where(s => !string.IsNullOrEmpty(s.UsuarioRevisorId))
                .Select(s => s.UsuarioRevisorId)
                .Distinct()
                .ToList();

            var revisoresDict = _context.Users
                .Where(u => revisoresIds.Contains(u.Id))
                .ToDictionary(u => u.Id, u => u.UserName); // O u.NombreCompleto si tienes el campo

            // Llenar el ViewModel para la vista
            var sugerencias = sugerenciasRaw.Select(s => new SugerenciaFAQViewModel
            {
                Pregunta = s.Pregunta,
                Estado = s.Estado.ToString(),
                Fecha = s.FechaRevision ?? s.FechaSugerencia,
                AprobadoPor = !string.IsNullOrEmpty(s.UsuarioRevisorId) && revisoresDict.ContainsKey(s.UsuarioRevisorId)
                                ? revisoresDict[s.UsuarioRevisorId]
                                : ""
            }).ToList();

            return View(sugerencias);
        }



        [HttpGet]
        [Authorize]
        public async Task<ActionResult> Revisar(int id)
        {
            var sugerencia = await _context.SugerenciasPreguntasFAQ.FindAsync(id);
            if (sugerencia == null)
                return HttpNotFound();

            return View(sugerencia);
        }

        [HttpPost]
        [Authorize]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Revisar(SugerenciaPreguntasFAQ model, string accion)
        {
            var sugerencia = await _context.SugerenciasPreguntasFAQ.FindAsync(model.Id);
    if (sugerencia == null)
        return HttpNotFound();

            sugerencia.FechaRevision = DateTime.Now;
            sugerencia.UsuarioRevisorId = User.Identity.GetUserId();
            sugerencia.Respuesta = model.Respuesta;
            sugerencia.Motivo = model.Motivo;

            if (accion == "Aprobar")
            {
                sugerencia.Estado = EstadoSugerencia.Aprobada;

                // Previene duplicados
                var yaExiste = _context.PreguntasFAQ.Any(p =>
                    p.Titulo == sugerencia.Pregunta && p.CategoriaFAQId == sugerencia.CategoriaFAQId);

                if (!yaExiste)
                {
                    var nuevaPregunta = new PreguntaFAQ
                    {
                        Titulo = sugerencia.Pregunta,
                        Respuesta = sugerencia.Respuesta,
                        CategoriaFAQId = sugerencia.CategoriaFAQId,
                        FechaCreacion = DateTime.Now,
                        VecesConsultada = 0
                    };

                    _context.PreguntasFAQ.Add(nuevaPregunta);
                }
            }
            else if (accion == "Rechazar")
            {
                sugerencia.Estado = EstadoSugerencia.Rechazada;
            }




            await _context.SaveChangesAsync();
            TempData["MensajeAdmin"] = "¡Sugerencia gestionada correctamente!";
            return RedirectToAction("PanelAdmin");
        }
    }
}
