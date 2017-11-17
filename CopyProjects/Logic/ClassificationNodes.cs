using CopyProjects.Models;
using CopyProjects.Models.ClassificationNodes;
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
using System.Web.Hosting;

namespace CopyProjects.Logic
{
    public class ClassificationNodes
    {
        public string logFileName;
        public string logPath;

        Config conf = new Config();
        SuccErrorMsg sucErrMsg = new SuccErrorMsg();
        RefreshToken refreshToken = new RefreshToken();
        AccessDetails Details = new AccessDetails();
        public GetNodesResponse.Nodes GetIterations(Config con)//string projectName, string URL, string _credentials, string srcProject)
        {
            try
            {
                logPath = HostingEnvironment.MapPath("~/ApiLog");

                GetNodesResponse.Nodes viewModel = new GetNodesResponse.Nodes();

                using (var client = new HttpClient())
                {
                    client.BaseAddress = new Uri(con.srcURL);
                    client.DefaultRequestHeaders.Accept.Clear();
                    client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                    client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", con._credentials);

                    HttpResponseMessage response = client.GetAsync(string.Format("{0}/_apis/wit/classificationNodes/iterations?$depth=5&api-version=1.0", con.project)).Result;
                    if (response.StatusCode == HttpStatusCode.NonAuthoritativeInformation)
                    {
                        Details = refreshToken.Refresh_AccessToken(con.refresh_token);
                        con.refresh_token = Details.refresh_token;
                        con._credentials = Details.access_token;
                        GetIterations(con);
                    }
                    else if (response.IsSuccessStatusCode)
                    {
                        string result = response.Content.ReadAsStringAsync().Result;
                        viewModel = JsonConvert.DeserializeObject<GetNodesResponse.Nodes>(result);
                        return viewModel;
                    }
                    else
                    {
                        var errorMessage = response.Content.ReadAsStringAsync();
                        string error = Utility.GeterroMessage(errorMessage.Result.ToString());

                        logFileName = logPath + "\\CopyProjectErrors\\GetIterations_Errors_" + DateTime.Now.ToString("yyyyMMddHHmmss") + ".txt";
                        LogData("Vsts call has been made at:" + DateTime.Now + Environment.NewLine + Environment.NewLine + Environment.NewLine + Environment.NewLine + error);


                        sucErrMsg.errormsg.Add("Unable to fetch Iterations" + Environment.NewLine + error);
                    }
                }
            }
            catch (Exception ex)
            {
                logFileName = logPath + "\\CopyProjectErrors\\Errors_" + DateTime.Now.ToString("yyyyMMddHHmmss") + ".txt";
                LogData("Vsts call has been made at:" + DateTime.Now + Environment.NewLine + Environment.NewLine + Environment.NewLine + Environment.NewLine + ex.Message);

                sucErrMsg.errormsg.Add("Unable to fetch Iterations" + Environment.NewLine + ex.Message);
            }
            return new GetNodesResponse.Nodes();

        }

        public GetNodeResponse.Node CreateIteration(string projectName, string path, Config con)
        {
            try
            {
                logPath = HostingEnvironment.MapPath("~/ApiLog");

                CreateUpdateNodeViewModel.Node node = new CreateUpdateNodeViewModel.Node()
                {
                    name = path
                };

                GetNodeResponse.Node viewModel = new GetNodeResponse.Node();

                using (var client = new HttpClient())
                {
                    client.DefaultRequestHeaders.Accept.Clear();
                    client.DefaultRequestHeaders.Accept.Add(new System.Net.Http.Headers.MediaTypeWithQualityHeaderValue("application/json"));
                    client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", con._credentials);

                    // serialize the fields array into a json string  
                    //var patchValue = new StringContent(JsonConvert.SerializeObject(team), Encoding.UTF8, "application/json");
                    var jsonContent = new StringContent(JsonConvert.SerializeObject(node), Encoding.UTF8, "application/json");
                    var method = new HttpMethod("POST");

                    var request = new HttpRequestMessage(method, con.targetURL + string.Format("/{0}/_apis/wit/classificationNodes/iterations?api-version=1.0", projectName)) { Content = jsonContent };
                    var response = client.SendAsync(request).Result;
                    if (response.StatusCode == HttpStatusCode.NonAuthoritativeInformation)
                    {
                        Details = refreshToken.Refresh_AccessToken(con.refresh_token);
                        con.refresh_token = Details.refresh_token;
                        con._credentials = Details.access_token;
                        CreateIteration(projectName, path, con);
                    }
                    else if (response.IsSuccessStatusCode)
                    {
                        string result = response.Content.ReadAsStringAsync().Result;
                        viewModel = JsonConvert.DeserializeObject<GetNodeResponse.Node>(result);
                        return viewModel;
                    }
                    else
                    {
                        var errorMessage = response.Content.ReadAsStringAsync();
                        string error = Utility.GeterroMessage(errorMessage.Result.ToString());
                        logFileName = logPath + "\\CopyProjectErrors\\CreateIteration_Errors_" + DateTime.Now.ToString("yyyyMMddHHmmss") + ".txt";
                        LogData("Vsts call has been made at:" + DateTime.Now + Environment.NewLine + Environment.NewLine + Environment.NewLine + Environment.NewLine + error);

                        sucErrMsg.errormsg.Add("Unable to create Iterations" + Environment.NewLine + error);

                    }
                }

            }
            catch (Exception ex)
            {
                logFileName = logPath + "\\CopyProjectErrors\\Errors_" + DateTime.Now.ToString("yyyyMMddHHmmss") + ".txt";
                LogData("Vsts call has been made at:" + DateTime.Now + Environment.NewLine + Environment.NewLine + Environment.NewLine + Environment.NewLine + ex.Message);

                sucErrMsg.errormsg.Add("Unable to create Iterations" + Environment.NewLine + ex.Message);
            }
            return new GetNodeResponse.Node();

        }

        public GetNodeResponse.Node MoveIteration(string projectName, string targetIteration, int sourceIterationId, Config con)
        {
            try
            {
                logPath = HostingEnvironment.MapPath("~/ApiLog");

                CreateUpdateNodeViewModel.Node node = new CreateUpdateNodeViewModel.Node()
                {
                    id = sourceIterationId
                };

                GetNodeResponse.Node viewModel = new GetNodeResponse.Node();

                using (var client = new HttpClient())
                {
                    client.DefaultRequestHeaders.Accept.Clear();
                    client.DefaultRequestHeaders.Accept.Add(new System.Net.Http.Headers.MediaTypeWithQualityHeaderValue("application/json"));
                    client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", con._credentials);

                    var jsonContent = new StringContent(JsonConvert.SerializeObject(node), Encoding.UTF8, "application/json");
                    var method = new HttpMethod("POST");

                    var request = new HttpRequestMessage(method, con.targetURL + string.Format("/{0}/_apis/wit/classificationNodes/iterations/{1}?api-version=1.0", projectName, targetIteration)) { Content = jsonContent };
                    var response = client.SendAsync(request).Result;
                    if (response.StatusCode == HttpStatusCode.NonAuthoritativeInformation)
                    {
                        Details = refreshToken.Refresh_AccessToken(con.refresh_token);
                        con.refresh_token = Details.refresh_token;
                        con._credentials = Details.access_token;
                        MoveIteration(projectName, targetIteration, sourceIterationId, con);
                    }
                    else if (response.IsSuccessStatusCode)
                    {
                        string result = response.Content.ReadAsStringAsync().Result;

                        viewModel = JsonConvert.DeserializeObject<GetNodeResponse.Node>(result);
                    }
                    else
                    {
                        var errorMessage = response.Content.ReadAsStringAsync();
                        string error = Utility.GeterroMessage(errorMessage.Result.ToString());
                        logFileName = logPath + "\\CopyProjectErrors\\MoveIteration_Errors_" + DateTime.Now.ToString("yyyyMMddHHmmss") + ".txt";
                        LogData("Vsts call has been made at:" + DateTime.Now + Environment.NewLine + Environment.NewLine + Environment.NewLine + Environment.NewLine + error);

                        sucErrMsg.errormsg.Add("Unable to move Iteration(s)" + Environment.NewLine + error);

                    }
                }

            }
            catch (Exception ex)
            {
                logFileName = logPath + "\\CopyProjectErrors\\Errors_" + DateTime.Now.ToString("yyyyMMddHHmmss") + ".txt";
                LogData("Vsts call has been made at:" + DateTime.Now + Environment.NewLine + Environment.NewLine + Environment.NewLine + Environment.NewLine + ex.Message);

                sucErrMsg.errormsg.Add("Unable to move Iteration(s)" + Environment.NewLine + ex.Message);

            }
            return new GetNodeResponse.Node();

        }
        public bool UpdateMoreIterationDates(string ProjectName, string templateType, Config con, string IterationName)
        {
            try
            {
                logPath = HostingEnvironment.MapPath("~/ApiLog");

                string project = ProjectName;
                DateTime StartDate = DateTime.Today.AddDays(-22);
                DateTime EndDate = DateTime.Today.AddDays(-1);

                Dictionary<string, string[]> sprint_dic = new Dictionary<string, string[]>();

                if (string.IsNullOrWhiteSpace(templateType) || templateType.ToLower() == TemplateType.Scrum.ToString().ToLower())
                {
                    sprint_dic.Add(IterationName, new string[] { StartDate.ToShortDateString(), EndDate.ToShortDateString() });
                }

                foreach (var key in sprint_dic.Keys)
                {
                    UpdateIterationDates(project, key, StartDate, EndDate, con);
                    StartDate = EndDate.AddDays(1);
                    EndDate = StartDate.AddDays(21);
                }
                return true;
            }
            catch (Exception ex)
            {
                logFileName = logPath + "\\CopyProjectErrors\\UpdateMoreIterationDates_Errors_" + DateTime.Now.ToString("yyyyMMddHHmmss") + ".txt";
                LogData("Vsts call has been made at:" + DateTime.Now + Environment.NewLine + Environment.NewLine + Environment.NewLine + Environment.NewLine + ex.Message);

                sucErrMsg.errormsg.Add("Error in updating Iteration dates" + Environment.NewLine + ex.Message);
            }
            return false;
        }
        public bool UpdateIterationDates(string ProjectName, string templateType, Config con)
        {
            try
            {
                logPath = HostingEnvironment.MapPath("~/ApiLog");

                string project = ProjectName;
                DateTime StartDate = DateTime.Today.AddDays(-22);
                DateTime EndDate = DateTime.Today.AddDays(-1);

                Dictionary<string, string[]> sprint_dic = new Dictionary<string, string[]>();

                if (string.IsNullOrWhiteSpace(templateType) || templateType.ToLower() == TemplateType.Scrum.ToString().ToLower())
                {
                    for (int i = 1; i <= 6; i++)
                    {
                        sprint_dic.Add("Sprint " + i, new string[] { StartDate.ToShortDateString(), EndDate.ToShortDateString() });
                    }
                }
                else
                {
                    for (int i = 1; i <= 3; i++)
                    {
                        sprint_dic.Add("Iteration " + i, new string[] { StartDate.ToShortDateString(), EndDate.ToShortDateString() });
                    }
                }

                foreach (var key in sprint_dic.Keys)
                {
                    UpdateIterationDates(project, key, StartDate, EndDate, con);
                    StartDate = EndDate.AddDays(1);
                    EndDate = StartDate.AddDays(21);
                }
                return true;
            }
            catch (Exception ex)
            {
                logFileName = logPath + "\\CopyProjectErrors\\UpdateIterationDates_Errors_" + DateTime.Now.ToString("yyyyMMddHHmmss") + ".txt";
                LogData("Vsts call has been made at:" + DateTime.Now + Environment.NewLine + Environment.NewLine + Environment.NewLine + Environment.NewLine + ex.Message);

                sucErrMsg.errormsg.Add("Error in updating Iteration dates" + Environment.NewLine + ex.Message);
            }
            return false;
        }
        public GetNodeResponse.Node UpdateIterationDates(string project, string path, DateTime startDate, DateTime finishDate, Config con)
        {
            try
            {
                logPath = HostingEnvironment.MapPath("~/ApiLog");


                CreateUpdateNodeViewModel.Node node = new CreateUpdateNodeViewModel.Node()
                {
                    //name = path,
                    attributes = new CreateUpdateNodeViewModel.Attributes()
                    {
                        startDate = startDate,
                        finishDate = finishDate
                    }
                };

                GetNodeResponse.Node viewModel = new GetNodeResponse.Node();

                using (var client = new HttpClient())
                {
                    client.DefaultRequestHeaders.Accept.Clear();
                    client.DefaultRequestHeaders.Accept.Add(new System.Net.Http.Headers.MediaTypeWithQualityHeaderValue("application/json"));
                    client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", con._credentials);

                    // serialize the fields array into a json string          
                    var patchValue = new StringContent(JsonConvert.SerializeObject(node), Encoding.UTF8, "application/json");
                    var method = new HttpMethod("PATCH");

                    // send the request
                    var request = new HttpRequestMessage(method, con.targetURL + project + "/_apis/wit/classificationNodes/iterations/" + path + "?api-version=1.0") { Content = patchValue };
                    var response = client.SendAsync(request).Result;

                    if (response.IsSuccessStatusCode)
                    {
                        string result = response.Content.ReadAsStringAsync().Result;
                        viewModel = JsonConvert.DeserializeObject<GetNodeResponse.Node>(result);
                        //viewModel.Message = "success";
                    }
                    //else
                    //{
                    //    string result = response.Content.ReadAsStringAsync().Result;

                    //    dynamic responseForInvalidStatusCode = JsonConvert.DeserializeXmlNode<dynamic>(result);
                    //    Newtonsoft.Json.Linq.JContainer msg = responseForInvalidStatusCode.Result;
                    //    viewModel.Message = msg.ToString();

                    //    var errorMessage = response.Content.ReadAsStringAsync();
                    //    string error = Utility.GeterroMessage(errorMessage.Result.ToString());
                    //    this.LastFailureMessage = error;
                    //}


                    return viewModel;
                }
            }
            catch (Exception ex)
            {
                logFileName = logPath + "\\CopyProjectErrors\\UpdateIterationDates_Errors_" + DateTime.Now.ToString("yyyyMMddHHmmss") + ".txt";
                LogData("Vsts call has been made at:" + DateTime.Now + Environment.NewLine + Environment.NewLine + Environment.NewLine + Environment.NewLine + ex.Message);

                sucErrMsg.errormsg.Add("Error in updating Iteration dates" + Environment.NewLine + ex.Message);
            }
            return new GetNodeResponse.Node();
        }
        //public bool RenameIteration(string projectName, Dictionary<string, string> IterationToUpdate)
        //{
        //    bool isSuccesfull = false;
        //    try
        //    {
        //        foreach (var key in IterationToUpdate.Keys)
        //        {
        //            CreateUpdateNodeViewModel.Node node = new CreateUpdateNodeViewModel.Node()
        //            {
        //                name = IterationToUpdate[key]
        //            };

        //            using (var client = new HttpClient())
        //            {
        //                client.DefaultRequestHeaders.Accept.Clear();
        //                client.DefaultRequestHeaders.Accept.Add(new System.Net.Http.Headers.MediaTypeWithQualityHeaderValue("application/json"));
        //                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Basic", _credentials);

        //                // serialize the fields array into a json string          
        //                var patchValue = new StringContent(JsonConvert.SerializeObject(node), Encoding.UTF8, "application/json");
        //                var method = new HttpMethod("PATCH");

        //                // send the request
        //                var request = new HttpRequestMessage(method, _configuration.UriString + projectName + "/_apis/wit/classificationNodes/Iterations/" + key + "?api-version=" + _configuration.VersionNumber) { Content = patchValue };
        //                var response = client.SendAsync(request).Result;

        //                if (response.IsSuccessStatusCode)
        //                {
        //                    isSuccesfull = true;
        //                }
        //            }
        //        }
        //        string project = projectName;
        //        DateTime StartDate = DateTime.Today.AddDays(-22);
        //        DateTime EndDate = DateTime.Today.AddDays(-1);

        //        Dictionary<string, string[]> sprint_dic = new Dictionary<string, string[]>();
        //        for (int i = 1; i <= IterationToUpdate.Count; i++)
        //        {
        //            sprint_dic.Add("Sprint " + i, new string[] { StartDate.ToShortDateString(), EndDate.ToShortDateString() });
        //        }
        //        foreach (var key in sprint_dic.Keys)
        //        {
        //            UpdateIterationDates(project, key, StartDate, EndDate);
        //            StartDate = EndDate.AddDays(1);
        //            EndDate = StartDate.AddDays(21);
        //        }

        //    }
        //    catch (Exception ex)
        //    {

        //    }
        //    return isSuccesfull;

        //}

        private void LogData(string message)
        {
            System.IO.File.AppendAllText(logFileName, message);
        }
    }
}