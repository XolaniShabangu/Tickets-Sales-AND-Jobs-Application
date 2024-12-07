using Hangfire;
using Hangfire.SqlServer;
using Microsoft.Owin;
using Owin;
using Microsoft.Owin.Security.Cookies;
using Microsoft.Owin.Security.OpenIdConnect;
using System.Configuration;
using System.Threading.Tasks;

[assembly: OwinStartup(typeof(TheBookBuffetSem2.Startup))]
namespace TheBookBuffetSem2
{
    public partial class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            ConfigureAuth(app);

            // Configure Hangfire
            GlobalConfiguration.Configuration
                .UseSqlServerStorage("DefaultConnection"); // Use your connection string here

            // Enable the Hangfire dashboard (optional but recommended)
            app.UseHangfireDashboard("/hangfire");

            // Start the Hangfire server
            app.UseHangfireServer();
        }
    }
}





