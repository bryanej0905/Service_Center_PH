using System;
using System.Data.Entity;
using System.IO;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;
using Microsoft.AspNet.Identity;
using ServiceCenter.Models;

namespace ServiceCenter.Controllers
{
    [Authorize]
    public class AdjuntosController : Controller
    {
        private ApplicationDbContext db = new ApplicationDbContext();

        // POST: Adjuntos/Subir
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Subir(int ticketId, HttpPostedFileBase archivoAdjunto)
        {
            if (archivoAdjunto != null && archivoAdjunto.ContentLength > 0)
            {
                var fileName = Path.GetFileName(archivoAdjunto.FileName);
                var carpetaDestino = "~/Content/Adjuntos/Tickets/" + ticketId;

                var rutaServidor = Server.MapPath(carpetaDestino);
                if (!Directory.Exists(rutaServidor))
                {
                    Directory.CreateDirectory(rutaServidor);
                }

                var rutaCompleta = Path.Combine(rutaServidor, fileName);
                archivoAdjunto.SaveAs(rutaCompleta);

                var adjunto = new Adjunto
                {
                    TicketId = ticketId,
                    NombreArchivo = fileName,
                    RutaArchivo = Path.Combine(carpetaDestino.TrimStart('~'), fileName).Replace("\\", "/"),
                    FechaSubida = DateTime.Now,
                    SubidoPorId = User.Identity.GetUserId()
                };

                db.Adjuntos.Add(adjunto);
                db.SaveChanges();

                TempData["Mensaje"] = "Archivo subido correctamente.";
            }
            else
            {
                TempData["Error"] = "Debe seleccionar un archivo válido.";
            }

            return RedirectToAction("Details", "Tickets", new { id = ticketId });
        }

        // GET: Adjuntos/Descargar/5
        public ActionResult Descargar(int id)
        {
            var adjunto = db.Adjuntos.Find(id);
            if (adjunto == null)
            {
                return HttpNotFound();
            }

            var rutaCompleta = Server.MapPath("~/" + adjunto.RutaArchivo);
            var nombreArchivo = adjunto.NombreArchivo;

            return File(rutaCompleta, MimeMapping.GetMimeMapping(nombreArchivo), nombreArchivo);
        }

        // POST: Adjuntos/Eliminar/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Eliminar(int id)
        {
            var adjunto = db.Adjuntos.Find(id);
            if (adjunto == null)
            {
                return HttpNotFound();
            }

            var rutaCompleta = Server.MapPath("~/" + adjunto.RutaArchivo);
            if (System.IO.File.Exists(rutaCompleta))
            {
                System.IO.File.Delete(rutaCompleta);
            }

            db.Adjuntos.Remove(adjunto);
            db.SaveChanges();

            TempData["Mensaje"] = "Archivo eliminado correctamente.";
            return RedirectToAction("Details", "Tickets", new { id = adjunto.TicketId });
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                db.Dispose();
            }
            base.Dispose(disposing);
        }
    }
}
