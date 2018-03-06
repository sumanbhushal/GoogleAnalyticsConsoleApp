using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

using System.IO;
using Google.Apis.AnalyticsReporting.v4;
using Google.Apis.AnalyticsReporting.v4.Data;
using Google.Apis.Auth.OAuth2;
using Google.Apis.Util.Store;
using Google.Apis.Services;

namespace GoogleAnalyticsConnector
{
    class Program
    {
        // Defining the Scope 
        static string[] Scopes = { AnalyticsReportingService.Scope.Analytics };

        static void Main(string[] args)
        {
            UserCredential userCredential;
            userCredential = GetCredential();

            // Create AnalyticsReportingService API service.
            var service = new AnalyticsReportingService(new BaseClientService.Initializer()
            {
                HttpClientInitializer = userCredential,
                ApplicationName = config.APPLICATION_NAME,
            });

        }

        static  UserCredential GetCredential()
        {
            /// <summary>
            /// Reading Client Secret Credintials, and getting permission to access
            /// Scope (Analytics Analytics) with the respective email address
            /// Store the credintials information in the folder in Client device
            /// </summary>
            UserCredential credential;

            using (var stream =
                new FileStream(config.APPLICATION_NAME, FileMode.Open, FileAccess.Read))
            {
                string credPath = System.Environment.GetFolderPath(
                    System.Environment.SpecialFolder.Personal);
                credPath = Path.Combine(credPath, ".credentials/client_credentials.json");

                credential = GoogleWebAuthorizationBroker.AuthorizeAsync(
                    GoogleClientSecrets.Load(stream).Secrets,
                    Scopes,
                    "user",
                    CancellationToken.None,
                    new FileDataStore(credPath, true)).Result;
                Console.WriteLine("Credential file saved to: " + credPath);
            }
            return credential;
        }
    }
}
