namespace TheBookBuffetSem2.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class AddTicketModel : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.Tickets",
                c => new
                    {
                        TicketID = c.Int(nullable: false, identity: true),
                        EventID = c.Int(nullable: false),
                        Category = c.String(nullable: false, maxLength: 50),
                        Price = c.Decimal(nullable: false, precision: 18, scale: 2),
                        UserID = c.String(nullable: false, maxLength: 128),
                        QRCode = c.String(),
                    })
                .PrimaryKey(t => t.TicketID)
                .ForeignKey("dbo.Events", t => t.EventID, cascadeDelete: true)
                .ForeignKey("dbo.AspNetUsers", t => t.UserID, cascadeDelete: true)
                .Index(t => t.EventID)
                .Index(t => t.UserID);
            
        }
        
        public override void Down()
        {
            DropForeignKey("dbo.Tickets", "UserID", "dbo.AspNetUsers");
            DropForeignKey("dbo.Tickets", "EventID", "dbo.Events");
            DropIndex("dbo.Tickets", new[] { "UserID" });
            DropIndex("dbo.Tickets", new[] { "EventID" });
            DropTable("dbo.Tickets");
        }
    }
}
