using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

using System.IO;
using System.Data;
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

            // Create the DataRange Object
            DateRange dataRange = new DateRange() { StartDate = "2018-01-01", EndDate = "2018-03-07" };

            //Create the Metrics Object "ga:pageviews,ga:entrances"
            Metric sessions = new Metric { Expression = "ga:sessions", Alias = "Session" };

            //Crate the Dimensions Object. "ga:date,ga:pagePath,ga:landingPagePath,ga:source"
            Dimension browser = new Dimension { Name = "ga:browser" };

            //Create the ReportRequest Object
            ReportRequest reportRequest = new ReportRequest
            {
                ViewId = config.VIEW_ID,
                DateRanges = new List<DateRange>() { dataRange },
                Dimensions = new List<Dimension>() { browser },
                Metrics = new List<Metric>() { sessions }
            };

            List<ReportRequest> requests = new List<ReportRequest>();
            requests.Add(reportRequest);

            //Create the GetReportsRequest object
            GetReportsRequest getReport = new GetReportsRequest() { ReportRequests = requests };

            //Call the batchGet Method
            GetReportsResponse response = service.Reports.BatchGet(getReport).Execute();

            printResults(response.Reports);
            //writeResultsToFile();

        }

        private static void writeResultsToFile()
        {
            /// <summary>
            /// Method for writing the results to external file at given folder destination, 
            /// given view_id and format type
            /// </summary>
            using (StreamWriter writer = new StreamWriter(config.DESTINATION_FOLDER_PATH + "ga_" + config.VIEW_ID + config.DESTINATION_FILE_TYPE))
            {
                writer.WriteLine("This is test file");
            }
        }

        private static void printResults(IList<Report> reports)
        {
            foreach (Report report in reports)
            {
                ColumnHeader header = report.ColumnHeader;
                List<string> dimensionHeaders = (List<string>)header.Dimensions;

                List<MetricHeaderEntry> metricHeaders = (List<MetricHeaderEntry>)header.MetricHeader.MetricHeaderEntries;
                List<ReportRow> rows = (List<ReportRow>)report.Data.Rows;

                if (report.Data.Rows == null)
                {
                    Console.WriteLine("No Data Available in Google Analytics");
                    Console.ReadKey();
                }
                else
                {
                    foreach (ReportRow row in rows)
                    {
                        List<string> dimensions = (List<string>)row.Dimensions;
                        List<DateRangeValues> metrics = (List<DateRangeValues>)row.Metrics;

                        for (int i = 0; i < dimensionHeaders.Count() && i < dimensions.Count(); i++)
                        {
                            Console.WriteLine(dimensionHeaders[i] + ": " + dimensions[i]);
                        }

                        for (int j = 0; j < metrics.Count(); j++)
                        {
                            Console.WriteLine("Data Range (" + j + "): ");
                            DateRangeValues values = metrics[j];
                            for (int k = 0; k < values.Values.Count() && k < metricHeaders.Count(); k++)
                            {
                                Console.WriteLine(metricHeaders[k].Name + ": " + values.Values[k]);
                            }
                        }
                    }
                }
            }
        }

        static UserCredential GetCredential()
        {
            /// <summary>
            /// Reading Client Secret Credintials, and getting permission to access
            /// Scope (Analytics Analytics) with the respective email address
            /// Store the credintials information in the folder in Client device
            /// </summary>
            UserCredential credential;

            using (var stream =
                new FileStream(config.CLIENT_SECRET_JSON_RESOURCE_PATH, FileMode.Open, FileAccess.Read))
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
