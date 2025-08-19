using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Net.Sockets;
using System.Web;

namespace ServiceCenter.Models
{
    public class Departamento
    {
        public int Id { get; set; }

        [Required, StringLength(100)]
        public string Nombre { get; set; }

        public virtual ICollection<Ticket> Tickets { get; set; }
    }
}