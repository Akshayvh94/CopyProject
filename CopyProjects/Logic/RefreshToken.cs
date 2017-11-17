using CopyProjects.Models;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Web;
using System.Web.Hosting;

namespace CopyProjects.Logic
{
    public class RefreshToken
    {

        public string logFileName;
        public string logPath;
        public AccessDetails Refresh_AccessToken(string refreshToken)
        {
            logPath = HostingEnvironment.MapPath("~/ApiLog");

            using (var client = new HttpClient())
            {
                string redirectUri = System.Configuration.ConfigurationManager.AppSettings["RedirectUri"];
                string ClientSecret = System.Configuration.ConfigurationManager.AppSettings["ClientSecret"];
                var request = new HttpRequestMessage(HttpMethod.Post, "https://app.vssps.visualstudio.com/oauth2/token");
                var requestContent = string.Format(
                    "client_assertion_type=urn:ietf:params:oauth:client-assertion-type:jwt-bearer&client_assertion={0}&grant_type=refresh_token&assertion={1}&redirect_uri={2}",
                    HttpUtility.UrlEncode(ClientSecret),
                    HttpUtility.UrlEncode(refreshToken), redirectUri
                    );

                request.Content = new StringContent(requestContent, Encoding.UTF8, "application/x-www-form-urlencoded");
                try
                {
                    var response = client.SendAsync(request).Result;
                    if (response.IsSuccessStatusCode)
                    {
                        string result = response.Content.ReadAsStringAsync().Result;
                        AccessDetails accesDetails = JsonConvert.DeserializeObject<AccessDetails>(result);
                        return accesDetails;
                    }
                    else
                    {
                        return null;
                    }
                }
                catch (Exception ex)
                {
                    logFileName = logPath + "\\CopyProjectErrors\\Refresh_AccessToken_Errors_" + DateTime.Now.ToString("yyyyMMddHHmmss") + ".txt";
                    LogData("Vsts call has been made at:" + DateTime.Now + Environment.NewLine + Environment.NewLine + Environment.NewLine + Environment.NewLine + ex.Message);
                    return null;
                }
            }

        }
        private void LogData(string message)
        {

            System.IO.File.AppendAllText(logFileName, message);
        }

    }
}