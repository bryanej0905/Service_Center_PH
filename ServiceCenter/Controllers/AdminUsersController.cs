using System;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using System.Web.Mvc;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.EntityFramework;
using ServiceCenter.Models;
using ServiceCenter.ViewModels;

namespace ServiceCenter.Controllers
{
    [Authorize(Roles = "Admin")]
    public class AdminUsersController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public AdminUsersController()
        {
            _context = new ApplicationDbContext();
            var store = new UserStore<ApplicationUser>(_context);
            _userManager = new UserManager<ApplicationUser>(store);
        }

        // GET: AdminUsers
        public ActionResult Index()
        {
            var users = _userManager.Users
                .ToList()
                .Select(u => new UserListViewModel
                {
                    Id = u.Id,
                    Nombre = u.Nombre,
                    UserName = u.UserName,
                    Email = u.Email,
                    Roles = string.Join(", ", _userManager.GetRoles(u.Id))
                });
            return View(users);
        }

        // GET: AdminUsers/Details/5
        public async Task<ActionResult> Details(string id)
        {
            if (id == null) return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            var user = await _userManager.FindByIdAsync(id);
            if (user == null) return HttpNotFound();

            var model = new ViewModels.UserEditViewModel
            {
                Id = user.Id,
                UserName = user.UserName,
                Email = user.Email,
                Nombre = user.Nombre,              // tu nuevo campo
                LockoutEnabled = user.LockoutEnabled,
                LockoutEndDateUtc = user.LockoutEndDateUtc,
                SelectedRoles = _userManager.GetRoles(user.Id)
            };
            model.AllRoles = _context.Roles
                .Select(r => new SelectListItem
                {
                    Value = r.Name,
                    Text = r.Name,
                    Selected = model.SelectedRoles.Contains(r.Name)
                })
                .ToList();

            return View("Details", model);
        }

        // GET: AdminUsers/Create
        public ActionResult Create()
        {
            ViewBag.AllRoles = _context.Roles
                .Select(r => new SelectListItem
                {
                    Value = r.Name,
                    Text = r.Name
                })
                .ToList();

            return View("Create", new UserCreateViewModel());
        }

        // AdminUsersController.cs

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Create(UserCreateViewModel model)
        {
            // 1) Validaciones de modelo
            if (!ModelState.IsValid)
            {
                ViewBag.AllRoles = _context.Roles
                    .Select(r => new SelectListItem { Value = r.Name, Text = r.Name })
                    .ToList();
                return View(model);
            }

            // 2) Comprueba si ya existe un usuario con ese email
            var emailExists = await _userManager.FindByEmailAsync(model.Email);
            if (emailExists != null)
            {
                ModelState.AddModelError(
                    nameof(model.Email),
                    "Ya existe un usuario registrado con ese correo."
                );
                ViewBag.AllRoles = _context.Roles
                    .Select(r => new SelectListItem { Value = r.Name, Text = r.Name })
                    .ToList();
                return View(model);
            }

            // 3) Creamos el usuario
            var user = new ApplicationUser
            {
                UserName = model.UserName,
                Email = model.Email,
                Nombre = model.Nombre,      // si añadiste el campo Nombre
                FechaCreacion = DateTime.UtcNow
            };

            var result = await _userManager.CreateAsync(user, model.Password);
            if (!result.Succeeded)
            {
                // Mostrar errores generados por Identity
                foreach (var err in result.Errors)
                    ModelState.AddModelError("", err);

                ViewBag.AllRoles = _context.Roles
                    .Select(r => new SelectListItem { Value = r.Name, Text = r.Name })
                    .ToList();
                return View(model);
            }

            // 4) Asignar roles y redirigir
            if (model.SelectedRoles?.Any() == true)
                await _userManager.AddToRolesAsync(user.Id, model.SelectedRoles);

            return RedirectToAction("Index");
        }


        // GET: AdminUsers/Edit/5
        public async Task<ActionResult> Edit(string id)
        {
            if (id == null) return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            var user = await _userManager.FindByIdAsync(id);
            if (user == null) return HttpNotFound();

            var model = new ViewModels.UserEditViewModel
            {
                Id = user.Id,
                UserName = user.UserName,
                Email = user.Email,
                Nombre = user.Nombre,
                LockoutEnabled = user.LockoutEnabled,
                LockoutEndDateUtc = user.LockoutEndDateUtc,
                SelectedRoles = _userManager.GetRoles(user.Id)
            };
            
            model.AllRoles = _context.Roles
                .Select(r => new SelectListItem
                {
                    Value = r.Name,
                    Text = r.Name,
                    Selected = model.SelectedRoles.Contains(r.Name)
                })
                .ToList();
            return View("Edit", model);

        }

        // POST: AdminUsers/Edit/5
        [HttpPost, ValidateAntiForgeryToken]
        public async Task<ActionResult> Edit(ViewModels.UserEditViewModel model)
        {
            // Recargar roles si hay error
            ActionResult ReturnWithRoles()
            {
                model.AllRoles = _context.Roles
                    .Select(r => new SelectListItem
                    {
                        Value = r.Name,
                        Text = r.Name,
                        Selected = model.SelectedRoles.Contains(r.Name)
                    })
                    .ToList();
                return View("Edit", model);
            }

            if (!ModelState.IsValid)
                return ReturnWithRoles();

            var user = await _userManager.FindByIdAsync(model.Id);
            if (user == null) return HttpNotFound();

            // 1) Perfil básico
            user.UserName = model.UserName;
            user.Email = model.Email;
            user.Nombre = model.Nombre;
            user.LockoutEnabled = model.LockoutEnabled;
            user.LockoutEndDateUtc = model.LockoutEnabled
                                      ? model.LockoutEndDateUtc
                                      : (DateTime?)null;

            var upd = await _userManager.UpdateAsync(user);
            if (!upd.Succeeded)
            {
                upd.Errors.ToList().ForEach(e => ModelState.AddModelError("", e));
                return ReturnWithRoles();
            }

            // 2) Cambio de contraseña (opcional)
            // Dentro de tu Edit(POST), en lugar de ResetPasswordAsync:

            if (!string.IsNullOrWhiteSpace(model.NewPassword))
            {
                // 1) Quitar la contraseña antigua
                var removeResult = await _userManager.RemovePasswordAsync(user.Id);
                if (!removeResult.Succeeded)
                {
                    removeResult.Errors.ToList()
                        .ForEach(e => ModelState.AddModelError("", e));
                    return ReturnWithRoles(); // tu método local para recargar AllRoles y devolver View
                }

                // 2) Añadir la nueva
                var addResult = await _userManager.AddPasswordAsync(user.Id, model.NewPassword);
                if (!addResult.Succeeded)
                {
                    addResult.Errors.ToList()
                        .ForEach(e => ModelState.AddModelError("", e));
                    return ReturnWithRoles();
                }
            }


            // 3) Roles
            var currentRoles = await _userManager.GetRolesAsync(user.Id);
            var toAdd = model.SelectedRoles.Except(currentRoles).ToArray();
            var toRemove = currentRoles.Except(model.SelectedRoles).ToArray();
            if (toAdd.Any()) await _userManager.AddToRolesAsync(user.Id, toAdd);
            if (toRemove.Any()) await _userManager.RemoveFromRolesAsync(user.Id, toRemove);

            return RedirectToAction("Index");
        }


        // GET: AdminUsers/Delete/5
        public async Task<ActionResult> Delete(string id)
        {
            if (id == null) return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            var user = await _userManager.FindByIdAsync(id);
            if (user == null) return HttpNotFound();

            var model = new UserListViewModel
            {
                Id = user.Id,
                UserName = user.UserName,
                Email = user.Email
            };
            return View("Delete", model);
        }

        // POST: AdminUsers/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> DeleteConfirmed(string id)
        {
            var user = await _userManager.FindByIdAsync(id);
            if (user != null)
                await _userManager.DeleteAsync(user);

            return RedirectToAction("Index");
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing) _context.Dispose();
            base.Dispose(disposing);
        }
    }
}
