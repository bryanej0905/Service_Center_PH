using Microsoft.AspNet.Identity.EntityFramework;
using PH_ServiceCenter.Models;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Web;

namespace ServiceCenter.Models
{
    public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
    {
        public ApplicationDbContext()
            : base("DefaultConnection", throwIfV1Schema: false)
        {
        }
        public static ApplicationDbContext Create()
        {
            return new ApplicationDbContext();
        }
        public DbSet<CategoriaFAQ> CategoriasFAQ { get; set; }
        public DbSet<PreguntaFAQ> PreguntasFAQ { get; set; }
        public DbSet<ReporteErrorGuia> ReportesError { get; set; }
        public DbSet<Guia> Guias { get; set; }
        public DbSet<Categoria> Categorias { get; set; }
        public DbSet<Ticket> Tickets { get; set; }
        public DbSet<Departamento> Departamentos { get; set; }
        public DbSet<Comentario> Comentarios { get; set; }
        public DbSet<Evidencia> Evidencias { get; set; }
        public DbSet<Adjunto> Adjuntos { get; set; }
        public DbSet<Biblioteca_Items_New> Biblioteca_Items_New { get; set; }
        public DbSet<HistorialAsignacion> HistorialAsignaciones { get; set; }
        public DbSet<SugerenciaPreguntasFAQ> SugerenciasPreguntasFAQ { get; set; }
        public DbSet<GuiaInteraccion> GuiaInteracciones { get; set; }
        public DbSet<EmailDestinatario> EmailDestinatarios { get; set; }
        public DbSet<EmailServerConfig> EmailServerConfig { get; set; }
        public DbSet<ConsultaRecienteFAQ> ConsultasRecientesFAQ { get; set; }
        public DbSet<ManageIPChatBot> ManageIPChatBot { get; set; }
        public DbSet<InteraccionFAQ> InteraccionesFAQ { get; set; }
        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            

        }
    }
}