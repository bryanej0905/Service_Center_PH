namespace ServiceCenter.Migrations
{
    using global::ServiceCenter.Models;
    using System;
    using System.Data.Entity;
    using System.Data.Entity.Migrations;
    using System.Linq;

    internal sealed class Configuration : DbMigrationsConfiguration<Models.ApplicationDbContext>
    {
        public Configuration()
        {
            AutomaticMigrationsEnabled = false;
        }

        protected override void Seed(Models.ApplicationDbContext context)
        {
            //  This method will be called after migrating to the latest version.

            //  You can use the DbSet<T>.AddOrUpdate() helper extension method
            //  to avoid creating duplicate seed data.
            if (!context.CategoriasFAQ.Any())
            {
                context.CategoriasFAQ.AddOrUpdate(
                    c => c.Nombre,
                    new CategoriaFAQ { Nombre = "Políticas de la empresa" },
                    new CategoriaFAQ { Nombre = "Horarios" },
                    new CategoriaFAQ { Nombre = "Pedidos y Productos" },
                    new CategoriaFAQ { Nombre = "Promociones" },
                    new CategoriaFAQ { Nombre = "Pago y Facturación" }
                );
            }

            // 2) Agregar algunas preguntas ejemplo para la primera categoría
            //    Nos aseguramos de recuperar el objeto CategoriaFAQ y asignar su Id
            var catPoliticas = context.CategoriasFAQ
                                     .FirstOrDefault(c => c.Nombre == "Políticas de la empresa");
            if (catPoliticas != null && !context.PreguntasFAQ.Any(p => p.CategoriaFAQId == catPoliticas.Id))
            {
                context.PreguntasFAQ.AddOrUpdate(
                    p => p.Titulo,
                    new PreguntaFAQ
                    {
                        Titulo = "¿Cuáles son los métodos de pago aceptados?",
                        Respuesta = "Aceptamos tarjetas de crédito y débito (Visa, Mastercard, American Express), así como pagos en efectivo al momento de la entrega. Algunas ubicaciones también permiten pagos con billeteras digitales.",
                        CategoriaFAQId = catPoliticas.Id,
                        FechaCreacion = DateTime.Now,
                        VecesConsultada = 0
                    },
                    new PreguntaFAQ
                    {
                        Titulo = "¿Puedo modificar los ingredientes de una pizza al hacer un pedido?",
                        Respuesta = "Sí, puedes personalizar los ingredientes a tu gusto. Algunas promociones no permiten cambios, pero normalmente se puede modificar mediante nuestro sistema en línea o solicitándolo al cajero.",
                        CategoriaFAQId = catPoliticas.Id,
                        FechaCreacion = DateTime.Now,
                        VecesConsultada = 0
                    }
                );
            }

            // Puedes repetir la inserción de preguntas de ejemplo para otras categorías, si lo deseas.

            // IMPORTANTE: al final, guardar cambios
            context.SaveChanges();
        }
    }
}
