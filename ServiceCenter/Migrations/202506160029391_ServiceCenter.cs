namespace ServiceCenter.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class ServiceCenter : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.Adjuntoes",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        TicketId = c.Int(nullable: false),
                        NombreArchivo = c.String(),
                        RutaArchivo = c.String(),
                        FechaSubida = c.DateTime(nullable: false),
                        SubidoPorId = c.String(maxLength: 128),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.AspNetUsers", t => t.SubidoPorId)
                .ForeignKey("dbo.Tickets", t => t.TicketId, cascadeDelete: true)
                .Index(t => t.TicketId)
                .Index(t => t.SubidoPorId);
            
            CreateTable(
                "dbo.Tickets",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        Titulo = c.String(nullable: false, maxLength: 150),
                        Descripcion = c.String(),
                        Estado = c.String(nullable: false, maxLength: 20),
                        Prioridad = c.String(nullable: false, maxLength: 20),
                        Categoria = c.String(nullable: false, maxLength: 30),
                        FechaCreacion = c.DateTime(nullable: false),
                        UsuarioCreadorId = c.String(maxLength: 128),
                        TecnicoAsignadoId = c.String(maxLength: 128),
                        DepartamentoId = c.Int(nullable: false),
                        Solucion = c.String(),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.Departamentoes", t => t.DepartamentoId, cascadeDelete: true)
                .ForeignKey("dbo.AspNetUsers", t => t.TecnicoAsignadoId)
                .ForeignKey("dbo.AspNetUsers", t => t.UsuarioCreadorId)
                .Index(t => t.UsuarioCreadorId)
                .Index(t => t.TecnicoAsignadoId)
                .Index(t => t.DepartamentoId);
            
            CreateTable(
                "dbo.Departamentoes",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        Nombre = c.String(nullable: false, maxLength: 100),
                    })
                .PrimaryKey(t => t.Id);
            
            CreateTable(
                "dbo.Comentarios",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        TicketId = c.Int(nullable: false),
                        UsuarioId = c.String(maxLength: 128),
                        ComentarioTexto = c.String(),
                        Fecha = c.DateTime(nullable: false),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.Tickets", t => t.TicketId, cascadeDelete: true)
                .ForeignKey("dbo.AspNetUsers", t => t.UsuarioId)
                .Index(t => t.TicketId)
                .Index(t => t.UsuarioId);
            
            CreateTable(
                "dbo.Evidencias",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        TicketId = c.Int(nullable: false),
                        UrlArchivo = c.String(),
                        FechaSubida = c.DateTime(nullable: false),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.Tickets", t => t.TicketId, cascadeDelete: true)
                .Index(t => t.TicketId);
            
            CreateTable(
                "dbo.HistorialAsignacions",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        TicketId = c.Int(nullable: false),
                        AsignadoPorId = c.String(maxLength: 128),
                        AsignadoAId = c.String(maxLength: 128),
                        FechaAsignacion = c.DateTime(nullable: false),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.AspNetUsers", t => t.AsignadoAId)
                .ForeignKey("dbo.AspNetUsers", t => t.AsignadoPorId)
                .ForeignKey("dbo.Tickets", t => t.TicketId, cascadeDelete: true)
                .Index(t => t.TicketId)
                .Index(t => t.AsignadoPorId)
                .Index(t => t.AsignadoAId);
            
        }
        
        public override void Down()
        {
            DropForeignKey("dbo.HistorialAsignacions", "TicketId", "dbo.Tickets");
            DropForeignKey("dbo.HistorialAsignacions", "AsignadoPorId", "dbo.AspNetUsers");
            DropForeignKey("dbo.HistorialAsignacions", "AsignadoAId", "dbo.AspNetUsers");
            DropForeignKey("dbo.Evidencias", "TicketId", "dbo.Tickets");
            DropForeignKey("dbo.Comentarios", "UsuarioId", "dbo.AspNetUsers");
            DropForeignKey("dbo.Comentarios", "TicketId", "dbo.Tickets");
            DropForeignKey("dbo.Adjuntoes", "TicketId", "dbo.Tickets");
            DropForeignKey("dbo.Tickets", "UsuarioCreadorId", "dbo.AspNetUsers");
            DropForeignKey("dbo.Tickets", "TecnicoAsignadoId", "dbo.AspNetUsers");
            DropForeignKey("dbo.Tickets", "DepartamentoId", "dbo.Departamentoes");
            DropForeignKey("dbo.Adjuntoes", "SubidoPorId", "dbo.AspNetUsers");
            DropIndex("dbo.HistorialAsignacions", new[] { "AsignadoAId" });
            DropIndex("dbo.HistorialAsignacions", new[] { "AsignadoPorId" });
            DropIndex("dbo.HistorialAsignacions", new[] { "TicketId" });
            DropIndex("dbo.Evidencias", new[] { "TicketId" });
            DropIndex("dbo.Comentarios", new[] { "UsuarioId" });
            DropIndex("dbo.Comentarios", new[] { "TicketId" });
            DropIndex("dbo.Tickets", new[] { "DepartamentoId" });
            DropIndex("dbo.Tickets", new[] { "TecnicoAsignadoId" });
            DropIndex("dbo.Tickets", new[] { "UsuarioCreadorId" });
            DropIndex("dbo.Adjuntoes", new[] { "SubidoPorId" });
            DropIndex("dbo.Adjuntoes", new[] { "TicketId" });
            DropTable("dbo.HistorialAsignacions");
            DropTable("dbo.Evidencias");
            DropTable("dbo.Comentarios");
            DropTable("dbo.Departamentoes");
            DropTable("dbo.Tickets");
            DropTable("dbo.Adjuntoes");
        }
    }
}
