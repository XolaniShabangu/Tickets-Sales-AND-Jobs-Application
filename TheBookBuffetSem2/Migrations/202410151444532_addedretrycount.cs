namespace TheBookBuffetSem2.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class addedretrycount : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.Applicants", "AudioRetryCount", c => c.Int());
        }
        
        public override void Down()
        {
            DropColumn("dbo.Applicants", "AudioRetryCount");
        }
    }
}
