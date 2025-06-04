using System.Data.Entity;          
using System.Linq;
using System.Threading.Tasks;
using System.Web.Mvc;            
using ServiceCenter.Models;

namespace ServiceCenter.Controllers
{
    [Authorize]
    public class BibliotecaController : Controller
    {
        
        private readonly ApplicationDbContext _context;

        public BibliotecaController()
        {
            _context = new ApplicationDbContext();
        }

        [HttpGet]
        public async Task<ActionResult> Index(string searchString)
        {
            // 1) Cargamos todas las guías incluyendo su categoría
            var query = _context.Guias.Include(g => g.Categoria).AsQueryable();

            
            if (!string.IsNullOrWhiteSpace(searchString))
            {
                query = query.Where(g =>
                    g.Titulo.Contains(searchString) ||
                    g.Contenido.Contains(searchString) ||
                    g.Keywords.Contains(searchString)
                );
            }

            
            var lista = await query.ToListAsync();

           
            ViewBag.CurrentFilter = searchString;

            return View(lista);
        }

      
        [HttpGet]
        public async Task<ActionResult> Details(int id)
        {
            
            var guia = await _context.Guias
                                     .Include(g => g.Categoria)
                                     .FirstOrDefaultAsync(g => g.Id == id);

            if (guia == null)
            {
                
                return HttpNotFound();
            }

            return View(guia);
        }

      
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Rate(int id, bool isUseful)
        {
            
            var guia = await _context.Guias.FindAsync(id);
            if (guia == null)
            {
                return HttpNotFound();
            }

            if (isUseful)
                guia.UsefulCount++;
            else
                guia.NotUsefulCount++;

            
            _context.Entry(guia).State = EntityState.Modified;

            
            await _context.SaveChangesAsync();

            
            return RedirectToAction("Details", new { id = id });
        }

        
        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                _context.Dispose();
            }
            base.Dispose(disposing);
        }
    }
}