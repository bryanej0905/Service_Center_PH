using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace ServiceCenter.Models
{

    public class Evidencia
    {
        public int Id { get; set; }

        public int TicketId { get; set; }

        public string UrlArchivo { get; set; }

        public DateTime FechaSubida { get; set; }

        public virtual Ticket Ticket { get; set; }
    }
}