using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.EntityFramework;
using PH_ServiceCenter.Models;
using System;
using System.ComponentModel.DataAnnotations;
using System.Data.Entity;
using System.Security.Claims;
using System.Threading.Tasks;

namespace ServiceCenter.Models
{
    // You can add profile data for the user by adding more properties to your ApplicationUser class, please visit https://go.microsoft.com/fwlink/?LinkID=317594 to learn more.
    public class ApplicationUser : IdentityUser
    {
        [StringLength(100)]
        public string Nombre { get; set; }
        public string PhotoPath { get; set; }
        public DateTime FechaCreacion { get; set; } = DateTime.UtcNow;
        public async Task<ClaimsIdentity> GenerateUserIdentityAsync(UserManager<ApplicationUser> manager)
        {
            // Este es el default:
            var userIdentity = await manager
                .CreateIdentityAsync(this, DefaultAuthenticationTypes.ApplicationCookie);


            // --- Añade aquí cada rol como ClaimTypes.Role ---
            var roles = await manager.GetRolesAsync(this.Id);
            foreach (var rol in roles)
            {
                userIdentity.AddClaim(new Claim(ClaimTypes.Role, rol));
            }
            userIdentity.AddClaim(new Claim("FullName", this.Nombre ?? ""));

            return userIdentity;
        }

    }


}