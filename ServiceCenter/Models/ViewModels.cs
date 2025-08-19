using ServiceCenter.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace ServiceCenter.ViewModels
{
    public class TicketListViewModel
    {
        public int Id { get; set; }
        public string NumTicket { get; set; }
        public string Titulo { get; set; }
        public string Estado { get; set; }
        public string Prioridad { get; set; }
        public string Categoria { get; set; }
        public string DepartamentoNombre { get; set; }
        public string UsuarioCreadorNombre { get; set; }
        public DateTime FechaCreacion { get; set; }
        public List<Adjunto> Adjuntos { get; set; }

    }

    public class TicketDetalleViewModel
    {
        public int Id { get; set; }
        public string NumTicket { get; set; }
        public string Titulo { get; set; }
        public string Descripcion { get; set; }
        public string Estado { get; set; }
        public string Prioridad { get; set; }
        public string Categoria { get; set; }
        public string DepartamentoNombre { get; set; }
        public string UsuarioCreadorUserName { get; set; }
        public DateTime FechaCreacion { get; set; }
        public List<Adjunto> Adjuntos { get; set; }
        public List<ComentarioViewModel> Comentarios { get; set; }
        public string TecnicoAsignadoNombre { get; set; }
        public bool Calificado { get; internal set; }
    }

    public class TicketEditViewModel
    {
        public int Id { get; set; }
        public string NumTicket { get; set; }
        public string Titulo { get; set; }
        public string Descripcion { get; set; }
        public string Estado { get; set; }
        public string Prioridad { get; set; }
        public string Categoria { get; set; }
        public string DepartamentoNombre { get; set; }
        public string UsuarioCreadorUserName { get; set; }
        public DateTime FechaCreacion { get; set; }
        public List<Adjunto> Adjuntos { get; set; }
        public List<ComentarioViewModel> Comentarios { get; set; }
        public IEnumerable<SelectListItem> Tecnicos { get; set; }
        public string TecnicoAsignadoId { get; set; }
        public string TecnicoAsignadoNombre { get; set; }

        // Listas para los dropdowns
        public IEnumerable<SelectListItem> Estados { get; set; }
        public IEnumerable<SelectListItem> Prioridades { get; set; }
        public IEnumerable<SelectListItem> Departamentos { get; set; }
    }

    public class ComentarioViewModel
    {
        public string UsuarioNombre { get; set; }
        public DateTime Fecha { get; set; }
        public string Texto { get; set; }
    }
    public class UserListViewModel
    {
        public string Id { get; set; }
        [StringLength(100)]
        [Display(Name = "Nombre completo")]
        public string Nombre { get; set; }
        public string UserName { get; set; }
        public string Email { get; set; }
        public string Roles { get; set; }
    }
    public class UserCreateViewModel
    {
        [Required]
        [Display(Name = "Nombre completo")]
        public string Nombre { get; set; }
        [Required]
        public string UserName { get; set; }
        [Required, EmailAddress]
        public string Email { get; set; }
        [Required, DataType(DataType.Password)]
        public string Password { get; set; }
        [Display(Name = "Roles")]
        public string[] SelectedRoles { get; set; }
    }


    public class ChangePasswordViewModel
    {
        [Required]
        [DataType(DataType.Password)]
        [Display(Name = "Contraseña actual")]
        public string OldPassword { get; set; }

        [Required]
        [StringLength(100, MinimumLength = 6,
            ErrorMessage = "La {0} debe tener al menos {2} caracteres.")]
        [DataType(DataType.Password)]
        [Display(Name = "Nueva contraseña")]
        public string NewPassword { get; set; }

        [DataType(DataType.Password)]
        [Display(Name = "Confirmar nueva contraseña")]
        [System.ComponentModel.DataAnnotations.Compare("NewPassword", ErrorMessage = "Las contraseñas no coinciden.")]
        public string ConfirmPassword { get; set; }
    }

    public class UserEditViewModel
    {
        [Required]
        public string Id { get; set; }

        [Required]
        [Display(Name = "Usuario")]
        public string UserName { get; set; }

        [Required]
        [EmailAddress]
        [Display(Name = "Email")]
        public string Email { get; set; }

        [StringLength(100)]
        [Display(Name = "Nombre completo")]
        public string Nombre { get; set; }

        [Display(Name = "Bloquear inicio de sesión")]
        public bool LockoutEnabled { get; set; }

        [DataType(DataType.DateTime)]
        [Display(Name = "Fecha fin de bloqueo")]
        public DateTime? LockoutEndDateUtc { get; set; }

        [DataType(DataType.Password)]
        [Display(Name = "Nueva contraseña")]
        public string NewPassword { get; set; }

        [DataType(DataType.Password)]
        [Display(Name = "Confirmar contraseña")]
        [System.ComponentModel.DataAnnotations.Compare("NewPassword", ErrorMessage = "Las contraseñas no coinciden")]
        public string ConfirmPassword { get; set; }

        [Display(Name = "Roles")]
        public IList<string> SelectedRoles { get; set; }

        public IEnumerable<SelectListItem> AllRoles { get; set; }
    }
}
