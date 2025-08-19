namespace ServiceCenter.Models
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class NotMapped : DbMigration
    {
        public override void Up()
        {
            DropForeignKey("dbo.ReporteErrorGuias", "GuiaId", "dbo.Guias");
            DropIndex("dbo.ReporteErrorGuias", new[] { "GuiaId" });
        }
        
        public override void Down()
        {
            CreateIndex("dbo.ReporteErrorGuias", "GuiaId");
            AddForeignKey("dbo.ReporteErrorGuias", "GuiaId", "dbo.Guias", "Id", cascadeDelete: true);
        }
    }
}
