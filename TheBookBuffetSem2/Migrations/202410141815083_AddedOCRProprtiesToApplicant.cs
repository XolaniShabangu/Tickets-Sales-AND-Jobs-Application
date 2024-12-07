namespace TheBookBuffetSem2.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class AddedOCRProprtiesToApplicant : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.Applicants", "OCRStatus", c => c.String());
            AddColumn("dbo.Applicants", "OCRAccuracy", c => c.Double());
        }
        
        public override void Down()
        {
            DropColumn("dbo.Applicants", "OCRAccuracy");
            DropColumn("dbo.Applicants", "OCRStatus");
        }
    }
}
