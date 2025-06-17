namespace ServiceCenter.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class AgregarCategoriasYContadoresCalificacion : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.Categorias",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        Nombre = c.String(nullable: false, maxLength: 100),
                    })
                .PrimaryKey(t => t.Id);
            
            AddColumn("dbo.Guias", "CategoriaId", c => c.Int(nullable: false));
            AddColumn("dbo.Guias", "UsefulCount", c => c.Int(nullable: false));
            AddColumn("dbo.Guias", "NotUsefulCount", c => c.Int(nullable: false));
            CreateIndex("dbo.Guias", "CategoriaId");
            AddForeignKey("dbo.Guias", "CategoriaId", "dbo.Categorias", "Id", cascadeDelete: true);
        }
        
        public override void Down()
        {
            DropForeignKey("dbo.Guias", "CategoriaId", "dbo.Categorias");
            DropIndex("dbo.Guias", new[] { "CategoriaId" });
            DropColumn("dbo.Guias", "NotUsefulCount");
            DropColumn("dbo.Guias", "UsefulCount");
            DropColumn("dbo.Guias", "CategoriaId");
            DropTable("dbo.Categorias");
        }
    }
}
