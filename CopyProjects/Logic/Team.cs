using CopyProjects.Models;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Web;
using System.Web.Hosting;

namespace CopyProjects.Logic
{
    public class Team
    {
        public string logFileName;
        public string logPath;

        SuccErrorMsg sucerrmsg = new SuccErrorMsg();
        string lastFailureMessage = string.Empty;

        public string CreateArea(string projectName, string areaName, string URL, string _credentials)
        {
            string createdAreaName = string.Empty;

            object node = new { name = areaName };
            logPath = HostingEnvironment.MapPath("~/ApiLog");

            using (var client = new HttpClient())
            {
                client.DefaultRequestHeaders.Accept.Clear();
                client.DefaultRequestHeaders.Accept.Add(new System.Net.Http.Headers.MediaTypeWithQualityHeaderValue("application/json"));
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", _credentials);

                // serialize the fields array into a json string  
                //var patchValue = new StringContent(JsonConvert.SerializeObject(team), Encoding.UTF8, "application/json");
                var jsonContent = new StringContent(Newtonsoft.Json.JsonConvert.SerializeObject(node), Encoding.UTF8, "application/json");
                var method = new HttpMethod("POST");

                var request = new HttpRequestMessage(method, URL + projectName + "/_apis/wit/classificationNodes/areas?api-version=1.0") { Content = jsonContent };
                var response = client.SendAsync(request).Result;

                if (response.IsSuccessStatusCode)
                {
                    string result = response.Content.ReadAsStringAsync().Result;
                    JObject jobj = JObject.Parse(result);
                    createdAreaName = jobj["name"].ToString();
                    return createdAreaName;
                }
                else
                {
                    var errorMessage = response.Content.ReadAsStringAsync();
                    sucerrmsg.errormsg.Add("Error in creating area(s) " + areaName + Environment.NewLine + errorMessage);


                    logFileName = logPath + "\\CopyProjectErrors\\CreateArea_Errors_" + DateTime.Now.ToString("yyyyMMddHHmmss") + ".txt";
                    LogData("Vsts call has been made at:" + DateTime.Now + Environment.NewLine + Environment.NewLine + Environment.NewLine + Environment.NewLine + errorMessage);

                    //string error = Utility.GeterroMessage(errorMessage.Result.ToString());
                    //this.lastFailureMessage = error;
                    return string.Empty;
                }
            }
        }
        public bool SetAreaForTeams(string projectName, string teamName, string json, string URL, string _credentials)
        {
            logPath = HostingEnvironment.MapPath("~/ApiLog");
            using (var client = new HttpClient())
            {
                client.DefaultRequestHeaders.Accept.Clear();
                client.DefaultRequestHeaders.Accept.Add(new System.Net.Http.Headers.MediaTypeWithQualityHeaderValue("application/json"));
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", _credentials);

                var patchValue = new StringContent(json, Encoding.UTF8, "application/json");

                var method = new HttpMethod("PATCH");

                var request = new HttpRequestMessage(method, URL + projectName + "/" + teamName + "/_apis/work/teamsettings/teamfieldvalues?api-version=2.0-preview.1") { Content = patchValue };
                var response = client.SendAsync(request).Result;
                if (response.IsSuccessStatusCode)
                {
                    return true;
                }
                else
                {
                    var errorMessage = response.Content.ReadAsStringAsync();
                    string error = Utility.GeterroMessage(errorMessage.Result.ToString());
                    this.lastFailureMessage = error;
                    sucerrmsg.errormsg.Add("Error in Setting area(s) for team(s) " + teamName + Environment.NewLine + errorMessage);

                    logFileName = logPath + "\\CopyProjectErrors\\SetAreaForTeams_Errors_" + DateTime.Now.ToString("yyyyMMddHHmmss") + ".txt";
                    LogData("Vsts call has been made at:" + DateTime.Now + Environment.NewLine + Environment.NewLine + Environment.NewLine + Environment.NewLine + error);

                    return false;
                }
            }
        }


        public string GetTeamSetting(string project, string URL, string _credentials)
        {
            try
            {
                logPath = HostingEnvironment.MapPath("~/ApiLog");
                string jsonString = string.Empty;

                TeamSetting.Setting teamObj = new TeamSetting.Setting();
                using (var client = new HttpClient())
                {
                    client.BaseAddress = new Uri(URL);
                    client.DefaultRequestHeaders.Clear();
                    client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                    client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", _credentials);
                    HttpResponseMessage response = client.GetAsync(project + "_apis/work/teamsettings?api-version=3.0-preview").Result;
                    if (response.IsSuccessStatusCode)
                    {
                        string res = response.Content.ReadAsStringAsync().Result;
                        teamObj = JsonConvert.DeserializeObject<TeamSetting.Setting>(res);
                        return teamObj.backlogIteration.id;
                    }
                    else
                    {
                        return null;
                    }
                }

            }
            catch (Exception ex)
            {
                logFileName = logPath + "\\CopyProjectErrors\\GetTeamSetting_Errors_" + DateTime.Now.ToString("yyyyMMddHHmmss") + ".txt";
                LogData("Vsts call has been made at:" + DateTime.Now + Environment.NewLine + Environment.NewLine + Environment.NewLine + Environment.NewLine + ex.Message);
                throw ex;
            }
        }

        public TeamIteration.Iteration GetAllIterations(string project, string URL, string _credentials)
        {
            try
            {
                logPath = HostingEnvironment.MapPath("~/ApiLog");
                TeamIteration.Iteration viewModel = new TeamIteration.Iteration();

                using (var client = new HttpClient())
                {
                    client.BaseAddress = new Uri(URL);
                    client.DefaultRequestHeaders.Accept.Clear();
                    client.DefaultRequestHeaders.Accept.Add(new System.Net.Http.Headers.MediaTypeWithQualityHeaderValue("application/json"));
                    client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Basic", _credentials);

                    HttpResponseMessage response = client.GetAsync(project + "/_apis/work/teamsettings/iterations?api-version=v2.0-preview").Result;

                    if (response.IsSuccessStatusCode)
                    {
                        string res = response.Content.ReadAsStringAsync().Result;
                        viewModel = JsonConvert.DeserializeObject<TeamIteration.Iteration>(res);
                        return viewModel;
                    }
                    else
                    {
                        var res = response.Content.ReadAsStringAsync().Result;

                        logFileName = logPath + "\\CopyProjectErrors\\GetAllIterations_Errors_" + DateTime.Now.ToString("yyyyMMddHHmmss") + ".txt";
                        LogData("Vsts call has been made at:" + DateTime.Now + Environment.NewLine + Environment.NewLine + Environment.NewLine + Environment.NewLine + res);

                        return new TeamIteration.Iteration();
                    }
                }
            }
            catch (Exception ex)
            {
                logFileName = logPath + "\\CopyProjectErrors\\Errors_" + DateTime.Now.ToString("yyyyMMddHHmmss") + ".txt";
                LogData("Vsts call has been made at:" + DateTime.Now + Environment.NewLine + Environment.NewLine + Environment.NewLine + Environment.NewLine + ex.Message);

                throw ex;
            }
        }

        public bool SetIterationsForTeam(string IterationId, string teamName, string projectName, string URL, string _credentials)
        {
            try
            {
                logPath = HostingEnvironment.MapPath("~/ApiLog");
                object objJSON = new { id = IterationId };

                using (var client = new HttpClient())
                {
                    client.DefaultRequestHeaders.Accept.Clear();
                    client.DefaultRequestHeaders.Accept.Add(new System.Net.Http.Headers.MediaTypeWithQualityHeaderValue("application/json"));
                    client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Basic", _credentials);

                    var jsonContent = new StringContent(JsonConvert.SerializeObject(objJSON), Encoding.UTF8, "application/json");
                    var method = new HttpMethod("POST");

                    var request = new HttpRequestMessage(method, URL + projectName + "/" + teamName + "/_apis/work/teamsettings/iterations?api-version=v2.0-preview") { Content = jsonContent };
                    var response = client.SendAsync(request).Result;

                    if (response.IsSuccessStatusCode)
                    {
                        return true;
                    }
                    else
                    {
                        var errorMessage = response.Content.ReadAsStringAsync();
                        string error = Utility.GeterroMessage(errorMessage.Result.ToString());

                        logFileName = logPath + "\\CopyProjectErrors\\SetIterationsForTeam_Errors_" + DateTime.Now.ToString("yyyyMMddHHmmss") + ".txt";
                        LogData("Vsts call has been made at:" + DateTime.Now + Environment.NewLine + Environment.NewLine + Environment.NewLine + Environment.NewLine + error);
                        
                        this.lastFailureMessage = error;
                        return false;
                    }
                }
            }
            catch (Exception ex)
            {
                logFileName = logPath + "\\CopyProjectErrors\\SetIterationsForTeam_Errors_" + DateTime.Now.ToString("yyyyMMddHHmmss") + ".txt";
                LogData("Vsts call has been made at:" + DateTime.Now + Environment.NewLine + Environment.NewLine + Environment.NewLine + Environment.NewLine + ex.Message);
                throw ex;
            }
        }
        private void LogData(string message)
        {

            System.IO.File.AppendAllText(logFileName, message);
        }
    }
}