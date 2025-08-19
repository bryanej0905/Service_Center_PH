using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;

namespace ServiceCenter.Models
{
    public class ManageIPChatBot
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [MaxLength(100)]
        public string Ip { get; set; }

        [Required]
        public int Puerto { get; set; }

        [Required]
        [MaxLength(100)]
        public string CreadoPor { get; set; }

        [Required]
        public DateTime FechaCreacion { get; set; }

        [NotMapped]
        public string UrlBase => $"http://{Ip}:{Puerto}";
    }
}