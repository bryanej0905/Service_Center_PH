using Microsoft.AspNet.Identity;
using PH_ServiceCenter.Models;
using Rotativa;
using Rotativa.Options;
using ServiceCenter.Models;
using System;
using System.Data.Entity;
using System.Linq;
using System.Threading.Tasks;
using System.Web.Mvc;
using System.Net.Mail;
using System.Net;
using ServiceCenter.Helpers;
using System.Security.Cryptography;
using System.Text;
using SendGrid.Helpers.Mail;
using SendGrid;
using ServiceCenter.Services;


namespace ServiceCenter.Controllers
{
    [Authorize]
    public class BibliotecaController : Controller
    {
        private readonly ApplicationDbContext _context;

        public BibliotecaController()
        {
            _context = new ApplicationDbContext();
        }

        [HttpGet]
        public async Task<ActionResult> Index(string searchString)
        {
            var query = _context.Guias.Include(g => g.Categoria).AsQueryable();

            if (!string.IsNullOrWhiteSpace(searchString))
            {
                query = query.Where(g =>
                    g.Titulo.Contains(searchString) ||
                    g.Contenido.Contains(searchString) ||
                    g.Keywords.Contains(searchString)
                );
            }

            var listaGuias = await query.ToListAsync();
            var listaArticulos = await _context.Biblioteca_Items_New.ToListAsync();

            // Mapear guías a ViewModel
            var listaCombinada = listaGuias.Select(g => new BibliotecaItemVM
            {
                Id = g.Id,
                Titulo = g.Titulo,
                Categoria = g.Categoria?.Nombre ?? "Sin Categoría",
                Contenido = g.Contenido,
                FechaCreacion = DateTime.Now, // Cambia esto si tienes fecha real en Guia
                Tipo = "Guia"
            }).ToList();

            // Mapear artículos a ViewModel
            listaCombinada.AddRange(listaArticulos.Select(a => new BibliotecaItemVM
            {
                Id = a.Id,
                Titulo = a.Titulo,
                Categoria = a.Categoria ?? "Sin Categoría",
                Contenido = a.Contenido,
                FechaCreacion = a.FechaCreacion,
                Tipo = "Articulo"
            }));

            // Ordenar por fecha de creación, recientes primero
            listaCombinada = listaCombinada.OrderByDescending(x => x.FechaCreacion).ToList();

            return View(listaCombinada);
        }

        [HttpGet]
        public async Task<ActionResult> Details(int? id)
        {
            if (id == null)
                return HttpNotFound();

            var guia = await _context.Guias
                                     .Include(g => g.Categoria)
                                     .Include(g => g.Interacciones) 
                                     .FirstOrDefaultAsync(g => g.Id == id.Value);

            if (guia == null)
                return HttpNotFound();

            var userId = User.Identity.GetUserId();

            
            var interaccionUsuario = await _context.GuiaInteracciones
                .FirstOrDefaultAsync(i => i.GuiaId == guia.Id && i.UserId == userId);

            ViewBag.InteraccionUsuario = interaccionUsuario;
            ViewBag.UtilCount = guia.Interacciones.Count(i => i.EsUtil);
            ViewBag.NoUtilCount = guia.Interacciones.Count(i => !i.EsUtil);

            return View(guia);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Rate(int id, bool isUseful)
        {
            var guia = await _context.Guias.FindAsync(id);
            if (guia == null)
                return HttpNotFound();

            var userId = User.Identity.GetUserId();

            var yaVoto = await _context.GuiaInteracciones
                .AnyAsync(i => i.GuiaId == id && i.UserId == userId);

            if (yaVoto)
            {
                TempData["Mensaje"] = "Ya votaste esta guía.";
                return RedirectToAction("Details", new { id });
            }

            var interaccion = new GuiaInteraccion
            {
                GuiaId = id,
                UserId = userId,
                EsUtil = isUseful,
                FechaInteraccion = DateTime.Now
            };

            _context.GuiaInteracciones.Add(interaccion);
            await _context.SaveChangesAsync();

            return RedirectToAction("Details", new { id });
        }

        [HttpGet]
        [AllowAnonymous]
        public async Task<ActionResult> DescargarPDF(int id)
        {
            var guia = await _context.Guias
                                     .Include(g => g.Categoria)
                                     .FirstOrDefaultAsync(g => g.Id == id);

            if (guia == null)
                return HttpNotFound();

            ViewBag.FechaGeneracion = DateTime.Now.ToString("dd/MM/yyyy");
            ViewBag.Version = DateTime.Now.ToString("yyMM");

            string pdfTitle = $"{guia.Titulo.Replace(' ', '_')}.pdf";
            string headerUrl = $"{Request.Url.Scheme}://{Request.Url.Host}:{Request.Url.Port}/Biblioteca/PdfHeader";

            return new ViewAsPdf("GuiaPDF", guia)
            {
                FileName = pdfTitle,
                PageSize = Size.A4,
                PageOrientation = Orientation.Portrait,
            };
        }

        [AllowAnonymous]
        public ActionResult PdfHeader()
        {
            return View();
        }

        [AllowAnonymous]
        [HttpPost]     
        public async Task<ActionResult> ReportarError(int guiaId, string descripcion)
        {
            try
            {
                var userId = User.Identity.GetUserId();
                var user = await _context.Users.SingleOrDefaultAsync(u => u.Id == userId);
                var userName = user?.UserName ?? User.Identity.Name;
                var userEmail = user?.Email ?? "";

                var reporte = new ReporteErrorGuia
                {
                    GuiaId = guiaId,
                    Descripcion = descripcion,
                    UsuarioNombre = userName,
                    UsuarioEmail = userEmail,
                };
                _context.ReportesError.Add(reporte);
                await _context.SaveChangesAsync();
                var guia = await _context.Guias.FindAsync(guiaId);
                var tituloGuia = guia?.Titulo ?? "Guía desconocida";

                var body = $@"
<html>
  <body style=""font-family:Arial,sans-serif;color:#333;margin:0;padding:20px;"">
    <h2 style=""color:#c00;border-bottom:2px solid #c00;padding-bottom:5px;"">
      📚 Reporte de Error en Biblioteca
    </h2>
    <p><strong>Artículo:</strong> {tituloGuia}</p>
    <p>El usuario {userName}, ha reportado el artículo antes mencionado
      con el siguiente detalle.</p>
      
    </p>
    <p><strong>Descripción del problema:</strong></p>
    <div style=""background:#f5f5f5;padding:15px;border-radius:5px;margin-bottom:15px;"">
      {descripcion}
    </div>
    <p><small style=""color:#666;"">
      Fecha del reporte: {reporte.FechaReporte:dd/MM/yyyy HH:mm}
    </small></p>
  </body>
</html>
";

                var subject = $"[Biblioteca] Error en guía: {tituloGuia}";

                var emailSender = new ServiceCenter.Services.EmailSender(_context);
                await emailSender.SendEmailAsync(notificar: true, subject, body, replyToEmail: userEmail);


                return Json(new { success = true });
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine("ERROR EN ReportarError: " + ex);
                return Json(new { success = false, error = ex.Message });
            }
        }

       



        protected override void Dispose(bool disposing)
        {
            if (disposing) _context.Dispose();
            base.Dispose(disposing);
        }
    }
}