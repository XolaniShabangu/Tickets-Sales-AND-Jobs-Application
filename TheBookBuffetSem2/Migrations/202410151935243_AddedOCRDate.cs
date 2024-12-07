namespace TheBookBuffetSem2.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class AddedOCRDate : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.Applicants", "OCRPassedDate", c => c.DateTime());
        }
        
        public override void Down()
        {
            DropColumn("dbo.Applicants", "OCRPassedDate");
        }
    }
}
