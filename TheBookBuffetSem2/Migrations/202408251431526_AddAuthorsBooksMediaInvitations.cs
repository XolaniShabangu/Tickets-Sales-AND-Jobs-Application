namespace TheBookBuffetSem2.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class AddAuthorsBooksMediaInvitations : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.Authors",
                c => new
                    {
                        AuthorID = c.Int(nullable: false, identity: true),
                        Name = c.String(),
                        Email = c.String(),
                    })
                .PrimaryKey(t => t.AuthorID);
            
            CreateTable(
                "dbo.Books",
                c => new
                    {
                        BookID = c.Int(nullable: false, identity: true),
                        Title = c.String(),
                        Price = c.Decimal(nullable: false, precision: 18, scale: 2),
                        AuthorID = c.Int(nullable: false),
                    })
                .PrimaryKey(t => t.BookID)
                .ForeignKey("dbo.Authors", t => t.AuthorID, cascadeDelete: true)
                .Index(t => t.AuthorID);
            
            CreateTable(
                "dbo.Invitations",
                c => new
                    {
                        InvitationID = c.Int(nullable: false, identity: true),
                        Token = c.String(),
                        InviteeType = c.String(),
                        InviteeID = c.Int(nullable: false),
                        RSVPStatus = c.String(),
                        WillingToSpeak = c.Boolean(),
                        EventID = c.Int(),
                    })
                .PrimaryKey(t => t.InvitationID)
                .ForeignKey("dbo.Events", t => t.EventID)
                .Index(t => t.EventID);
            
            CreateTable(
                "dbo.Media",
                c => new
                    {
                        MediaID = c.Int(nullable: false, identity: true),
                        Name = c.String(),
                        Email = c.String(),
                    })
                .PrimaryKey(t => t.MediaID);
            
        }
        
        public override void Down()
        {
            DropForeignKey("dbo.Invitations", "EventID", "dbo.Events");
            DropForeignKey("dbo.Books", "AuthorID", "dbo.Authors");
            DropIndex("dbo.Invitations", new[] { "EventID" });
            DropIndex("dbo.Books", new[] { "AuthorID" });
            DropTable("dbo.Media");
            DropTable("dbo.Invitations");
            DropTable("dbo.Books");
            DropTable("dbo.Authors");
        }
    }
}
