namespace ServiceCenter.Models
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class FixRelacionUsuarioEnInteraccionFAQ : DbMigration
    {
        public override void Up()
        {
            DropForeignKey("dbo.InteraccionFAQs", "Usuario_Id", "dbo.AspNetUsers");
            DropIndex("dbo.InteraccionFAQs", new[] { "Usuario_Id" });
            DropColumn("dbo.InteraccionFAQs", "UserId");
            RenameColumn(table: "dbo.InteraccionFAQs", name: "Usuario_Id", newName: "UserId");
            AlterColumn("dbo.InteraccionFAQs", "UserId", c => c.String(nullable: false, maxLength: 128));
            CreateIndex("dbo.InteraccionFAQs", "UserId");
            AddForeignKey("dbo.InteraccionFAQs", "UserId", "dbo.AspNetUsers", "Id", cascadeDelete: true);
        }
        
        public override void Down()
        {
            DropForeignKey("dbo.InteraccionFAQs", "UserId", "dbo.AspNetUsers");
            DropIndex("dbo.InteraccionFAQs", new[] { "UserId" });
            AlterColumn("dbo.InteraccionFAQs", "UserId", c => c.String(maxLength: 128));
            RenameColumn(table: "dbo.InteraccionFAQs", name: "UserId", newName: "Usuario_Id");
            AddColumn("dbo.InteraccionFAQs", "UserId", c => c.String(nullable: false, maxLength: 128));
            CreateIndex("dbo.InteraccionFAQs", "Usuario_Id");
            AddForeignKey("dbo.InteraccionFAQs", "Usuario_Id", "dbo.AspNetUsers", "Id");
        }
    }
}
