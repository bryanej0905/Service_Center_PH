using System.Collections.Generic;
using System.Data.Entity;           
using System.Linq;
using System.Threading.Tasks;          
using ServiceCenter.Models;         

namespace ServiceCenter.Services
{
    public class GuiaService
    {
        private readonly ApplicationDbContext _context;

        public GuiaService()
        {
           
            _context = new ApplicationDbContext();
        }

        /// <summary>
        /// Busca todas las guías cuyo Titulo, Contenido o Keywords contengan la cadena 'termino'.
        /// Si 'termino' es null o vacío, devuelve todas las guías.
        /// </summary>
        public async Task<List<Guia>> BuscarGuiasAsync(string termino)
        {
            if (string.IsNullOrWhiteSpace(termino))
            {
                return await _context.Guias.ToListAsync();
            }

            // En EF6 no está EF.Functions.Like, pero sí podemos usar Contains para SQL Server.
            return await _context.Guias
                                 .Where(g =>
                                     g.Titulo.Contains(termino) ||
                                     g.Contenido.Contains(termino) ||
                                     (g.Keywords != null && g.Keywords.Contains(termino)))
                                 .ToListAsync();
        }

        // Asegúrate de desechar el contexto al final (opcional)
        public void Dispose()
        {
            _context.Dispose();
        }
    }
}
