using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;

namespace ServiceCenter.Models
{
    public class HistorialAsignacion
    {
        public int Id { get; set; }

        public int TicketId { get; set; }

        public string AsignadoPorId { get; set; }

        public string AsignadoAId { get; set; }

        public DateTime FechaAsignacion { get; set; }

        public virtual Ticket Ticket { get; set; }

        [ForeignKey("AsignadoPorId")]
        public virtual ApplicationUser AsignadoPor { get; set; }

        [ForeignKey("AsignadoAId")]
        public virtual ApplicationUser AsignadoA { get; set; }
    }
}