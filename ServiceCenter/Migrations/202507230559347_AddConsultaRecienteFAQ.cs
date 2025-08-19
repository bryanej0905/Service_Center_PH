namespace ServiceCenter.Models
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class AddConsultaRecienteFAQ : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.ConsultaRecienteFAQs",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        PreguntaFAQId = c.Int(nullable: false),
                        UsuarioId = c.String(nullable: false, maxLength: 128),
                        Fecha = c.DateTime(nullable: false),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.PreguntaFAQs", t => t.PreguntaFAQId, cascadeDelete: true)
                .ForeignKey("dbo.AspNetUsers", t => t.UsuarioId, cascadeDelete: true)
                .Index(t => t.PreguntaFAQId)
                .Index(t => t.UsuarioId);
            
        }
        
        public override void Down()
        {
            DropForeignKey("dbo.ConsultaRecienteFAQs", "UsuarioId", "dbo.AspNetUsers");
            DropForeignKey("dbo.ConsultaRecienteFAQs", "PreguntaFAQId", "dbo.PreguntaFAQs");
            DropIndex("dbo.ConsultaRecienteFAQs", new[] { "UsuarioId" });
            DropIndex("dbo.ConsultaRecienteFAQs", new[] { "PreguntaFAQId" });
            DropTable("dbo.ConsultaRecienteFAQs");
        }
    }
}
