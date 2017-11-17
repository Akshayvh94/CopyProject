using CopyProjects.Models;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Web;
using System.Web.Hosting;
using System.Web.Mvc;

namespace CopyProjects.Controllers
{
    public class HomeController : Controller
    {
        public string logFileName;
        public string logPath;
        public ActionResult Index()
        {
            return View();
        }

        public ActionResult About()
        {
            ViewBag.Message = "Your application description page.";

            return View();
        }

        public ActionResult Test(AccessDetails model)
        {
            return View(model);
        }
        public ActionResult Contact()
        {
            ViewBag.Message = "Your contact page.";

            return View();
        }
        string accname = string.Empty;
        string PAT = string.Empty;
        string URL = string.Empty;

        public JsonResult GetprojectList(ProjectList.Authentication auth)
        {
            logPath = HostingEnvironment.MapPath("~/ApiLog");

            ProjectList.ProjectCount load = new ProjectList.ProjectCount();
            accname = auth.accname;
            PAT = auth.pat;
            URL = "https://" + accname + ".visualstudio.com/DefaultCollection/";
            string _credentials = auth.pat;
            //string _credentials = Convert.ToBase64String(System.Text.ASCIIEncoding.ASCII.GetBytes(string.Format("{0}:{1}", "", auth.pat)));
            try
            {
                using (var client = new HttpClient())
                {
                    client.BaseAddress = new Uri(URL);
                    client.DefaultRequestHeaders.Accept.Clear();
                    client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("appication/json"));
                    client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", _credentials);
                    HttpResponseMessage response = client.GetAsync("/_apis/projects?stateFilter=WellFormed&api-version=1.0").Result;
                    if (response.IsSuccessStatusCode)
                    {
                        string res = response.Content.ReadAsStringAsync().Result;
                        load = JsonConvert.DeserializeObject<ProjectList.ProjectCount>(res);
                    }
                    else
                    {
                        var res = response.Content.ReadAsStringAsync().Result;

                        logFileName = logPath + "\\CopyProjectErrors\\Errors_" + DateTime.Now.ToString("yyyyMMddHHmmss") + ".txt";
                        LogData("Vsts call has been made at:" + DateTime.Now + Environment.NewLine + Environment.NewLine + Environment.NewLine + Environment.NewLine + res);

                        string result = JsonConvert.DeserializeObject<dynamic>(res);
                        load.errmsg = result;
                    }

                }
            }
            catch (Exception ex)
            {
                logFileName = logPath + "\\CopyProjectErrors\\Errors_" + DateTime.Now.ToString("yyyyMMddHHmmss") + ".txt";
                LogData("Vsts call has been made at:" + DateTime.Now + Environment.NewLine + Environment.NewLine + Environment.NewLine + Environment.NewLine + ex.Message);

                load.errmsg = ex.Message.ToString();
                string message = ex.Message.ToString();
            }
            return Json(load,JsonRequestBehavior.AllowGet);
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