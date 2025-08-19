using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using Microsoft.AspNet.Identity;
using ServiceCenter.Models;

namespace ServiceCenter.Controllers
{
    public class GuiasController : Controller
    {
        private ApplicationDbContext db = new ApplicationDbContext();
        private readonly ApplicationDbContext _context;

        public GuiasController()
        {
            _context = new ApplicationDbContext();
        }

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
            if (disposing)
            {
                db.Dispose();
            }
            base.Dispose(disposing);
        }
    }
}
