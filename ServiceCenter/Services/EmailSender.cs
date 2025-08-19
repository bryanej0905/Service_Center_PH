using System;
using System.Linq;
using System.Net;
using System.Net.Mail;
using System.Threading.Tasks;
using SendGrid;
using SendGrid.Helpers.Mail;
using ServiceCenter.Models;

namespace ServiceCenter.Services
{
    /// <summary>
    /// Servicio para enviar correos usando SendGrid o SMTP, configurado desde la BD.
    /// </summary>
    public class EmailSender
    {
        private readonly ApplicationDbContext _context;

        public EmailSender(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task SendEmailAsync(bool notificar, string subject, string body, string replyToEmail = null)
        {
            // 1) Leer configuración
            var cfg = _context.EmailServerConfig
                .OrderByDescending(c => c.FechaCreacion)
                .FirstOrDefault();
            if (cfg == null)
                throw new InvalidOperationException("Falta configuración de correo en BD.");

            // 2) Construir lista de destinatarios
            var tos = new System.Collections.Generic.List<EmailAddress>();
            if (notificar)
            {
                var raw = _context.EmailDestinatarios
                    .Where(d => d.Activo)
                    .Select(d => new { d.Correo, d.Nombre })
                    .ToList();
                tos.AddRange(raw.Select(x => new EmailAddress(x.Correo, x.Nombre)));
            }
            // Agregar replyToEmail si existe y no está en la lista
            if (!string.IsNullOrWhiteSpace(replyToEmail) &&
                !tos.Any(t => t.Email.Equals(replyToEmail, StringComparison.OrdinalIgnoreCase)))
            {
                tos.Add(new EmailAddress(replyToEmail, replyToEmail));
            }

            // 3) Enviar según proveedor
            if (cfg.Provider == "SendGrid")
            {
                var client = new SendGridClient(cfg.ApiKey);
                var from = new EmailAddress(cfg.FromEmail, cfg.FromName);
                var msg = MailHelper.CreateSingleEmailToMultipleRecipients(
                    from: from,
                    tos: tos,
                    subject: subject,
                    plainTextContent: null,
                    htmlContent: body,
                    showAllRecipients: false);

                if (!string.IsNullOrWhiteSpace(replyToEmail))
                    msg.ReplyTo = new EmailAddress(replyToEmail);

                var resp = await client.SendEmailAsync(msg);
                if (resp.StatusCode != System.Net.HttpStatusCode.Accepted)
                    throw new Exception($"SendGrid error: {resp.StatusCode}");
            }
            else
            {
                using (var smtp = new SmtpClient())
                {
                    smtp.UseDefaultCredentials = false;
                    smtp.Host = cfg.Host;
                    smtp.Port = cfg.Puerto;
                    smtp.EnableSsl = cfg.HabilitarSSL;
                    smtp.Credentials = new NetworkCredential(cfg.Usuario, cfg.Contrasena);

                    using (var mail = new MailMessage())
                    {
                        mail.From = new MailAddress(cfg.FromEmail ?? cfg.Usuario, cfg.FromName);
                        foreach (var to in tos)
                            mail.To.Add(new MailAddress(to.Email, to.Name));

                        if (!string.IsNullOrWhiteSpace(replyToEmail))
                            mail.ReplyToList.Add(new MailAddress(replyToEmail));

                        mail.Subject = subject;
                        mail.Body = body;
                        mail.IsBodyHtml = true;

                        await smtp.SendMailAsync(mail);
                    }
                }
            }
        }
    }
}
