namespace ServiceCenter.Models
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class articulo : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.Biblioteca_Items_New",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        Titulo = c.String(nullable: false),
                        Contenido = c.String(nullable: false),
                        Categoria = c.String(nullable: false),
                        ImagenRuta = c.String(),
                        Usuario = c.String(),
                        FechaCreacion = c.DateTime(nullable: false),
                    })
                .PrimaryKey(t => t.Id);
            
        }
        
        public override void Down()
        {
            DropTable("dbo.Biblioteca_Items_New");
        }
    }
}
