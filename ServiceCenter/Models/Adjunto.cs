using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace ServiceCenter.Models
{
    public class Adjunto
    {
        public int Id { get; set; }

        public int TicketId { get; set; }

        public string NombreArchivo { get; set; }

        public string RutaArchivo { get; set; }

        public DateTime FechaSubida { get; set; }

        public string SubidoPorId { get; set; }

        public virtual Ticket Ticket { get; set; }

        public virtual ApplicationUser SubidoPor { get; set; }
    }
}