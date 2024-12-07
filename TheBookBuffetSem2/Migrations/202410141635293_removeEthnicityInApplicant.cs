namespace TheBookBuffetSem2.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class removeEthnicityInApplicant : DbMigration
    {
        public override void Up()
        {
            DropColumn("dbo.Applicants", "Ethnicity");
        }
        
        public override void Down()
        {
            AddColumn("dbo.Applicants", "Ethnicity", c => c.String());
        }
    }
}
