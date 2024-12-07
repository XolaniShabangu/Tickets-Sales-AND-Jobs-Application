namespace TheBookBuffetSem2.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class AddedFAQsAndUserQuestion : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.FAQs",
                c => new
                    {
                        FAQId = c.Int(nullable: false, identity: true),
                        Question = c.String(nullable: false, maxLength: 200),
                        Answer = c.String(nullable: false, maxLength: 1000),
                        IsVisible = c.Boolean(nullable: false),
                    })
                .PrimaryKey(t => t.FAQId);
            
            CreateTable(
                "dbo.UserQuestions",
                c => new
                    {
                        QuestionId = c.Int(nullable: false, identity: true),
                        Question = c.String(nullable: false, maxLength: 200),
                        Answer = c.String(),
                        SubmittedOn = c.DateTime(nullable: false),
                        IsAnswered = c.Boolean(nullable: false),
                    })
                .PrimaryKey(t => t.QuestionId);
            
        }
        
        public override void Down()
        {
            DropTable("dbo.UserQuestions");
            DropTable("dbo.FAQs");
        }
    }
}
