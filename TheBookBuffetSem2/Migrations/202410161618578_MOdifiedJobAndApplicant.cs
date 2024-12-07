namespace TheBookBuffetSem2.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class MOdifiedJobAndApplicant : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.Jobs", "NumberOfPositions", c => c.Int(nullable: false));
        }
        
        public override void Down()
        {
            DropColumn("dbo.Jobs", "NumberOfPositions");
        }
    }
}
