using Google.Apis.Auth.OAuth2;
using Google.Apis.Calendar.v3;
using Google.Apis.Calendar.v3.Data;
using Google.Apis.Services;
using Google.Apis.Util.Store;
using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

public class GoogleCalendarService
{
    private static string[] Scopes = { CalendarService.Scope.Calendar };
    private static string ApplicationName = "TheBookBuffetScheduler";

    public static CalendarService GetCalendarService()
    {
        // Path to your service account JSON file
        var credentialFilePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "App_Data", "meetingsproject-438814-c33a821a98ed.json");

        GoogleCredential credential;
        using (var stream = new FileStream(credentialFilePath, FileMode.Open, FileAccess.Read))
        {
            credential = GoogleCredential.FromStream(stream)
                         .CreateScoped(Scopes);
        }

        // Create the Calendar service
        return new CalendarService(new BaseClientService.Initializer()
        {
            HttpClientInitializer = credential,
            ApplicationName = ApplicationName,
        });
    }
}
