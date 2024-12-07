namespace TheBookBuffetSem2.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class AddSurveyTable1 : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.Surveys", "SubmissionDate", c => c.DateTime(nullable: false));
        }
        
        public override void Down()
        {
            DropColumn("dbo.Surveys", "SubmissionDate");
        }
    }
}
