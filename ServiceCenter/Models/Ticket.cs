using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace ServiceCenter.Models
{
    using System;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;

    public class Ticket
    {
        public int Id { get; set; }

        [DatabaseGenerated(DatabaseGeneratedOption.Computed)]
        public string NumTicket { get; set; }

        [Required, StringLength(150)]
        public string Titulo { get; set; }

        public string Descripcion { get; set; }

        [Required, Column("Estado"), StringLength(20)]
        public string EstadoString { get; set; }

        [NotMapped]
        public EnumEstadoTicket Estado
        {
            get => Enum.TryParse(EstadoString, out EnumEstadoTicket result) ? result : EnumEstadoTicket.Abierto;
            set => EstadoString = value.ToString();
        }

        [Required, Column("Prioridad"), StringLength(20)]
        public string PrioridadString { get; set; }

        [NotMapped]
        public EnumPrioridadTicket Prioridad
        {
            get => Enum.TryParse(PrioridadString, out EnumPrioridadTicket result) ? result : EnumPrioridadTicket.Media;
            set => PrioridadString = value.ToString();
        }

        [Required, Column("Categoria"), StringLength(30)]
        public string CategoriaString { get; set; }

        [NotMapped]
        public EnumCategoriaTicket Categoria
        {
            get => Enum.TryParse(CategoriaString, out EnumCategoriaTicket result) ? result : EnumCategoriaTicket.Otro;
            set => CategoriaString = value.ToString();
        }

        public DateTime FechaCreacion { get; set; }

        public string UsuarioCreadorId { get; set; }

        public string TecnicoAsignadoId { get; set; }

        public int DepartamentoId { get; set; }

        public bool Calificado { get; set; }

        public string Solucion { get; set; }

        [ForeignKey("UsuarioCreadorId")]
        public virtual ApplicationUser UsuarioCreador { get; set; }

        [ForeignKey("TecnicoAsignadoId")]
        public virtual ApplicationUser TecnicoAsignado { get; set; }

        [ForeignKey("DepartamentoId")]
        public virtual Departamento Departamento { get; set; }

        public Ticket()
        {
            FechaCreacion = DateTime.Now;
            Estado = EnumEstadoTicket.Abierto;
            Prioridad = EnumPrioridadTicket.Media;
            Categoria = EnumCategoriaTicket.Otro;
        }

    }


}