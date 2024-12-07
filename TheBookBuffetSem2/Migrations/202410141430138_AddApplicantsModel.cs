namespace TheBookBuffetSem2.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class AddApplicantsModel : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.Applicants",
                c => new
                    {
                        ApplicantId = c.Int(nullable: false, identity: true),
                        FullName = c.String(nullable: false),
                        IDNumber = c.String(nullable: false),
                        Email = c.String(nullable: false),
                        MobileNumber = c.String(nullable: false),
                        Gender = c.String(nullable: false),
                        Ethnicity = c.String(),
                        CVBlobUrl = c.String(nullable: false),
                        IDBlobUrl = c.String(nullable: false),
                        ExtraDocumentBlobUrl = c.String(),
                        ExtraDocumentDescription = c.String(),
                        JobId = c.Int(nullable: false),
                        ApplicationDate = c.DateTime(nullable: false),
                    })
                .PrimaryKey(t => t.ApplicantId)
                .ForeignKey("dbo.Jobs", t => t.JobId, cascadeDelete: true)
                .Index(t => t.JobId);
            
        }
        
        public override void Down()
        {
            DropForeignKey("dbo.Applicants", "JobId", "dbo.Jobs");
            DropIndex("dbo.Applicants", new[] { "JobId" });
            DropTable("dbo.Applicants");
        }
    }
}
