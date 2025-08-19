namespace ServiceCenter.Models
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class AddFilteredUniqueIndexToGuiaInteraccion : DbMigration
    {
        public override void Up()
        {
            // Eliminar índice anterior si existe
            Sql(@"IF EXISTS (SELECT name FROM sys.indexes WHERE name = 'IX_GuiaId_UserId' AND object_id = OBJECT_ID('dbo.GuiaInteraccions'))
              DROP INDEX IX_GuiaId_UserId ON dbo.GuiaInteraccions;");

        // Crear nuevo índice filtrado
        Sql(@"CREATE UNIQUE INDEX IX_GuiaId_UserId
              ON dbo.GuiaInteraccions(GuiaId, UserId)
              WHERE GuiaId IS NOT NULL;");
    }
        
        public override void Down()
        {
            // Eliminar índice filtrado si se revierte
            Sql(@"DROP INDEX IX_GuiaId_UserId ON dbo.GuiaInteraccions;");
        }
    }
}
