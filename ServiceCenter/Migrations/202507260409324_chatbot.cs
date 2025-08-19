namespace ServiceCenter.Models
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class chatbot : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.ManageIPChatBots",
                c => new
                {
                    Id = c.Int(nullable: false, identity: true),
                    Ip = c.String(nullable: false, maxLength: 100),
                    Puerto = c.Int(nullable: false),
                    CreadoPor = c.String(maxLength: 100),
                    FechaCreacion = c.DateTime(nullable: false, defaultValueSql: "GETDATE()"),
                })
                .PrimaryKey(t => t.Id);
        }

        public override void Down()
        {
            DropTable("dbo.ManageIPChatBots");
        }
    }
}
