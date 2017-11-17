using CopyProjects.Models;
using CopyProjects.Models.Area;
using CopyProjects.Models.WorkItems;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Web;
using System.Net;
using static CopyProjects.Models.Area.Areas;

namespace CopyProjects.Logic
{


    public class AreaMigrate
    {
        public string logFileName;
        public string logPath;
        
        SuccErrorMsg sucerrmsg = new SuccErrorMsg();

        RefreshToken refreshToken = new RefreshToken();
        AccessDetails Details = new AccessDetails();

     
        public Areas.AreaList GetAreas(Config con)
        {
            logPath = System.Web.Hosting.HostingEnvironment.MapPath("~/ApiLog");
            Areas.AreaList obj = new Areas.AreaList();

            using (var client = new HttpClient())
            {
                client.BaseAddress = new Uri(con.srcURL);
                client.DefaultRequestHeaders.Accept.Clear();
                client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", con._credentials);
                HttpResponseMessage response = client.GetAsync(con.srcprojectID + "/_apis/wit/classificationNodes/areas?$depth=6&api-version=1.0").Result;

                if (response.StatusCode == HttpStatusCode.NonAuthoritativeInformation)
                {
                    Details = refreshToken.Refresh_AccessToken(con.refresh_token);
                    con._credentials = Details.access_token;
                    con.refresh_token = Details.refresh_token;
                    GetAreas(con);
                }
                else if (response.IsSuccessStatusCode)
                {
                    string res = response.Content.ReadAsStringAsync().Result;
                    obj = JsonConvert.DeserializeObject<Areas.AreaList>(res);
                }
                else
                {
                    var errorMessage = response.Content.ReadAsStringAsync().Result;

                    logFileName = logPath + "\\CopyProjectErrors\\GetAreas_Errors_" + DateTime.Now.ToString("yyyyMMddHHmmss") + ".txt";
                    LogData("Vsts call has been made at:" + DateTime.Now + Environment.NewLine + Environment.NewLine + Environment.NewLine + Environment.NewLine + errorMessage);

                }
            }
            return obj;
        }
        public int CreateAreaPath(string project, string name, Config con)
        {
            logPath = System.Web.Hosting.HostingEnvironment.MapPath("~/ApiLog");
            if (string.IsNullOrEmpty(name)) return 0;

            CreateUpdateNodeViewModel.Node node = new CreateUpdateNodeViewModel.Node();
            node.name = name;

            GetNodeResponse.Node viewModel = new GetNodeResponse.Node();

            using (var client = new HttpClient())
            {
                client.BaseAddress = new Uri(con.targetURL);
                client.DefaultRequestHeaders.Accept.Clear();
                client.DefaultRequestHeaders.Accept.Add(new System.Net.Http.Headers.MediaTypeWithQualityHeaderValue("application/json"));
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", con._credentials);


                // serialize the fields array into a json string          
                var postValue = new StringContent(JsonConvert.SerializeObject(node), Encoding.UTF8, "application/json");
                var method = new HttpMethod("POST");

                // send the request
                string url = con.targetURL + project + "/_apis/wit/classificationNodes/areas?api-version=1.0";
                var request = new HttpRequestMessage(method, url) { Content = postValue };
                var response = client.SendAsync(request).Result;
                if (response.StatusCode == HttpStatusCode.NonAuthoritativeInformation)
                {
                    Details = refreshToken.Refresh_AccessToken(con.refresh_token);
                    con._credentials = Details.access_token;
                    con.refresh_token = Details.refresh_token;
                    CreateAreaPath(project, name, con);
                }
                else if (response.IsSuccessStatusCode)
                {
                    string res = response.Content.ReadAsStringAsync().Result;
                    viewModel = JsonConvert.DeserializeObject<GetNodeResponse.Node>(res);
                    viewModel.Message = "success";
                }
                else
                {
                    var errorMessage = response.Content.ReadAsStringAsync();
                    string error = Utility.GeterroMessage(errorMessage.Result.ToString());

                    logFileName = logPath + "\\CopyProjectErrors\\CreateAreaPath_Errors_" + DateTime.Now.ToString("yyyyMMddHHmmss") + ".txt";
                    LogData("Vsts call has been made at:" + DateTime.Now + Environment.NewLine + Environment.NewLine + Environment.NewLine + Environment.NewLine + error);


                    sucerrmsg.errormsg.Add(" Error Creating areas paths" + Environment.NewLine + error);
                }
                viewModel.HttpStatusCode = response.StatusCode;
                return viewModel.id;
            }
        }

        public int MoveArePath(string project, int childId, string parentName, Config con)
        {
            logPath = System.Web.Hosting.HostingEnvironment.MapPath("~/ApiLog");

            if (childId == 0) return 0;

            CreateUpdateNodeViewModel.Node node = new CreateUpdateNodeViewModel.Node();
            node.id = childId;

            GetNodeResponse.Node viewModel = new GetNodeResponse.Node();

            using (var client = new HttpClient())
            {
                client.BaseAddress = new Uri(con.targetURL);
                client.DefaultRequestHeaders.Accept.Clear();
                client.DefaultRequestHeaders.Accept.Add(new System.Net.Http.Headers.MediaTypeWithQualityHeaderValue("application/json"));
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", con._credentials);

                // serialize the fields array into a json string          
                var postValue = new StringContent(JsonConvert.SerializeObject(node), Encoding.UTF8, "application/json");
                var method = new HttpMethod("POST");

                // send the request
                string url = con.targetURL + project + "/_apis/wit/classificationNodes/areas/" + parentName + "?api-version=1.0";
                var request = new HttpRequestMessage(method, url) { Content = postValue };
                var response = client.SendAsync(request).Result;
                if (response.StatusCode == HttpStatusCode.NonAuthoritativeInformation)
                {
                    Details = refreshToken.Refresh_AccessToken(con.refresh_token);
                    con._credentials = Details.access_token;
                    con.refresh_token = Details.refresh_token;
                    MoveArePath(project, childId, parentName, con);
                }
                else if (response.IsSuccessStatusCode)
                {
                    string res = response.Content.ReadAsStringAsync().Result;
                    viewModel = JsonConvert.DeserializeObject<GetNodeResponse.Node>(res);
                    viewModel.Message = "success";
                }
                else
                {
                    var errorMessage = response.Content.ReadAsStringAsync();
                    string error = Utility.GeterroMessage(errorMessage.Result.ToString());
                    logFileName = logPath + "\\CopyProjectErrors\\MoveArePath_Errors_" + DateTime.Now.ToString("yyyyMMddHHmmss") + ".txt";
                    LogData("Vsts call has been made at:" + DateTime.Now + Environment.NewLine + Environment.NewLine + Environment.NewLine + Environment.NewLine + error);

                    sucerrmsg.errormsg.Add(" Error Moving areas paths" + Environment.NewLine + error);
                }
                viewModel.HttpStatusCode = response.StatusCode;
                return viewModel.id;

            }

        }

        #region LOGdATA
        private void LogData(string message)
        {
            //File.Create(logFileName);
            System.IO.File.AppendAllText(logFileName, message);
        }

        #endregion

    }
}