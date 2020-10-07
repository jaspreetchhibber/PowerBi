using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.PowerBI.Api;
using Microsoft.PowerBI.Api.Models;
using Microsoft.Rest;
using Newtonsoft.Json;
using PowerBiApplication.Models;
using PowerBiApplication.PowerBI;

namespace PowerBiApplication.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        public static string _accessToken;
        public string AccessToken { get; set; }
        public AppSettings AppSettings { get; }
        IConfiguration _iconfiguration;

        public static string BaseUrl
        {
            get
            {
                return "https://login.microsoftonline.com/common/oauth2/token";
            }
        }

        public HomeController(ILogger<HomeController> logger, IConfiguration iconfiguration)
        {
            _logger = logger;
            _iconfiguration = iconfiguration;
            AppSettings = _iconfiguration.GetSection("AppSettings").Get<AppSettings>();
        }

        public async Task<IActionResult> Index()
        {
            await GetPowerBIAccessToken();
            return View();
        }

        public IActionResult Privacy()
        {
            return View();
        }
        public async Task<IActionResult> Report()
        {            
            AccessToken = _accessToken;
            ViewBag.AccessToken = AccessToken;
            Task.Run(async () => await EmbedReport()).Wait();
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
        public async Task<IActionResult> GetPowerBIAccessToken()
        {
            AccessToken=await GetAccessToken();
            _accessToken = AccessToken;
            using (var client = new PowerBIClient(new Uri(AppSettings.ApiUrl), new TokenCredentials(AccessToken, "Bearer")))

            {

                var workspaceId = Guid.Parse("084c63d2-08b8-4584-8355-9e0db915ff7b");//24d84fc1-27b9-491b-bf11-9c5e79d3ec1d

                var reportId = Guid.Parse("d9a989d6-cc20-4614-be56-554eb943b9c1");//9df5a78c-001f-479c-ad46-676da2a32988

                var report = await client.Reports.GetReportInGroupAsync(workspaceId, reportId);

                try

                {

                    var generateTokenRequestParameters = new GenerateTokenRequest(accessLevel: "view");

                    var tokenResponse = await client.Reports.GenerateTokenAsync(workspaceId, reportId, generateTokenRequestParameters);

                }

                catch (Exception ex)

                {

                }

                return Ok(new { token = AccessToken, embedUrl = report.EmbedUrl });

            }

        }

        public static async Task<HttpResponseMessage> MakeAsyncRequest(string url, Dictionary<string, string> content)
        {
            var httpClient = new HttpClient
            {
                Timeout = new TimeSpan(0, 5, 0),
                BaseAddress = new Uri(url)
            };

            httpClient.DefaultRequestHeaders.TryAddWithoutValidation("Content-Type: application/x-www-form-urlencoded", "application/json");

            if (content == null)
            {
                content = new Dictionary<string, string>();
            }

            var encodedContent = new FormUrlEncodedContent(content);

            var result = await httpClient.PostAsync(httpClient.BaseAddress, encodedContent);

            return result;
        }

        private async Task<string> GetAccessToken()
        {
            var url = BaseUrl;
            var content = new Dictionary<string, string>();
            content["grant_type"] = "password";
            content["resource"] = "https://analysis.windows.net/powerbi/api";
            content["username"] = AppSettings.UserName;
            content["password"] = AppSettings.Password;
            content["client_id"] = AppSettings.ApplicationId;

            var response = await MakeAsyncRequest(url, content);
            var tokenresult = response.Content.ReadAsStringAsync().Result;
            var AAD = JsonConvert.DeserializeObject<PowerBI.Models.AAD>(tokenresult);

            return AAD.AccessToken;
        }
        private async Task EmbedReport()
        {
            using (var client = new PowerBIClient(new Uri(AppSettings.ApiUrl), new TokenCredentials(AccessToken, "Bearer")))
            {
                Guid groupId = (await client.Groups.GetGroupsAsync()).Value.FirstOrDefault().Id;
                
                if (groupId == Guid.Empty)
                {
                    // no groups available for user
                    string message = "No group available, need to create a group and upload a report";
                    Response.Redirect($"Error?message={message}");
                }

                // getting first report in selected group from GetReports results
                Report report = (await client.Reports.GetReportsInGroupAsync(groupId)).Value.FirstOrDefault();

                if (report != null)
                {
                    ViewBag.EmbedUrl = report.EmbedUrl + "?rs:Command=Render&rc:Toolbar=true";
                    ViewBag.ReportId = report.Id;
                }
                else
                {
                    // no reports available for user in chosen group
                    // need to upload a report or insert a specific group id in appsettings.json
                    string message = "No report available in workspace with ID " + groupId + ", Please fill a group id with existing report in appsettings.json file";
                    Response.Redirect($"Error?message={message}");
                }
            }
        }
    }
}
