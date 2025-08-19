using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;
using ServiceCenter.Models;

namespace ServiceCenter.Controllers
{
    [Authorize(Roles = "Admin")]
    public class EmailDestinatariosController : Controller
    {
        private ApplicationDbContext db = new ApplicationDbContext();

        public ActionResult Index()
        {
            return View(db.EmailDestinatarios.ToList());
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        public JsonResult CreateInline(string Nombre, string Correo, bool Activo)
        {
            try
            {
                var entity = new EmailDestinatario
                {
                    Nombre = Nombre,
                    Correo = Correo,
                    Activo = Activo,
                    FechaCreacion = DateTime.Now
                };
                db.EmailDestinatarios.Add(entity);
                db.SaveChanges();

                return Json(new
                {
                    success = true,
                    data = new
                    {
                        entity.Id,
                        entity.Nombre,
                        entity.Correo,
                        entity.Activo,
                        FechaCreacion = entity.FechaCreacion.ToString("dd/MM/yyyy HH:mm")
                    }
                });
            }
            catch
            {
                return Json(new { success = false, message = "Error en el servidor al crear." });
            }
        }


        [HttpPost]
        public JsonResult EditInline(int id, string Nombre, string Correo, bool Activo)
        {
            try
            {
                var destinatario = db.EmailDestinatarios.Find(id);
                if (destinatario == null)
                    return Json(new { success = false });

                destinatario.Nombre = Nombre;
                destinatario.Correo = Correo;
                destinatario.Activo = Activo;
                db.SaveChanges();

                return Json(new { success = true });
            }
            catch
            {
                return Json(new { success = false });
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteInline(int id)
        {
            try
            {
                var entity = db.EmailDestinatarios.Find(id);
                if (entity == null)
                    return Json(new { success = false, message = "Registro no encontrado." });

                db.EmailDestinatarios.Remove(entity);
                db.SaveChanges();
                return Json(new { success = true });
            }
            catch (Exception ex)
            {
                // TODO: log ex
                return Json(new { success = false, message = "Error en servidor al eliminar." });
            }
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
