namespace TheBookBuffetSem2.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class AddJobStatusToApplicant : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.Applicants", "JobStatus", c => c.String());
        }
        
        public override void Down()
        {
            DropColumn("dbo.Applicants", "JobStatus");
        }
    }
}
