namespace TheBookBuffetSem2.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class AddTicketImagePathToTicketSales : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.InvitedGuestTickets",
                c => new
                    {
                        InvitedGuestTicketID = c.Int(nullable: false, identity: true),
                        InvitationID = c.Int(nullable: false),
                        InviteeName = c.String(nullable: false),
                        InviteeType = c.String(nullable: false),
                        IssuedDate = c.DateTime(nullable: false),
                        QRCode = c.String(),
                        TicketImagePath = c.String(),
                    })
                .PrimaryKey(t => t.InvitedGuestTicketID)
                .ForeignKey("dbo.Invitations", t => t.InvitationID, cascadeDelete: true)
                .Index(t => t.InvitationID);
            
            AddColumn("dbo.TicketSales", "TicketImagePath", c => c.String());
        }
        
        public override void Down()
        {
            DropForeignKey("dbo.InvitedGuestTickets", "InvitationID", "dbo.Invitations");
            DropIndex("dbo.InvitedGuestTickets", new[] { "InvitationID" });
            DropColumn("dbo.TicketSales", "TicketImagePath");
            DropTable("dbo.InvitedGuestTickets");
        }
    }
}
