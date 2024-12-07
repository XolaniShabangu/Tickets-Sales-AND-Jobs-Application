namespace TheBookBuffetSem2.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class addedcontract : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.Applicants", "ContractText", c => c.String());
        }
        
        public override void Down()
        {
            DropColumn("dbo.Applicants", "ContractText");
        }
    }
}
