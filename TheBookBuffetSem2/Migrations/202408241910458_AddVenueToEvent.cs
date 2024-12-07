namespace TheBookBuffetSem2.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class AddVenueToEvent : DbMigration
    {
        public override void Up()
        {
            RenameColumn(table: "dbo.Events", name: "Venue_VenueID", newName: "VenueID");
            RenameIndex(table: "dbo.Events", name: "IX_Venue_VenueID", newName: "IX_VenueID");
        }
        
        public override void Down()
        {
            RenameIndex(table: "dbo.Events", name: "IX_VenueID", newName: "IX_Venue_VenueID");
            RenameColumn(table: "dbo.Events", name: "VenueID", newName: "Venue_VenueID");
        }
    }
}
