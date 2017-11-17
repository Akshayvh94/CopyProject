using CopyProjects.Models;
using CopyProjects.Models.Repository;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Web;
using System.Web.Hosting;

namespace CopyProjects.Logic
{

    public class SourceCodeImport
    {
        public string logFileName;
        public string logPath;

        SuccErrorMsg sucerrmsg = new SuccErrorMsg();

        string lastFailureMessage = string.Empty;
        public RepositoryResponse.Repository GetRepository(string project, string URL, string _credentials)
        {
            try
            {
                logPath = HostingEnvironment.MapPath("~/ApiLog");
                using (var client = new HttpClient())
                {
                    client.BaseAddress = new Uri(URL);
                    client.DefaultRequestHeaders.Accept.Clear();
                    client.DefaultRequestHeaders.Accept.Add(new System.Net.Http.Headers.MediaTypeWithQualityHeaderValue("application/json"));
                    client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", _credentials);

                    HttpResponseMessage response = client.GetAsync(URL + project + "/_apis/git/repositories?api-version=1.0").Result;
                    if (response.IsSuccessStatusCode)
                    {
                        RepositoryResponse.Repository repository = Newtonsoft.Json.JsonConvert.DeserializeObject<RepositoryResponse.Repository>(response.Content.ReadAsStringAsync().Result.ToString());
                        return repository;
                    }
                    else
                    {
                        var errorMessage = response.Content.ReadAsStringAsync();
                        string error = Utility.GeterroMessage(errorMessage.Result.ToString());
                        logFileName = logPath + "\\CopyProjectErrors\\GetRepository_Errors_" + DateTime.Now.ToString("yyyyMMddHHmmss") + ".txt";
                        LogData("Vsts call has been made at:" + DateTime.Now + Environment.NewLine + Environment.NewLine + Environment.NewLine + Environment.NewLine + error);

                        sucerrmsg.errormsg.Add("Error in fetching repository " + Environment.NewLine + error);
                        return new RepositoryResponse.Repository();
                    }
                }
            }
            catch (Exception ex)
            {
                logFileName = logPath + "\\CopyProjectErrors\\GetRepository_Errors_" + DateTime.Now.ToString("yyyyMMddHHmmss") + ".txt";
                LogData("Vsts call has been made at:" + DateTime.Now + Environment.NewLine + Environment.NewLine + Environment.NewLine + Environment.NewLine + ex.Message);
                throw ex;
            }
        }

        public RepositoryCreated.Created CreateRepository(string reponame, string tarAccName, string _crederntials, string jsonString)
        {
            try
            {
                logPath = HostingEnvironment.MapPath("~/ApiLog");
                string URL = "https://" + tarAccName + ".visualstudio.com/DefaultCollection/";
                using (var client = new HttpClient())
                {
                    client.BaseAddress = new Uri(URL);
                    client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                    client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", _crederntials);

                    var jsonContent = new StringContent(jsonString, Encoding.UTF8, "application/json");
                    var method = new HttpMethod("POST");
                    var request = new HttpRequestMessage(method, "_apis/git/repositories?api-version=1.0") { Content = jsonContent };
                    var response = client.SendAsync(request).Result;
                    if (response.IsSuccessStatusCode)
                    {
                        string repoCreated = response.Content.ReadAsStringAsync().Result;
                        RepositoryCreated.Created CreatedRepo = new RepositoryCreated.Created();
                        CreatedRepo = JsonConvert.DeserializeObject<RepositoryCreated.Created>(repoCreated);
                        return CreatedRepo;
                    }
                    else
                    {
                        var errorMessage = response.Content.ReadAsStringAsync();
                        string error = Utility.GeterroMessage(errorMessage.Result.ToString());

                        logFileName = logPath + "\\CopyProjectErrors\\CreateRepository_Errors_" + DateTime.Now.ToString("yyyyMMddHHmmss") + ".txt";
                        LogData("Vsts call has been made at:" + DateTime.Now + Environment.NewLine + Environment.NewLine + Environment.NewLine + Environment.NewLine + error);
                        sucerrmsg.errormsg.Add("Unable to create repository " + Environment.NewLine + error);

                    }
                }
            }
            catch (Exception ex)
            {
                logFileName = logPath + "\\CopyProjectErrors\\CreateRepository_Errors_" + DateTime.Now.ToString("yyyyMMddHHmmss") + ".txt";
                LogData("Vsts call has been made at:" + DateTime.Now + Environment.NewLine + Environment.NewLine + Environment.NewLine + Environment.NewLine + ex.Message);
                sucerrmsg.errormsg.Add("Error in creating repository " + Environment.NewLine + ex.Message);
            }
            return new RepositoryCreated.Created();

        }

        //public string GetSourceCode(string URL, string _credential, string RepoID, string Project)
        //{
        //    //return null;
        //    using (var client1 = new HttpClient())
        //    {
        //        client1.BaseAddress = new Uri(URL);
        //        client1.DefaultRequestHeaders.Accept.Clear();
        //        client1.DefaultRequestHeaders.Accept.Add(new System.Net.Http.Headers.MediaTypeWithQualityHeaderValue("application/json"));
        //        client1.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", _credential);

        //        HttpResponseMessage responseCode = client1.GetAsync(URL + Project + "/_apis/git/repositories/" + RepoID + "/importRequests?includeAbandoned=true&api-version=3.0-preview").Result;
        //        if (responseCode.IsSuccessStatusCode)
        //        {
        //            SourceCodeResponse.Code code = Newtonsoft.Json.JsonConvert.DeserializeObject<SourceCodeResponse.Code>(responseCode.Content.ReadAsStringAsync().Result.ToString());
        //            if (code.value != null)
        //            {

        //            }
        //        }
        //    }
        //    return null;
        //}

        public string CreateServiceEndPoint(string jsonString, string Credentials, string URL, string Project)
        {
            try
            {
                logPath = HostingEnvironment.MapPath("~/ApiLog");
                using (var client = new HttpClient())
                {
                    client.DefaultRequestHeaders.Clear();
                    client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                    client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", Credentials);

                    var jsonContent = new StringContent(jsonString, Encoding.UTF8, "application/json");
                    var method = new HttpMethod("POST");
                    var request = new HttpRequestMessage(method, URL + Project + "/_apis/distributedtask/serviceendpoints?api-version=3.0-preview.1") { Content = jsonContent };

                    var response = client.SendAsync(request).Result;
                    //POST https://{instance}/defaultcollection/{project}/_apis/distributedtask/serviceendpoints?[api-version={version}]
                    if (response.IsSuccessStatusCode)
                    {
                        string EndPointCreated = response.Content.ReadAsStringAsync().Result;
                        ServiceEndPointCreated.EndPoint CreatedSEP = new ServiceEndPointCreated.EndPoint();
                        CreatedSEP = JsonConvert.DeserializeObject<ServiceEndPointCreated.EndPoint>(EndPointCreated);
                        return CreatedSEP.id;
                    }
                    else
                    {
                        var errorMessage = response.Content.ReadAsStringAsync();
                        string error = Utility.GeterroMessage(errorMessage.Result.ToString());

                        logFileName = logPath + "\\CopyProjectErrors\\CreateServiceEndPoint_Errors_" + DateTime.Now.ToString("yyyyMMddHHmmss") + ".txt";
                        LogData("Vsts call has been made at:" + DateTime.Now + Environment.NewLine + Environment.NewLine + Environment.NewLine + Environment.NewLine + error);
                        sucerrmsg.errormsg.Add("Unable to create Service endpoint " + Environment.NewLine + error);
                    }
                }

            }
            catch (Exception ex)
            {
                logFileName = logPath + "\\CopyProjectErrors\\CreateServiceEndPoint_Errors_" + DateTime.Now.ToString("yyyyMMddHHmmss") + ".txt";
                LogData("Vsts call has been made at:" + DateTime.Now + Environment.NewLine + Environment.NewLine + Environment.NewLine + Environment.NewLine + ex.Message);
                sucerrmsg.errormsg.Add("Error in creating service endpoints " + Environment.NewLine + ex.Message);
            }
            return null;

        }

        public bool getSourceCodeFromGitHub(string json, string Project, string RepositoryID, string _credentials, string URL, string Reponame)
        {
            using (var client = new HttpClient())
            {
                logPath = HostingEnvironment.MapPath("~/ApiLog");

                client.DefaultRequestHeaders.Accept.Clear();
                client.DefaultRequestHeaders.Accept.Add(new System.Net.Http.Headers.MediaTypeWithQualityHeaderValue("application/json"));
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", _credentials);

                var jsonContent = new StringContent(json, Encoding.UTF8, "application/json");
                var method = new HttpMethod("POST");

                var request = new HttpRequestMessage(method, URL + Project + "/_apis/git/repositories/" + RepositoryID + "/importRequests?api-version=3.0-preview") { Content = jsonContent };
                var response = client.SendAsync(request).Result;

                if (response.IsSuccessStatusCode)
                {
                    return response.IsSuccessStatusCode;
                }
                else
                {
                    var errorMessage = response.Content.ReadAsStringAsync();
                    string error = Utility.GeterroMessage(errorMessage.Result.ToString());
                    logFileName = logPath + "\\CopyProjectErrors\\getSourceCodeFromGitHub_Errors_" + DateTime.Now.ToString("yyyyMMddHHmmss") + ".txt";
                    LogData("Vsts call has been made at:" + DateTime.Now + Environment.NewLine + Environment.NewLine + Environment.NewLine + Environment.NewLine + error);

                    sucerrmsg.errormsg.Add("Unable to fetch source code from  " + Reponame + Environment.NewLine + error);

                    this.lastFailureMessage = error;
                    return false;

                }
            }


        }
        private void LogData(string message)
        {

            System.IO.File.AppendAllText(logFileName, message);
        }

    }
}