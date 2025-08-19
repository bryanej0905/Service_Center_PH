using System.ComponentModel.DataAnnotations;

namespace APIServiceCenterPH.Models
{
    public class CreateTicketDto
    {
        [Required, StringLength(150)]
        public string Nombre { get; set; } = default!;

        [Required, StringLength(150)]
        public string Titulo { get; set; } = default!;

        [Required]
        public string Descripcion { get; set; } = default!;

        [Required, StringLength(30)]
        public string Categoria { get; set; } = default!;
    }

}
