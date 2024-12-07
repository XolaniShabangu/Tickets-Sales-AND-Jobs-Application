namespace TheBookBuffetSem2.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class AddRemainingTicketsToTicket : DbMigration
    {
        public override void Up()
        {
            DropForeignKey("dbo.Tickets", "UserID", "dbo.AspNetUsers");
            DropIndex("dbo.Tickets", new[] { "UserID" });
            AddColumn("dbo.Tickets", "TotalTickets", c => c.Int(nullable: false));
            AddColumn("dbo.Tickets", "RemainingTickets", c => c.Int(nullable: false));
            AlterColumn("dbo.Tickets", "Category", c => c.String(nullable: false));
            AlterColumn("dbo.Tickets", "UserID", c => c.String());
        }
        
        public override void Down()
        {
            AlterColumn("dbo.Tickets", "UserID", c => c.String(nullable: false, maxLength: 128));
            AlterColumn("dbo.Tickets", "Category", c => c.String(nullable: false, maxLength: 50));
            DropColumn("dbo.Tickets", "RemainingTickets");
            DropColumn("dbo.Tickets", "TotalTickets");
            CreateIndex("dbo.Tickets", "UserID");
            AddForeignKey("dbo.Tickets", "UserID", "dbo.AspNetUsers", "Id", cascadeDelete: true);
        }
    }
}
