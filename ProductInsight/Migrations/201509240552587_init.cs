namespace ProductInsight.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class init : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.Features",
                c => new
                    {
                        id = c.Int(nullable: false, identity: true),
                        AuthToken = c.String(),
                        ProductID = c.String(),
                        Feature = c.String(),
                        FeatureScores = c.String(),
                    })
                .PrimaryKey(t => t.id);
            
        }
        
        public override void Down()
        {
            DropTable("dbo.Features");
        }
    }
}
