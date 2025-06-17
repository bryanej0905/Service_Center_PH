namespace ServiceCenter.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class CrearTablaGuias : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.Guias",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        Titulo = c.String(nullable: false, maxLength: 200),
                        Contenido = c.String(nullable: false),
                        Keywords = c.String(maxLength: 500),
                    })
                .PrimaryKey(t => t.Id);
            
        }
        
        public override void Down()
        {
            DropTable("dbo.Guias");
        }
    }
}
