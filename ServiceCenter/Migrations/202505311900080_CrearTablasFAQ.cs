namespace ServiceCenter.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class CrearTablasFAQ : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.CategoriaFAQs",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        Nombre = c.String(nullable: false, maxLength: 100),
                    })
                .PrimaryKey(t => t.Id);
            
            CreateTable(
                "dbo.PreguntaFAQs",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        Titulo = c.String(nullable: false, maxLength: 200),
                        Respuesta = c.String(nullable: false),
                        CategoriaFAQId = c.Int(nullable: false),
                        FechaCreacion = c.DateTime(nullable: false),
                        VecesConsultada = c.Int(nullable: false),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.CategoriaFAQs", t => t.CategoriaFAQId, cascadeDelete: true)
                .Index(t => t.CategoriaFAQId);
            
        }
        
        public override void Down()
        {
            DropForeignKey("dbo.PreguntaFAQs", "CategoriaFAQId", "dbo.CategoriaFAQs");
            DropIndex("dbo.PreguntaFAQs", new[] { "CategoriaFAQId" });
            DropTable("dbo.PreguntaFAQs");
            DropTable("dbo.CategoriaFAQs");
        }
    }
}
