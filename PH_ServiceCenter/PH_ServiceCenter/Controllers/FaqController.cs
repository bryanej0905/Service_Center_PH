using Microsoft.AspNetCore.Mvc;

namespace PH_ServiceCenter.Controllers
{
    public class FaqController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }

        public IActionResult Sugerir()
        {
            return View();
        }

        [HttpPost]
        public IActionResult GuardarSugerencia(string pregunta, string email)
        {
            // Aquí podrías guardar en DB o enviar a revisión
            TempData["Mensaje"] = "¡Gracias por tu sugerencia!";
            return RedirectToAction("Index");
        }

        public IActionResult Historial()
        {
            return View();
        }
    }
}

