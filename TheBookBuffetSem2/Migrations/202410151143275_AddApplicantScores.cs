namespace TheBookBuffetSem2.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class AddApplicantScores : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.Applicants", "AudioSampleScore", c => c.Double());
            AddColumn("dbo.Applicants", "InterviewScore", c => c.Double());
        }
        
        public override void Down()
        {
            DropColumn("dbo.Applicants", "InterviewScore");
            DropColumn("dbo.Applicants", "AudioSampleScore");
        }
    }
}
