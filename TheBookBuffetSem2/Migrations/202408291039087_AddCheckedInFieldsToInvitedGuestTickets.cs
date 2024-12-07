namespace TheBookBuffetSem2.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class AddCheckedInFieldsToInvitedGuestTickets : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.InvitedGuestTickets", "CheckedIn", c => c.Boolean(nullable: false));
            AddColumn("dbo.InvitedGuestTickets", "CheckedInTime", c => c.DateTime());
        }
        
        public override void Down()
        {
            DropColumn("dbo.InvitedGuestTickets", "CheckedInTime");
            DropColumn("dbo.InvitedGuestTickets", "CheckedIn");
        }
    }
}
