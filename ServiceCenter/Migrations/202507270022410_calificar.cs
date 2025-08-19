namespace ServiceCenter.Models
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class calificar : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.Tickets", "Calificado", c => c.Boolean(nullable: false));
        }
        
        public override void Down()
        {
            DropColumn("dbo.Tickets", "Calificado");
        }
    }
}
