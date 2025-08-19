namespace ServiceCenter.Models
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class CrearInteraccionFAQ : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.InteraccionFAQs",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        PreguntaId = c.Int(nullable: false),
                        UserId = c.String(nullable: false, maxLength: 128),
                        EsUtil = c.Boolean(nullable: false),
                        Fecha = c.DateTime(nullable: false),
                        Usuario_Id = c.String(maxLength: 128),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.PreguntaFAQs", t => t.PreguntaId, cascadeDelete: true)
                .ForeignKey("dbo.AspNetUsers", t => t.Usuario_Id)
                .Index(t => t.PreguntaId)
                .Index(t => t.Usuario_Id);
            
        }
        
        public override void Down()
        {
            DropForeignKey("dbo.InteraccionFAQs", "Usuario_Id", "dbo.AspNetUsers");
            DropForeignKey("dbo.InteraccionFAQs", "PreguntaId", "dbo.PreguntaFAQs");
            DropIndex("dbo.InteraccionFAQs", new[] { "Usuario_Id" });
            DropIndex("dbo.InteraccionFAQs", new[] { "PreguntaId" });
            DropTable("dbo.InteraccionFAQs");
        }
    }
}
