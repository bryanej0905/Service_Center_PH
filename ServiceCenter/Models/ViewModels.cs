using ServiceCenter.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace ServiceCenter.ViewModels
{
    public class TicketListViewModel
    {
        public int Id { get; set; }
        public string Titulo { get; set; }
        public string Estado { get; set; }
        public string Prioridad { get; set; }
        public string Categoria { get; set; }
        public string DepartamentoNombre { get; set; }
        public string UsuarioCreadorNombre { get; set; }
        public List<Adjunto> Adjuntos { get; set; }

    }

    public class TicketDetalleViewModel
    {
        public int Id { get; set; }
        public string Titulo { get; set; }
        public string Descripcion { get; set; }
        public string Estado { get; set; }
        public string Prioridad { get; set; }
        public string Categoria { get; set; }
        public string DepartamentoNombre { get; set; }
        public string UsuarioCreadorUserName { get; set; }
        public DateTime FechaCreacion { get; set; }
        public List<Adjunto> Adjuntos { get; set; }
    }

}
