namespace TheBookBuffetSem2.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class AddedPropToApplicant : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.Applicants", "IsCanceled", c => c.Boolean(nullable: false));
        }
        
        public override void Down()
        {
            DropColumn("dbo.Applicants", "IsCanceled");
        }
    }
}
