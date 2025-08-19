namespace ServiceCenter.Models
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class FixUserIdLengthInGuiaInteracciones : DbMigration
    {
        public override void Up()
        {
            DropForeignKey("dbo.GuiaInteraccions", "UserId", "dbo.AspNetUsers");
            DropForeignKey("dbo.GuiaInteraccions", "GuiaId", "dbo.Guias");
            DropIndex("dbo.GuiaInteraccions", new[] { "GuiaId", "UserId" });
            AddColumn("dbo.GuiaInteraccions", "ArticuloId", c => c.Int());
            AddColumn("dbo.GuiaInteraccions", "Tipo", c => c.String());
            AlterColumn("dbo.GuiaInteraccions", "GuiaId", c => c.Int());
            AlterColumn("dbo.GuiaInteraccions", "UserId", c => c.String(maxLength: 128));
            CreateIndex("dbo.GuiaInteraccions", new[] { "GuiaId", "UserId" }, unique: true);
            CreateIndex("dbo.GuiaInteraccions", "ArticuloId");
            AddForeignKey("dbo.GuiaInteraccions", "ArticuloId", "dbo.Biblioteca_Items_New", "Id");
            AddForeignKey("dbo.GuiaInteraccions", "GuiaId", "dbo.Guias", "Id");
        }
        
        public override void Down()
        {
            DropForeignKey("dbo.GuiaInteraccions", "GuiaId", "dbo.Guias");
            DropForeignKey("dbo.GuiaInteraccions", "ArticuloId", "dbo.Biblioteca_Items_New");
            DropIndex("dbo.GuiaInteraccions", new[] { "ArticuloId" });
            DropIndex("dbo.GuiaInteraccions", new[] { "GuiaId", "UserId" });
            AlterColumn("dbo.GuiaInteraccions", "UserId", c => c.String(nullable: false, maxLength: 128));
            AlterColumn("dbo.GuiaInteraccions", "GuiaId", c => c.Int(nullable: false));
            DropColumn("dbo.GuiaInteraccions", "Tipo");
            DropColumn("dbo.GuiaInteraccions", "ArticuloId");
            CreateIndex("dbo.GuiaInteraccions", new[] { "GuiaId", "UserId" }, unique: true);
            AddForeignKey("dbo.GuiaInteraccions", "GuiaId", "dbo.Guias", "Id", cascadeDelete: true);
            AddForeignKey("dbo.GuiaInteraccions", "UserId", "dbo.AspNetUsers", "Id", cascadeDelete: true);
        }
    }
}
