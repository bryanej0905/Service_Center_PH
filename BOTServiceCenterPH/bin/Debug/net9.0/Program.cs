using System;
using System.Diagnostics;
using System.IO;

class Program
{
    static void Main(string[] args)
    {
        Console.WriteLine("Ejecutando archivo Bot_PH.bat...");

        string batPath = Path.Combine(Directory.GetCurrentDirectory(), "Bot_PH.bat");

        if (File.Exists(batPath))
        {
            try
            {
                Process.Start(new ProcessStartInfo
                {
                    FileName = batPath,
                    UseShellExecute = true,
                    WindowStyle = ProcessWindowStyle.Normal
                });
                Console.WriteLine("Bot_PH.bat ejecutado correctamente.");
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error al ejecutar Bot_PH.bat: " + ex.Message);
            }
        }
        else
        {
            Console.WriteLine("El archivo Bot_PH.bat no fue encontrado en: " + batPath);
        }

        //string htmlPath = Path.Combine(Directory.GetCurrentDirectory(), "resources", "index.html");
        //if (File.Exists(htmlPath))
        //{
        //    Process.Start(new ProcessStartInfo
        //    {
        //        FileName = htmlPath,
        //        UseShellExecute = true
        //    });
        //}
    }
}
