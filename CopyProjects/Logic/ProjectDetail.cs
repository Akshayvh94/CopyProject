using CopyProjects.Models;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Web;
using System.Web.Hosting;

namespace CopyProjects.Logic
{
   
    public class ProjectDetail
    {
        public string logFileName;
        public string logPath;

        SuccErrorMsg sucerrmsg = new SuccErrorMsg();
        public string lastFailureMessage;
        
        public GetProjectDetail.ProjectStaus GetProjectStateByName(string _credentials, string URL, string project)
        {
           
            try
            {
                logPath = System.Web.Hosting.HostingEnvironment.MapPath("~/ApiLog");
                using (var client = new HttpClient())
                {
                    client.BaseAddress = new Uri(URL);
                    client.DefaultRequestHeaders.Accept.Clear();
                    client.DefaultRequestHeaders.Accept.Add(new System.Net.Http.Headers.MediaTypeWithQualityHeaderValue("application/json"));
                    client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", _credentials);

                    HttpResponseMessage response = client.GetAsync("_apis/projects/" + project + "?includeCapabilities=true&api-version=1.0").Result;

                    if (response.IsSuccessStatusCode)
                    {
                        GetProjectDetail.ProjectStaus obj = new GetProjectDetail.ProjectStaus();
                        string result = response.Content.ReadAsStringAsync().Result;
                        obj = JsonConvert.DeserializeObject<GetProjectDetail.ProjectStaus>(result);
                        return obj;
                    }
                    else
                    {

                        var errorMessage = response.Content.ReadAsStringAsync();
                        string error = Utility.GeterroMessage(errorMessage.Result.ToString());
                        this.lastFailureMessage = error;
                        sucerrmsg.errormsg.Add("Error in getting team project by name " + error);
                        logFileName = logPath + "\\CopyProjectErrors\\GetProjectStateByName_Errors_" + DateTime.Now.ToString("yyyyMMddHHmmss") + ".txt";
                        LogData("Vsts call has been made at:" + DateTime.Now + Environment.NewLine + Environment.NewLine + Environment.NewLine + Environment.NewLine + error);
                        return new GetProjectDetail.ProjectStaus();
                    }
                }
            }
            catch (Exception ex)
            {

                logFileName = logPath + "\\CopyProjectErrors\\GetProjectStateByName_Errors_" + DateTime.Now.ToString("yyyyMMddHHmmss") + ".txt";
                LogData("Vsts call has been made at:" + DateTime.Now + Environment.NewLine + Environment.NewLine + Environment.NewLine + Environment.NewLine + ex.Message);
                throw ex;
            }

             
    }
        private void LogData(string message)
        {            
           File.AppendAllText(logFileName, message);
        }

    }
}