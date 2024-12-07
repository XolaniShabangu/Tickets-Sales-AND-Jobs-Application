namespace TheBookBuffetSem2.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class AddSurveyTable : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.Surveys",
                c => new
                    {
                        SurveyID = c.Int(nullable: false, identity: true),
                        EventID = c.Int(nullable: false),
                        UserID = c.String(nullable: false, maxLength: 128),
                        Question1Rating = c.Int(nullable: false),
                        Question2Rating = c.Int(nullable: false),
                        Question3Rating = c.Int(nullable: false),
                        Question4Rating = c.Int(nullable: false),
                        Question5Rating = c.Int(nullable: false),
                        TotalRating = c.Int(nullable: false),
                    })
                .PrimaryKey(t => t.SurveyID)
                .ForeignKey("dbo.Events", t => t.EventID, cascadeDelete: true)
                .ForeignKey("dbo.AspNetUsers", t => t.UserID, cascadeDelete: true)
                .Index(t => t.EventID)
                .Index(t => t.UserID);
            
        }
        
        public override void Down()
        {
            DropForeignKey("dbo.Surveys", "UserID", "dbo.AspNetUsers");
            DropForeignKey("dbo.Surveys", "EventID", "dbo.Events");
            DropIndex("dbo.Surveys", new[] { "UserID" });
            DropIndex("dbo.Surveys", new[] { "EventID" });
            DropTable("dbo.Surveys");
        }
    }
}
