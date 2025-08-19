using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace ServiceCenter.Models
{
    public class EmailServerConfig
    {
        [Key]
        public int Id { get; set; }

        [Required, StringLength(256)]
        public string Host { get; set; }

        [Required]
        public int Puerto { get; set; }

        public bool HabilitarSSL { get; set; } = true;

        [Required, StringLength(256)]
        public string Usuario { get; set; }

        [Required, StringLength(512)]
        public string Contrasena { get; set; }  // almacena cifrada

        [StringLength(100)]
        public string FromName { get; set; }

        [StringLength(256)]
        public string FromEmail { get; set; }

        public DateTime FechaCreacion { get; set; } = DateTime.UtcNow;
        public string Provider { get; set; }
        public string ApiKey { get; set; }
    }
}