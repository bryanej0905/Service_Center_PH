using System;
using System.Diagnostics;
using System.IO;

class Program
{
    static void Main(string[] args)
    {
        Console.WriteLine("Abriendo página local...");

        string localPath = Path.Combine(Directory.GetCurrentDirectory(), "index.html");

        if (File.Exists(localPath))
        {
            try
            {
                Process.Start(new ProcessStartInfo
                {
                    FileName = localPath,
                    UseShellExecute = true
                });
                Console.WriteLine("Página abierta con éxito.");
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error al abrir index.html: " + ex.Message);
            }
        }
        else
        {
            Console.WriteLine("El archivo index.html no fue encontrado en: " + localPath);
        }
    }
}
