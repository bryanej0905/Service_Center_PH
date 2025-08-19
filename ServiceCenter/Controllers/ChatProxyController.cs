using System;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Web.Mvc;
using Newtonsoft.Json;
using ServiceCenter.Models;

namespace ServiceCenter.Controllers
{
    public class ChatProxyController : Controller
    {
        private readonly HttpClient _httpClient = new HttpClient();

        [HttpPost]
        public async Task<ActionResult> PostToBot(string message)
        {
            using (var db = new ApplicationDbContext())
            {
                // Suponemos que solo hay una IP activa, o tomamos la primera
                var config = db.ManageIPChatBot.FirstOrDefault();
                if (config == null)
                {
                    return new HttpStatusCodeResult(500, "No hay configuración del bot en la base de datos.");
                }

                string botUrl = $"http://{config.Ip}:{config.Puerto}/chat";

                var json = JsonConvert.SerializeObject(new { message });
                var content = new StringContent(json, Encoding.UTF8, "application/json");

                try
                {
                    var response = await _httpClient.PostAsync(botUrl, content);
                    var responseBody = await response.Content.ReadAsStringAsync();
                    return Content(responseBody, "application/json");
                }
                catch (Exception ex)
                {
                    return new HttpStatusCodeResult(502, $"Error al contactar al bot: {ex.Message}");
                }
            }
        }

        [HttpPost]
        public async Task<ActionResult> Crear_ticket(string nombre, string titulo, string descripcion)
        {
            using (var db = new ApplicationDbContext())
            {
                // Suponemos que solo hay una IP activa, o tomamos la primera
                var config = db.ManageIPChatBot.FirstOrDefault();
                if (config == null)
                {
                    return new HttpStatusCodeResult(500, "No hay configuración del bot en la base de datos.");
                }

                string botUrl = $"http://{config.Ip}:{config.Puerto}/crear_ticket";

                var json = JsonConvert.SerializeObject(new { nombre, titulo, descripcion });
                var content = new StringContent(json, Encoding.UTF8, "application/json");

                try
                {
                    var response = await _httpClient.PostAsync(botUrl, content);
                    var responseBody = await response.Content.ReadAsStringAsync();
                    return Content(responseBody, "application/json");
                }
                catch (Exception ex)
                {
                    return new HttpStatusCodeResult(502, $"Error al contactar al bot: {ex.Message}");
                }
            }
        }
    }
}
