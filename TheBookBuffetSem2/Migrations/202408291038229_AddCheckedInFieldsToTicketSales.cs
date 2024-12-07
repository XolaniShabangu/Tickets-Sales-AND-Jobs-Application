namespace TheBookBuffetSem2.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class AddCheckedInFieldsToTicketSales : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.TicketSales", "CheckedIn", c => c.Boolean(nullable: false));
            AddColumn("dbo.TicketSales", "CheckedInTime", c => c.DateTime());
        }
        
        public override void Down()
        {
            DropColumn("dbo.TicketSales", "CheckedInTime");
            DropColumn("dbo.TicketSales", "CheckedIn");
        }
    }
}
