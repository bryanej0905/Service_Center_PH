using System.Data.Entity;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.EntityFramework;

namespace ServiceCenter.Models
{
    // You can add profile data for the user by adding more properties to your ApplicationUser class, please visit https://go.microsoft.com/fwlink/?LinkID=317594 to learn more.
    public class ApplicationUser : IdentityUser
    {
        public async Task<ClaimsIdentity> GenerateUserIdentityAsync(UserManager<ApplicationUser> manager)
        {
            // Note the authenticationType must match the one defined in CookieAuthenticationOptions.AuthenticationType
            var userIdentity = await manager.CreateIdentityAsync(this, DefaultAuthenticationTypes.ApplicationCookie);
            // Add custom user claims here
            return userIdentity;
        }
    }

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
        public DbSet<Guia> Guias { get; set; }
        public DbSet<Categoria> Categorias { get; set; }
        public DbSet<Ticket> Tickets { get; set; }
        public DbSet<Departamento> Departamentos { get; set; }
        public DbSet<Comentario> Comentarios { get; set; }
        public DbSet<Evidencia> Evidencias { get; set; }
        public DbSet<Adjunto> Adjuntos { get; set; }
        public DbSet<HistorialAsignacion> HistorialAsignaciones { get; set; }
        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

        }
    }
}