using Microsoft.AspNet.Identity;
using ServiceCenter.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace ServiceCenter.Controllers
{
    [Authorize]
    public class HomeController : Controller
    {
        private readonly ApplicationDbContext db;

        public HomeController()
        {
            db = new ApplicationDbContext();
        }

        public ActionResult Index()
        {
            var userId = User.Identity.GetUserId();

            ViewBag.TotalTickets = db.Tickets
                .Count(t => t.UsuarioCreadorId == userId);

            ViewBag.TicketsAbiertos = db.Tickets
                .Count(t => t.UsuarioCreadorId == userId
                         && t.EstadoString != EnumEstadoTicket.Cerrado.ToString()
                         && t.EstadoString != EnumEstadoTicket.Cerrado_por_Usuario.ToString());

            ViewBag.TicketsCerrados = db.Tickets
                .Count(t => t.UsuarioCreadorId == userId
                         && (t.EstadoString == EnumEstadoTicket.Cerrado.ToString()
                           || t.EstadoString == EnumEstadoTicket.Cerrado_por_Usuario.ToString()));

            return View();
        }


        protected override void Dispose(bool disposing)
        {
            if (disposing) db.Dispose();
            base.Dispose(disposing);
        }

        public ActionResult About()
        {
            ViewBag.Message = "Your application description page.";

            return View();
        }

        public ActionResult Contact()
        {
            ViewBag.Message = "Your contact page.";

            return View();
        }
    }
}