using System;
using System.Data.Entity;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using System.Web.Security;
using Microsoft.Ajax.Utilities;
using Microsoft.AspNet.Identity;
using ServiceCenter.Models;
using ServiceCenter.Services;
using ServiceCenter.ViewModels;
using Microsoft.AspNet.Identity.Owin;
using Microsoft.AspNet.Identity.EntityFramework;
using System.Collections.Generic;


namespace ServiceCenter.Controllers
{
    [Authorize]
    public class TicketsController : Controller
    {
        private ApplicationDbContext db = new ApplicationDbContext();
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly ApplicationDbContext _context;

        public TicketsController()
        {
            var context = new ApplicationDbContext();
            var store = new UserStore<ApplicationUser>(context);
            _userManager = new UserManager<ApplicationUser>(store);
            _context = new ApplicationDbContext();
        }
   
        // GET: Tickets
        [Authorize]
        public ActionResult Index()
        {
            var usuarioId = User.Identity.GetUserId();

            IQueryable<Ticket> query = db.Tickets
                .Include(t => t.Departamento)
                .Include(t => t.UsuarioCreador);

            // Si el usuario es técnico, ve todos; si no, solo los propios
            if (!User.IsInRole("Tecnico") && !User.IsInRole("Admin"))
            {
                query = query.Where(t => t.UsuarioCreadorId == usuarioId);
            }

            var model = query
                .Select(t => new TicketListViewModel
                {
                    Id = t.Id,
                    NumTicket = t.NumTicket,
                    Titulo = t.Titulo,
                    Estado = t.EstadoString,
                    Prioridad = t.PrioridadString,
                    FechaCreacion = t.FechaCreacion,
                    DepartamentoNombre = t.Departamento.Nombre,
                    UsuarioCreadorNombre = t.UsuarioCreador != null && t.UsuarioCreador.UserName != null && t.UsuarioCreador.UserName != ""
                    ? t.UsuarioCreador.UserName
                    : t.UsuarioCreadorId



                })
                .ToList();

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
                ModelState.Remove("NumTicket");
                db.Tickets.Add(ticket);
                db.SaveChanges();

                // Si hay archivo, guardar el adjunto
                if (archivoAdjunto != null && archivoAdjunto.ContentLength > 0)
                {
                    var fileName = "Adjunto_Ticket_" + ticket.NumTicket + "_" + DateTime.Now.ToString("ddMMyyyy");
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

        [Authorize(Roles = "Tecnico,Admin")]
        public async Task<ActionResult> DetailsAdmin(int id)
        {
            var ticket = db.Tickets
                .Include(t => t.Departamento)
                .Include(t => t.UsuarioCreador)
                .FirstOrDefault(t => t.Id == id);

            if (ticket == null)
                return HttpNotFound();

            if (ticket.Estado == EnumEstadoTicket.Cerrado || ticket.Estado == EnumEstadoTicket.Cerrado_por_Usuario)
                return RedirectToAction("Details", new { id });

            var adjuntos = db.Adjuntos.Where(a => a.TicketId == id).ToList();
            var comentarios = db.Comentarios
                .Where(c => c.TicketId == id)
                .OrderByDescending(c => c.Fecha)
                .Select(c => new ComentarioViewModel
                {
                    UsuarioNombre = c.Usuario.UserName,
                    Fecha = c.Fecha,
                    Texto = c.ComentarioTexto
                })
                .ToList();

            // Cargar técnicos desde Identity
            var todosUsuarios = await _userManager.Users.ToListAsync();
            var tecnicos = new List<ApplicationUser>();
      


            foreach (var user in todosUsuarios)
            {
                if (await _userManager.IsInRoleAsync(user.Id, "Tecnico"))
                {
                    tecnicos.Add(user);
                }
            }
            var tecnicoAsignado = tecnicos.FirstOrDefault(t => t.Id == ticket.TecnicoAsignadoId);

            // Crear el ViewModel
            var vm = new TicketEditViewModel
            {

                Id = ticket.Id,
                NumTicket = ticket.NumTicket,
                Titulo = ticket.Titulo,
                Descripcion = ticket.Descripcion,
                Estado = ticket.EstadoString,
                Prioridad = ticket.PrioridadString,
                Categoria = ticket.CategoriaString,
                DepartamentoNombre = ticket.Departamento?.Nombre,
                UsuarioCreadorUserName = ticket.UsuarioCreador?.UserName ?? ticket.UsuarioCreadorId,
                FechaCreacion = ticket.FechaCreacion,
                TecnicoAsignadoNombre = tecnicoAsignado?.UserName ?? "Sin asignar",
                Adjuntos = adjuntos,
                Comentarios = comentarios,

                // Dropdowns
                Estados = Enum.GetValues(typeof(EnumEstadoTicket))
                    .Cast<EnumEstadoTicket>()
                    .Where(e => e != EnumEstadoTicket.Cerrado_por_Usuario)
                    .Select(e => new SelectListItem
                    {
                        Text = e.ToString(),
                        Value = e.ToString(),
                        Selected = e.ToString() == ticket.EstadoString
                    }),

                Prioridades = Enum.GetValues(typeof(EnumPrioridadTicket))
                    .Cast<EnumPrioridadTicket>()
                    .Select(p => new SelectListItem
                    {
                        Text = p.ToString(),
                        Value = p.ToString(),
                        Selected = p.ToString() == ticket.PrioridadString
                    }),

                Departamentos = db.Departamentos
                    .Select(d => new SelectListItem
                    {
                        Text = d.Nombre,
                        Value = d.Id.ToString(),
                        Selected = d.Id == ticket.DepartamentoId
                    }),

                // Técnicos
                Tecnicos = tecnicos.Select(t => new SelectListItem
                {
                    Text = t.UserName,
                    Value = t.Id,
                    Selected = t.Id == ticket.TecnicoAsignadoId
                })
            };

            return View("DetailsAdmin", vm);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Calificar(int ticketId, string calificacion, string comentario)
        {
            try
            {
                var userId = User.Identity.GetUserId();
                var usuario = await _context.Users.SingleOrDefaultAsync(u => u.Id == userId);
                var userName = usuario?.UserName ?? User.Identity.Name;
                var userEmail = usuario?.Email ?? "";

                var ticket = await _context.Tickets.FindAsync(ticketId);
                if (ticket == null)
                    return Json(new { success = false, error = "Ticket no encontrado" });

                // Obtener técnico asignado
                var tecnico = await _context.Users.SingleOrDefaultAsync(u => u.Id == ticket.TecnicoAsignadoId);
                if (tecnico == null || string.IsNullOrWhiteSpace(tecnico.Email))
                    return Json(new { success = false, error = "No se encontró correo del técnico asignado." });

                // Marcar como calificado en la base de datos
                ticket.Calificado = true;

                await _context.SaveChangesAsync();

                var asunto = $"📝 Calificación de Ticket #{ticket.NumTicket} – {calificacion}";

                var cuerpo = $@"
<html>
  <body style=""font-family:Arial,sans-serif;color:#333;margin:0;padding:20px;"">
    <h2 style=""color:#28a745;border-bottom:2px solid #28a745;padding-bottom:5px;"">
      ⭐ Calificación de Atención
    </h2>
    <p><strong>Ticket:</strong> #{ticket.NumTicket}</p>
    <p><strong>Título:</strong> {ticket.Titulo}</p>
    <p>El usuario <strong>{userName}</strong> ha calificado tu atención con la siguiente información:</p>

    <p><strong>Calificación:</strong> {calificacion}</p>
    <p><strong>Comentario:</strong></p>
    <div style=""background:#f8f9fa;padding:15px;border-radius:5px;"">
        {comentario}
    </div>
    <p><small style=""color:#666;"">
      Fecha: {DateTime.Now:dd/MM/yyyy HH:mm}
    </small></p>
  </body>
</html>";

                var emailSender = new ServiceCenter.Services.EmailSender(_context);
                await emailSender.SendEmailAsync(
                    notificar: false,
                    subject: asunto,
                    body: cuerpo,
                    replyToEmail: tecnico.Email
                );

                return Json(new { success = true });
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine("ERROR EN Calificar: " + ex);
                return Json(new { success = false, error = ex.Message });
            }
        }


        // GET: Tickets/Details/5
        public async Task<ActionResult> Details(int id)
        {
            var ticket = db.Tickets
                .Include(t => t.Departamento)
                .Include(t => t.UsuarioCreador)
                .FirstOrDefault(t => t.Id == id);

            if (ticket == null)
            {
                return HttpNotFound();
            }

            if ((User.IsInRole("Tecnico") || User.IsInRole("Admin"))
     && ticket.Estado != EnumEstadoTicket.Cerrado
     && ticket.Estado != EnumEstadoTicket.Cerrado_por_Usuario)
            {
                return RedirectToAction("DetailsAdmin", new { id });
            }

            var todosUsuarios = await _userManager.Users.ToListAsync();
            var tecnicos = new List<ApplicationUser>();



            foreach (var user in todosUsuarios)
            {
                if (await _userManager.IsInRoleAsync(user.Id, "Tecnico"))
                {
                    tecnicos.Add(user);
                }
            }
            var tecnicoAsignado = tecnicos.FirstOrDefault(t => t.Id == ticket.TecnicoAsignadoId);

            var adjuntos = db.Adjuntos
                .Where(a => a.TicketId == id)
                .ToList();

            var viewModel = new TicketDetalleViewModel
            {
                Id = ticket.Id,
                NumTicket = ticket.NumTicket,
                Titulo = ticket.Titulo,
                Calificado = ticket.Calificado,
                Descripcion = ticket.Descripcion,
                Estado = ticket.Estado.ToString(),
                Prioridad = ticket.Prioridad.ToString(),
                Categoria = ticket.Categoria.ToString(),
                TecnicoAsignadoNombre = tecnicoAsignado?.UserName ?? "Sin asignar",
                DepartamentoNombre = ticket.Departamento?.Nombre,
                UsuarioCreadorUserName = ticket.UsuarioCreador?.UserName,
                FechaCreacion = ticket.FechaCreacion,
                Adjuntos = adjuntos
            };

            var comentarios = db.Comentarios
             .Where(c => c.TicketId == id)
             .Include(c => c.Usuario)
             .OrderBy(c => c.Fecha)
             .Select(c => new ComentarioViewModel
             {
                 UsuarioNombre = c.Usuario.UserName,
                 Fecha = c.Fecha,
                 Texto = c.ComentarioTexto
             }).ToList();
            viewModel.Comentarios = comentarios;

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

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> AddComment(int ticketId, string textoComentario)
        {
            var ticket = db.Tickets.Find(ticketId);
            if (ticket == null) return HttpNotFound();

            // 1) Si está cerrado, redirigimos sin permitir comentar
            if (ticket.Estado == EnumEstadoTicket.Cerrado
             || ticket.Estado == EnumEstadoTicket.Cerrado_por_Usuario)
            {
                // opcional: TempData para mostrar alerta si quieres
                TempData["ErrorComentario"] = "No puede agregar comentarios a un ticket cerrado.";
                return RedirectToAction("Details", new { id = ticketId });
            }

            // 2) Validación de texto vacío
            if (string.IsNullOrWhiteSpace(textoComentario))
            {
                TempData["ErrorComentario"] = "El comentario no puede estar vacío.";
                return RedirectToAction("Details", new { id = ticketId });
            }

            // 3) Guardar comentario
            var comentario = new Comentario
            {
                TicketId = ticketId,
                UsuarioId = User.Identity.GetUserId(),
                ComentarioTexto = textoComentario,
                Fecha = DateTime.Now
            };
            db.Comentarios.Add(comentario);
            db.SaveChanges();

            // 4) Enviar notificación por correo
            try
            {
                var userId = User.Identity.GetUserId();
                var user = await db.Users.SingleOrDefaultAsync(u => u.Id == userId);
                var userName = user?.Nombre ?? User.Identity.Name;
                var userEmail = ticket.UsuarioCreador?.Email;
                Debug.WriteLine($"Enviando notificación a {userEmail} por nuevo comentario en Ticket #{ticket.NumTicket}");
                // Instancia el servicio con el mismo DbContext
                var emailSender = new ServiceCenter.Services.EmailSender(db);

                // Construye asunto y body

                var subject = $"[ServiceCenter] Nuevo comentario en Ticket #{ticket.NumTicket}";
                var body = $@"
<html>
  <body style=""font-family:Arial,sans-serif;color:#333;margin:0;padding:20px;"">
    <h2 style=""color:#0066cc;border-bottom:2px solid #0066cc;padding-bottom:5px;"">
      ✉️ Nuevo Comentario en Ticket #{ticket.NumTicket}
    </h2>
    <p><strong>Ticket:</strong> {ticket.Titulo}</p>
    <p><strong>Usuario:</strong> <span style=""color:#0066cc;"">{userName}</span> ha agregado un comentario:</p>

    <div style=""background:#f5f5f5;padding:15px;border-left:4px solid #0066cc;border-radius:4px;margin:20px 0;"">
      {textoComentario}
    </div>

    <p style=""margin-top:30px;"">
      <a href=""{Url.Action("Details", "Tickets", new { id = ticketId }, Request.Url.Scheme)}""
         style=""display:inline-block;padding:10px 20px;background:#0066cc;color:#fff;
                text-decoration:none;border-radius:4px;"">
        Ver ticket completo
      </a>
    </p>

    <p style=""font-size:0.8em;color:#666;margin-top:40px;"">
      Este es un mensaje automático de Pizza Hut. No respondas a este correo.
    </p>
  </body>
</html>
";



                await emailSender.SendEmailAsync(notificar: false, subject, body, replyToEmail: userEmail);
            }
            catch (Exception ex)
            {
                // Opcional: log, pero no interrumpimos el flujo de usuario
                System.Diagnostics.Debug.WriteLine("Error notificando por email: " + ex);
            }

            return RedirectToAction("Details", new { id = ticketId });
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Close(int id)
        {
            var ticket = db.Tickets.Find(id);
            if (ticket == null) return HttpNotFound();

            ticket.Estado = EnumEstadoTicket.Cerrado_por_Usuario;
            db.SaveChanges();

            return RedirectToAction("Details", new { id });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult CloseWithReason(int ticketId, string motivoCierre)
        {
            var ticket = db.Tickets.Find(ticketId);
            if (ticket == null) return HttpNotFound();

            ticket.Estado = EnumEstadoTicket.Cerrado_por_Usuario;

            var comentario = new Comentario
            {
                TicketId = ticketId,
                UsuarioId = User.Identity.GetUserId(),
                ComentarioTexto = "[Cierre por el usuario] " + motivoCierre,
                Fecha = DateTime.Now
            };
            db.Comentarios.Add(comentario);

            db.SaveChanges();
            return RedirectToAction("Details", new { id = ticketId });
        }

        // POST: Tickets/Atender
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Tecnico")]
        public async Task<ActionResult> Atender(TicketEditViewModel vm)
        {
            if (!ModelState.IsValid)
            {
                // Si falla validación, volver a cargar los dropdowns
                vm.Estados = Enum.GetValues(typeof(EnumEstadoTicket))
                                .Cast<EnumEstadoTicket>()
                                .Select(e => new SelectListItem
                                {
                                    Text = e.ToString(),
                                    Value = e.ToString(),
                                    Selected = e.ToString() == vm.Estado
                                });
                vm.Prioridades = Enum.GetValues(typeof(EnumPrioridadTicket))
                                     .Cast<EnumPrioridadTicket>()
                                     .Select(p => new SelectListItem
                                     {
                                         Text = p.ToString(),
                                         Value = p.ToString(),
                                         Selected = p.ToString() == vm.Prioridad
                                     });
                vm.Departamentos = db.Departamentos
                                     .Select(d => new SelectListItem
                                     {
                                         Text = d.Nombre,
                                         Value = d.Id.ToString(),
                                         Selected = d.Nombre == vm.DepartamentoNombre
                                     });   
                
                return View(vm);
            }
   


            // 1) Obtener entidad
            var ticket = db.Tickets.Find(vm.Id);
            if (ticket == null) return HttpNotFound();
         


            // 2) Guardar valores viejos para la notificación
            var oldEstado = ticket.EstadoString;
            var oldPrioridad = ticket.PrioridadString;
            var oldDeptId = ticket.DepartamentoId;

            // 2) Aplicar cambios
            ticket.EstadoString = vm.Estado;
            ticket.PrioridadString = vm.Prioridad;
            // Como vm.DepartamentoNombre lleva el ID en string, parseamos:
            if (int.TryParse(vm.DepartamentoNombre, out int depId))
                ticket.DepartamentoId = depId;

            ticket.TecnicoAsignadoId = vm.TecnicoAsignadoId;
       

            db.SaveChanges();

            // 4) Preparar y enviar notificación
            try
            {

                // Datos para el correo
                var techUserId = User.Identity.GetUserId();
                var techUser = await db.Users.SingleOrDefaultAsync(u => u.Id == techUserId);
                var techName = techUser?.UserName ?? User.Identity.Name;
                var creator = ticket.UsuarioCreador?.UserName;
                var creatorEmail = ticket.UsuarioCreador?.Email;
                Debug.WriteLine($"Enviando notificación a {creatorEmail} del tecnico {techName} por actualización de Ticket #{ticket.NumTicket}");

                // Construir asunto
                var subject = $"[ServiceCenter] Ticket #{ticket.NumTicket} actualizado";

                // Construir body con HTML embebido
                var body = $@"
<html>
  <body style=""font-family:Arial,sans-serif;color:#333;margin:0;padding:20px;"">
    <h2 style=""color:#0066cc;border-bottom:2px solid #0066cc;padding-bottom:5px;"">
      🔧 Ticket #{ticket.NumTicket} actualizado
    </h2>
    <p>El técnico <strong>{techName}</strong> ha realizado los siguientes cambios:</p>
    <ul>
      {(oldEstado != ticket.EstadoString ? $"<li><strong>Estado:</strong> {oldEstado} → {ticket.EstadoString}</li>" : "")}
      {(oldPrioridad != ticket.PrioridadString ? $"<li><strong>Prioridad:</strong> {oldPrioridad} → {ticket.PrioridadString}</li>" : "")}
      {(oldDeptId != ticket.DepartamentoId ? $"<li><strong>Departamento:</strong> ID {oldDeptId} → ID {ticket.DepartamentoId}</li>" : "")}
    </ul>
    <p style=""margin-top:20px;"">
      <a href=""{Url.Action("Details", "Tickets", new { id = ticket.Id }, Request.Url.Scheme)}""
         style=""display:inline-block;padding:10px 20px;background:#0066cc;color:#fff;
                text-decoration:none;border-radius:4px;"">
        Ver ticket completo
      </a>
    </p>
    <p style=""font-size:0.8em;color:#666;margin-top:40px;"">
      Este es un mensaje automático de Pizza Hut.
    </p>
  </body>
</html>
";

                var emailSender = new ServiceCenter.Services.EmailSender(db);
                // notificar=true para que llegue a todos los destinatarios activos y al creador
                await emailSender.SendEmailAsync(
                    notificar: false,
                    subject: subject,
                    body: body,
                    replyToEmail: creatorEmail
                );
            }
            catch (Exception ex)
            {
                // log sin interrumpir el flujo
                System.Diagnostics.Debug.WriteLine("Error notificando actualización de ticket: " + ex);
            }

            // 3) Volver al detalle del admin
            return RedirectToAction("DetailsAdmin", new { id = ticket.Id });
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
