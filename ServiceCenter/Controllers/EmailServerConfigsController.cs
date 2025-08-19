using System;
using System.Data.Entity;
using System.Net;
using System.Web.Mvc;
using ServiceCenter.Models;
using Microsoft.AspNet.Identity;  

namespace ServiceCenter.Controllers
{
    [Authorize(Roles = "Admin")] 
    public class EmailServerConfigsController : Controller
    {
        private readonly ApplicationDbContext db = new ApplicationDbContext();

        [Authorize]
        public ActionResult Edit(int? id)
        {
            if (id == null)
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);

            var config = db.EmailServerConfig.Find(id);
            if (config == null)
                return HttpNotFound();

            return View(config);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize]
        public ActionResult Edit(EmailServerConfig model)
        {
            if (!ModelState.IsValid)
                return View(model);

            var existing = db.EmailServerConfig.Find(model.Id);
            if (existing == null)
                return HttpNotFound();

 
            existing.Host = model.Host?.Trim();
            existing.Puerto = model.Puerto;
            existing.HabilitarSSL = model.HabilitarSSL;
            existing.Usuario = model.Usuario?.Trim();
            existing.Contrasena = model.Contrasena; 
            existing.FromName = model.FromName?.Trim();
            existing.FromEmail = model.FromEmail?.Trim();
            existing.Provider = model.Provider?.Trim();
            existing.ApiKey = model.ApiKey;
            existing.FechaCreacion = DateTime.Now;


            db.Entry(existing).State = EntityState.Modified;
            db.SaveChanges();

            return RedirectToAction("Edit", new { id = existing.Id });
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing) db.Dispose();
            base.Dispose(disposing);
        }
    }
}
