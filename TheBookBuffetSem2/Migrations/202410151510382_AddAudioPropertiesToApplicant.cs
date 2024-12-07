namespace TheBookBuffetSem2.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class AddAudioPropertiesToApplicant : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.Applicants", "AudioPenalty", c => c.Double());
            AddColumn("dbo.Applicants", "AudioBlobUrl", c => c.String());
        }
        
        public override void Down()
        {
            DropColumn("dbo.Applicants", "AudioBlobUrl");
            DropColumn("dbo.Applicants", "AudioPenalty");
        }
    }
}
