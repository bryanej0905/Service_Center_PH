using Microsoft.AspNet.Identity;
using PH_ServiceCenter.Models;
using Rotativa;
using Rotativa.Options;
using ServiceCenter.Models;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.IO;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;

namespace ServiceCenter.Controllers
{
    [Authorize]
    public class Biblioteca_Items_NewController : Controller
    {
        private ApplicationDbContext db = new ApplicationDbContext();
        private readonly ApplicationDbContext _context;

        public Biblioteca_Items_NewController()
        {
            _context = new ApplicationDbContext();
        }

        // GET: Articulos/Create
        public ActionResult Create()
        {
            return View();
        }

        // POST: Articulos/Create
        [HttpPost]

        public ActionResult Create([Bind(Include = "Titulo,Contenido,Categoria")] Biblioteca_Items_New Biblioteca_Items_New, HttpPostedFileBase ImagenUpload)
        {
            System.Diagnostics.Debug.WriteLine(">>>>> LLEGÓ AL MÉTODO POST CREATE <<<<<");
            System.Diagnostics.Debug.WriteLine("ImagenUpload es null?: " + (ImagenUpload == null));

            if (ImagenUpload == null)
            {
                ModelState.AddModelError("ImagenRuta", "La imagen es obligatoria");
            }
            else if (!ImagenUpload.ContentType.StartsWith("image/"))
            {
                ModelState.AddModelError("ImagenRuta", "Imagen no válida");
            }
            else
            {
                var fileName = Guid.NewGuid().ToString() + Path.GetExtension(ImagenUpload.FileName);
                var path = Path.Combine(Server.MapPath("~/Content/Articulos"), fileName);
                Directory.CreateDirectory(Server.MapPath("~/Content/Articulos"));
                ImagenUpload.SaveAs(path);
                Biblioteca_Items_New.ImagenRuta = "/Content/Articulos/" + fileName;
            }

            Biblioteca_Items_New.Usuario = User.Identity.IsAuthenticated ? User.Identity.Name : "Anonimo";

            if (ModelState.IsValid)
            {
                Biblioteca_Items_New.FechaCreacion = DateTime.Now;
                db.Biblioteca_Items_New.Add(Biblioteca_Items_New);
                db.SaveChanges();
                return RedirectToAction("Index", "Biblioteca");
            }

            return View(Biblioteca_Items_New);
        }

        // GET: Articulos/Index
        public ActionResult Index()
        {
            return View(db.Biblioteca_Items_New.ToList());
        }

        [HttpGet]
        [AllowAnonymous]
        public ActionResult DescargarPDF(int id)
        {
            var articulo = db.Biblioteca_Items_New.Find(id);

            if (articulo == null)
                return HttpNotFound();

            ViewBag.FechaGeneracion = DateTime.Now.ToString("dd/MM/yyyy");
            string pdfTitle = $"{articulo.Titulo.Replace(' ', '_')}.pdf";

            return new ViewAsPdf("~/Views/Biblioteca_Items_New/GuiaPDF.cshtml", articulo)
            {
                FileName = pdfTitle,
                PageSize = Size.A4,
                PageOrientation = Orientation.Portrait,
            };
        }
        // GET: Biblioteca_Items_New/Details/5
        public async Task<ActionResult> Details(int? id)
        {
            if (id == null) return HttpNotFound();

            var articulo = await db.Biblioteca_Items_New.FindAsync(id);
            if (articulo == null) return HttpNotFound();

            var userId = User.Identity.GetUserId();

            var interaccionUsuario = db.GuiaInteracciones.FirstOrDefault(i => i.ArticuloId == articulo.Id && i.UserId == userId);

            ViewBag.InteraccionUsuario = interaccionUsuario;
            ViewBag.UtilCount = db.GuiaInteracciones.Count(i => i.ArticuloId == articulo.Id && i.EsUtil);
            ViewBag.NoUtilCount = db.GuiaInteracciones.Count(i => i.ArticuloId == articulo.Id && !i.EsUtil);

            return View(articulo);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Rate(int articuloId, bool isUseful)
        {
            var articulo = db.Biblioteca_Items_New.Find(articuloId);
            if (articulo == null) return HttpNotFound();

            var userId = User.Identity.GetUserId();

            var yaVoto = db.GuiaInteracciones.Any(i => i.ArticuloId == articuloId && i.UserId == userId);

            if (yaVoto)
            {
                TempData["Mensaje"] = "Ya votaste este artículo.";
                return RedirectToAction("Details", new { id = articuloId });
            }

            var interaccion = new GuiaInteraccion
            {
                ArticuloId = articuloId,
                UserId = userId,
                EsUtil = isUseful,
                FechaInteraccion = DateTime.Now,
                Tipo = "Articulo"
            };

            db.GuiaInteracciones.Add(interaccion);
            db.SaveChanges();

            return RedirectToAction("Details", new { id = articuloId });
        }

   
            [AllowAnonymous]
            [HttpPost]
            [ValidateAntiForgeryToken]
            public async Task<ActionResult> ReportarError(int guiaId, string descripcion)
            {
                try
                {
                    var userId = User.Identity.GetUserId();
                    var user = await _context.Users.SingleOrDefaultAsync(u => u.Id == userId);
                    var userName = user?.UserName ?? User.Identity.Name;
                    var userEmail = user?.Email ?? "";
                var guia = await _context.Biblioteca_Items_New.FindAsync(guiaId);
                if (guia == null)
                {
                    TempData["MensajeError"] = "Artículo no encontrado.";
                    return RedirectToAction("Details", new { id = guiaId });
                }

                var reporte = new ReporteErrorGuia
                    {
                        GuiaId = guiaId,
                        Descripcion = descripcion,
                        UsuarioNombre = userName,
                        UsuarioEmail = userEmail,
                        FechaReporte = DateTime.Now 
                    };
                    _context.ReportesError.Add(reporte);
                    await _context.SaveChangesAsync();
                
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
                var innerMessage = ex.InnerException?.Message ?? ex.Message;
                System.Diagnostics.Debug.WriteLine("ERROR EN ReportarError: " + innerMessage);
                return Json(new { success = false, error = innerMessage });
            }

        }


    }
}



