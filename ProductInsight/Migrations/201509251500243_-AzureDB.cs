namespace ProductInsight.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class AzureDB : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.Results",
                c => new
                    {
                        id = c.Int(nullable: false, identity: true),
                        rev_id = c.Int(nullable: false),
                        rep_score = c.Single(nullable: false),
                        logisticsDept = c.String(),
                        financeDept = c.String(),
                        qualityDept = c.String(),
                    })
                .PrimaryKey(t => t.id);
            
            CreateTable(
                "dbo.ReviewsDetails",
                c => new
                    {
                        id = c.Int(nullable: false, identity: true),
                        AuthToken = c.String(nullable: false),
                        userID = c.String(nullable: false),
                        reviewID = c.String(nullable: false),
                        productID = c.String(nullable: false),
                        upvotes = c.Int(nullable: false),
                        downvotes = c.Int(nullable: false),
                        rating = c.Single(nullable: false),
                        reviewerText = c.String(nullable: false),
                        timestamp = c.DateTime(nullable: false),
                        Status = c.Boolean(nullable: false),
                    })
                .PrimaryKey(t => t.id);
            
        }
        
        public override void Down()
        {
            DropTable("dbo.ReviewsDetails");
            DropTable("dbo.Results");
        }
    }
}
