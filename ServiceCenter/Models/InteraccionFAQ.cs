using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ServiceCenter.Models
{
    public class InteraccionFAQ
    {
        public int Id { get; set; }

        [Required]
        public int PreguntaId { get; set; }

        [Required]
        [MaxLength(128)]
        [ForeignKey("Usuario")]
        public string UserId { get; set; }

        public bool EsUtil { get; set; }

        public DateTime Fecha { get; set; }

        public virtual PreguntaFAQ Pregunta { get; set; }
        public virtual ApplicationUser Usuario { get; set; }
    }
}
