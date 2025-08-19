using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Web;
using System.Web.Mvc;
using Microsoft.AspNet.Identity;
using ServiceCenter.Models;
using static System.Windows.Forms.VisualStyles.VisualStyleElement.TrackBar;

namespace ServiceCenter.Controllers
{
    [Authorize(Roles = "Admin")]
    public class ManageIPChatBotsController : Controller
    {
        private ApplicationDbContext db = new ApplicationDbContext();

        public ActionResult GetUrl()
        {
            var config = db.ManageIPChatBot.FirstOrDefault();

            if (config == null)
            {
                return Json(new { success = false, message = "No hay configuración disponible." }, JsonRequestBehavior.AllowGet);
            }

            return Json(new { success = true, url = config.UrlBase }, JsonRequestBehavior.AllowGet);
        }

        [Authorize]
        public ActionResult Edit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            var manageIPChatBot = db.ManageIPChatBot.Find(id);
            if (manageIPChatBot == null)
            {
                return HttpNotFound();
            }
          
            return View(manageIPChatBot);
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(ManageIPChatBot manageIPChatBot)
        {
            var userId = User.Identity.GetUserId();
            var creador = db.Users.Find(userId);
            manageIPChatBot.CreadoPor = creador != null
                ? (!string.IsNullOrWhiteSpace(creador.Nombre) ? creador.Nombre : creador.UserName)
                : "Desconocido";
            manageIPChatBot.FechaCreacion = DateTime.Now;

            if (ModelState.IsValid)
            {
                db.Entry(manageIPChatBot).State = EntityState.Modified;              
                db.SaveChanges();
                return RedirectToAction("Edit", new { id = manageIPChatBot.Id });
            }
            return View(manageIPChatBot);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public JsonResult ToggleActivo(int id, bool activo)
        {
            var registro = db.EmailDestinatarios.Find(id);
            if (registro == null)
                return Json(new { ok = false, message = "Registro no encontrado." });

            registro.Activo = activo;
            db.SaveChanges();

            return Json(new { ok = true, activo = registro.Activo });
        }

        public ActionResult TestConnection(string ip, string puerto)
        {
            try
            {
                using (var client = new TcpClient())
                {
                    var connectTask = client.ConnectAsync(ip, int.Parse(puerto));
                    bool success = connectTask.Wait(TimeSpan.FromSeconds(5));

                    if (success && client.Connected)
                        return Content("Conexión exitosa.");
                    else
                        return Content("Tiempo de espera agotado. No se pudo conectar al bot.");
                }
            }
            catch
            {
                return Content("Error al intentar conectar al bot.");
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
