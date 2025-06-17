using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;

namespace ServiceCenter.Models
{
    public class Comentario
    {
        public int Id { get; set; }

        public int TicketId { get; set; }

        public string UsuarioId { get; set; }

        public string ComentarioTexto { get; set; }

        public DateTime Fecha { get; set; }

        public virtual Ticket Ticket { get; set; }

        [ForeignKey("UsuarioId")]
        public virtual ApplicationUser Usuario { get; set; }
    }
}