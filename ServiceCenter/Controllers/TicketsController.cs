using System;
using System.Data.Entity;
using System.IO;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;
using Microsoft.AspNet.Identity;
using ServiceCenter.Models;
using ServiceCenter.ViewModels;

namespace ServiceCenter.Controllers
{
    public class TicketsController : Controller
    {
        private ApplicationDbContext db = new ApplicationDbContext();

        // GET: Tickets
        public ActionResult Index()
        {
            var usuarioId = User.Identity.GetUserId();

            var model = db.Tickets
                .Where(t => t.UsuarioCreadorId == usuarioId)
                .Include(t => t.Departamento)
                .Include(t => t.UsuarioCreador)
                .Select(t => new TicketListViewModel
                {
                    Id = t.Id,
                    Titulo = t.Titulo,
                    Estado = t.EstadoString,
                    Prioridad = t.PrioridadString,
                    Categoria = t.CategoriaString,
                    DepartamentoNombre = t.Departamento.Nombre,
                    UsuarioCreadorNombre = t.UsuarioCreador.UserName
                }).ToList();

            return View(model);
        }


        // GET: Tickets/Create
        public ActionResult Create()
        {
            ViewBag.DepartamentoId = new SelectList(db.Departamentos, "Id", "Nombre");
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(Ticket ticket, HttpPostedFileBase archivoAdjunto)
        {
            if (ModelState.IsValid)
            {
                var userId = User.Identity.GetUserId();
                ticket.UsuarioCreadorId = userId;
                ticket.FechaCreacion = DateTime.Now;

                db.Tickets.Add(ticket);
                db.SaveChanges(); // Primero guarda para tener el ticket.Id

                // Si hay archivo, guardar el adjunto
                if (archivoAdjunto != null && archivoAdjunto.ContentLength > 0)
                {
                    var fileName ="Adjunto_Ticket_" + ticket.Id + "_" + DateTime.Now.ToString("ddMMyyyy");
                    var carpetaDestino = "~/Content/Adjuntos/Tickets/" + ticket.Id;

                    var rutaServidor = Server.MapPath(carpetaDestino);
                    if (!Directory.Exists(rutaServidor))
                    {
                        Directory.CreateDirectory(rutaServidor);
                    }

                    var rutaCompleta = Path.Combine(rutaServidor, fileName);
                    archivoAdjunto.SaveAs(rutaCompleta);

                    var adjunto = new Adjunto
                    {
                        TicketId = ticket.Id,
                        NombreArchivo = fileName,
                        RutaArchivo = Path.Combine(carpetaDestino.TrimStart('~'), fileName).Replace("\\", "/"),
                        FechaSubida = DateTime.Now,
                        SubidoPorId = userId
                    };

                    db.Adjuntos.Add(adjunto);
                    db.SaveChanges();
                }

                return RedirectToAction("Index");
            }

            ViewBag.DepartamentoId = new SelectList(db.Departamentos, "Id", "Nombre", ticket.DepartamentoId);
            return View(ticket);
        }


        // GET: Tickets/Details/5
        public ActionResult Details(int id)
        {
            var ticket = db.Tickets
                .Include(t => t.Departamento)
                .Include(t => t.UsuarioCreador)
                .FirstOrDefault(t => t.Id == id);

            if (ticket == null)
            {
                return HttpNotFound();
            }

            var adjuntos = db.Adjuntos
                .Where(a => a.TicketId == id)
                .ToList();

            var viewModel = new TicketDetalleViewModel
            {
                Id = ticket.Id,
                Titulo = ticket.Titulo,
                Descripcion = ticket.Descripcion,
                Estado = ticket.Estado.ToString(),
                Prioridad = ticket.Prioridad.ToString(),
                Categoria = ticket.Categoria.ToString(),
                DepartamentoNombre = ticket.Departamento?.Nombre,
                UsuarioCreadorUserName = ticket.UsuarioCreador?.UserName,
                FechaCreacion = ticket.FechaCreacion,
                Adjuntos = adjuntos
            };

            return View(viewModel);
        }



        // GET: Tickets/Delete/5
        public ActionResult Delete(int? id)
        {
            if (id == null) return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            Ticket ticket = db.Tickets.Find(id);
            if (ticket == null) return HttpNotFound();
            return View(ticket);
        }

        // POST: Tickets/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            Ticket ticket = db.Tickets.Find(id);
            db.Tickets.Remove(ticket);
            db.SaveChanges();
            return RedirectToAction("Index");
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
