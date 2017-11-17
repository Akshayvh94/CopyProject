using CopyProjects.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Web;
using System.Web.Mvc;
using System.Net.Http.Headers;
using Newtonsoft.Json;
using System.Text;
using Newtonsoft.Json.Linq;
using System.Diagnostics;
using CopyProjects.Logic;
using CopyProjects.Models.BoardColumnMod;
using CopyProjects.Models.Repository;
using CopyProjects.Models.WorkItems;
using CopyProjects.Models.ClassificationNodes;
using CopyProjects.Models.ProjectSetting;
using System.IO;
using CopyProjects.Models.Area;
using System.Net;
using System.Web.Hosting;


namespace CopyProjects.Controllers
{

    public class ProjectSetupController : Controller
    {
        public string logFileName;
        public string logPath;


        AccessDetails details = new AccessDetails();
        RefreshToken refreshToken = new RefreshToken();

        // GET: ProjectSetup
        public string lastFailureMessage;
        delegate string[] ProcessEnvironment(Dashboard obj);

        private static object objLock = new object();
        private static Dictionary<string, string> statusMessages;

        private static Dictionary<string, string> StatusMessages
        {
            get
            {
                if (statusMessages == null)
                {
                    statusMessages = new Dictionary<string, string>();
                }

                return statusMessages;
            }
            set
            {
                statusMessages = value;
            }
        }
        public ActionResult Index()
        {
            return View();
        }

        string Token = string.Empty;
        string URL = string.Empty;
        string targetURL = string.Empty;
        string _credentials = string.Empty;
        string newprojectID = string.Empty;
        string versionControl = string.Empty;


        ProjectDetail BL = new ProjectDetail();
        NewProject.NewPro newname = new NewProject.NewPro();
        Team teamMethods = new Team();


        SuccErrorMsg sucermsg = new SuccErrorMsg();
        Config con = new Config();
        string message = string.Empty;



        public string GetStatusMessage(string id)
        {
            lock (objLock)
            {
                string message = string.Empty;
                if (StatusMessages.Keys.Count(x => x == id) == 1)
                {
                    message = StatusMessages[id];
                }
                else
                {
                    return "";
                }

                if (id.EndsWith("_Errors"))
                {
                    RemoveKey(id);
                }

                return message;
            }
        }
        public void RemoveKey(string id)
        {
            lock (objLock)
            {
                StatusMessages.Remove(id);
            }
        }

        public void AddMessage(string id, string message)
        {
            lock (objLock)
            {
                if (id.EndsWith("_Errors"))
                {
                    StatusMessages[id] = (StatusMessages.ContainsKey(id) ? StatusMessages[id] : string.Empty) + message;
                }
                else
                {
                    StatusMessages[id] = message;
                }
            }
        }

        public ContentResult GetCurrentProgress(string id)
        {
            this.ControllerContext.HttpContext.Response.AddHeader("cache-control", "no-cache");
            var currentProgress = GetStatusMessage(id).ToString();
            return Content(currentProgress);
        }

        #region create project environment
        public bool StartEnvironmentSetupProcess(Dashboard obj)
        {
            if (obj.accessToken == null && obj.accountName == null && obj.NewProjectName == null && obj.SrcProjectName == null)
            {
                return false;
            }
            else
            {
                ProcessEnvironment processTask = new ProcessEnvironment(CreateprojectEnvironment);
                processTask.BeginInvoke(obj, new AsyncCallback(EndEnvironmentSetupProcess), processTask);
                return true;
            }
        }
        public void EndEnvironmentSetupProcess(IAsyncResult result)
        {
            //try
            //{
            ProcessEnvironment processTask = (ProcessEnvironment)result.AsyncState;
            string[] strResult = processTask.EndInvoke(result);

            RemoveKey(strResult[0]);
            if (StatusMessages.Keys.Count(x => x == strResult[0] + "_Errors") == 1)
            {
                string errorMessages = statusMessages[strResult[0] + "_Errors"];
                if (errorMessages != "")
                {
                    //also, log message to file system
                    string LogPath = Server.MapPath("~") + @"\Log";
                    string accountName = strResult[1];
                    string fileName = string.Format("{0}_{1}.txt", accountName, DateTime.Now.ToString("ddMMMyyyy_HHmmss"));

                    if (!Directory.Exists(LogPath))
                        Directory.CreateDirectory(LogPath);
                    System.IO.File.AppendAllText(Path.Combine(LogPath, fileName), errorMessages);
                }
            }
            //}
            //catch (Exception ex)
            //{
            //    AddMessage(ex.Data.Keys.ToString().ErrorId(), ex.Message);
            //}
        }
        public string[] CreateprojectEnvironment(Dashboard obj)
        {
            logPath = HostingEnvironment.MapPath("~/ApiLog");
            GetProjectDetail.Project proObj = new GetProjectDetail.Project();

            proObj = GetTeamProject(obj);
            if (proObj.id != null)
            {
                string jsonString = string.Empty;
                jsonString = System.IO.File.ReadAllText(Server.MapPath("~") + @"\JSON\CreateProject.json");
                jsonString = jsonString.Replace("$projectname$", obj.NewProjectName).Replace("$description$", proObj.description).Replace("$templateid$", proObj.capabilities.processTemplate.templateTypeId).Replace("$versioncontrol$", proObj.capabilities.versioncontrol.sourceControlType);

                newname = JsonConvert.DeserializeObject<NewProject.NewPro>(jsonString);

                string ProjectID = CreateProject(jsonString, obj);
                if (ProjectID != null)
                {
                    Stopwatch watch = new Stopwatch();
                    watch.Start();
                    string projectStatus = string.Empty;

                    GetProjectDetail.ProjectStaus status = new GetProjectDetail.ProjectStaus();


                    while (status.state != "wellFormed")
                    {
                        status = BL.GetProjectStateByName(_credentials, targetURL, newname.name);
                        if (status.state != null)
                        {
                            if (watch.Elapsed.Minutes > 4)
                            {
                                sucermsg.errormsg.Add("Unable to create team project");
                                AddMessage(obj.uid.ErrorId(), "Unable to create team project");
                                return new string[] { obj.uid, obj.accountName };
                            }
                            projectStatus = status.state;
                            ProjectID = status.id;
                            newprojectID = ProjectID;

                        }

                    }
                    sucermsg.successmsg.Add("Created " + newname.name + " project");
                    AddMessage(obj.uid, "Created " + newname.name + " project");
                    watch.Stop();

                }
                else
                {
                    AddMessage(obj.uid, lastFailureMessage);
                    System.Threading.Thread.Sleep(1000);
                    AddMessage(obj.uid, "end");
                    return new string[] { obj.uid.ErrorId(), obj.accountName };
                }


                con.project = obj.SrcProjectName;
                con.srcprojectID = obj.SelectedID;
                con.targetProject = newname.name;
                con.targetProjectID = ProjectID;
                con.srcURL = URL;
                con.targetURL = targetURL;
                con._credentials = _credentials;
                con.Template = proObj.capabilities.processTemplate.templateName;
                con.TemplateID = proObj.capabilities.processTemplate.templateTypeId;
                con.VersionControl = proObj.capabilities.versioncontrol.sourceControlType;
                con.guid = obj.uid;


                CreateMoveAreas(con); //configured error and success message

                GetListofTeams(obj); //configured error and success message

                importIterations(con, obj);// configured error and success message

                UpdateSprintItems(con);// configured error and success message

                EnableEpic(con);

                WorkItemcalls(con); // configured error and success message
                AddMessage(obj.uid, "Migrated Workitems");

                ImportSourceCode(obj);  //configured error and success message
                AddMessage(obj.uid, "Migrated Source Code");
                System.Threading.Thread.Sleep(1500);

                AddMessage(obj.uid, "https://" + obj.TargetAccountName + ".visualstudio.com/" + obj.NewProjectName);

                AddMessage(obj.uid, "end");
                System.Threading.Thread.Sleep(1500);

                //return View("../Account/Messageview", sucermsg);
            }
            else
            {
                AddMessage(obj.uid, "100");

                return new string[] { obj.uid.ErrorId(), obj.accountName };
            }
            return new string[] { obj.uid, obj.accountName };

        }

        #endregion

        #region Get and create project

        public GetProjectDetail.Project GetTeamProject(Dashboard obj)
        {

            logPath = HostingEnvironment.MapPath("~/ApiLog");
            GetProjectDetail.Project proObj = new GetProjectDetail.Project();
            URL = "https://" + obj.accountName + ".visualstudio.com/DefaultCollection/";
            targetURL = "https://" + obj.TargetAccountName + ".visualstudio.com/DefaultCollection/";
            _credentials = obj.accessToken;

            using (var client = new HttpClient())
            {
                client.BaseAddress = new Uri(URL);
                client.DefaultRequestHeaders.Clear();
                client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", _credentials);
                HttpResponseMessage response = client.GetAsync("_apis/projects/" + obj.SelectedID + "?includeCapabilities=true&api-version=1.0").Result;
                if (response.IsSuccessStatusCode)
                {
                    string res = response.Content.ReadAsStringAsync().Result;
                    proObj = JsonConvert.DeserializeObject<GetProjectDetail.Project>(res);

                    obj.VersionControl = proObj.capabilities.versioncontrol.sourceControlType;
                    versionControl = obj.VersionControl;
                    return proObj;
                }
                else
                {
                    var errorMessage = response.Content.ReadAsStringAsync();
                    string error = Utility.GeterroMessage(errorMessage.Result.ToString());
                    logFileName = logPath + "\\CopyProjectErrors\\GetTeamProject_Errors_" + DateTime.Now.ToString("yyyyMMddHHmmss") + ".txt";
                    LogData("Vsts call has been made at:" + DateTime.Now + Environment.NewLine + Environment.NewLine + Environment.NewLine + Environment.NewLine + error);

                    sucermsg.errormsg.Add(message);
                    AddMessage(obj.uid.ErrorId(), " Error is fetching Team Project");
                }
                return new GetProjectDetail.Project();
            }
        }
        public string CreateProject(string jsonString, Dashboard obj)
        {
            try
            {

                logPath = HostingEnvironment.MapPath("~/ApiLog");
                string targetaccount = obj.TargetAccountName;
                string url = "https://" + targetaccount + ".visualstudio.com/DefaultCollection/";
                using (var client = new HttpClient())
                {
                    client.BaseAddress = new Uri(url);
                    client.DefaultRequestHeaders.Clear();
                    client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                    client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", _credentials);

                    var jsonContent = new StringContent(jsonString, Encoding.UTF8, "application/json");
                    var method = new HttpMethod("POST");
                    var request = new HttpRequestMessage(method, "_apis/projects?api-version=2.0-preview") { Content = jsonContent };
                    var response = client.SendAsync(request).Result;
                    if (response.IsSuccessStatusCode)
                    {
                        string result = response.Content.ReadAsStringAsync().Result;
                        string projectId = JObject.Parse(result)["id"].ToString();
                        newprojectID = projectId;
                        AddMessage(obj.uid, "Creating team Project");
                        return projectId;
                    }
                    else
                    {
                        var errorMessage = response.Content.ReadAsStringAsync();
                        string error = Utility.GeterroMessage(errorMessage.Result.ToString());
                        this.lastFailureMessage = error;
                        sucermsg.errormsg.Add(" Error While creating team project" + Environment.NewLine + error);
                        AddMessage(obj.uid.ErrorId(), "Error While creating team project" + Environment.NewLine + error);

                        logFileName = logPath + "\\CopyProjectErrors\\CreateProject_Error_" + DateTime.Now.ToString("yyyyMMddHHmmss") + ".txt";
                        LogData("Vsts call has been made at:" + DateTime.Now + Environment.NewLine + Environment.NewLine + Environment.NewLine + Environment.NewLine + error);
                    }
                }

            }
            catch (Exception ex)
            {
                logFileName = logPath + "\\CopyProjectErrors\\CreateProject_Error_" + DateTime.Now.ToString("yyyyMMddHHmmss") + ".txt";
                LogData("Vsts call has been made at:" + DateTime.Now + Environment.NewLine + Environment.NewLine + Environment.NewLine + Environment.NewLine + ex.Message);

                sucermsg.errormsg.Add(" Error While creating team project" + Environment.NewLine + ex.Message);
                AddMessage(obj.uid.ErrorId(), "Error While creating team project" + Environment.NewLine + ex.Message);
            }
            return null;
        }
        #endregion

        #region Get and create Teams and team members

        public bool GetListofTeams(Dashboard obj)
        {
            try
            {
                logPath = HostingEnvironment.MapPath("~/ApiLog");
                string jsonString = string.Empty;

                Teams.TeamList teamObj = new Teams.TeamList();
                using (var client = new HttpClient())
                {
                    client.BaseAddress = new Uri(URL);
                    client.DefaultRequestHeaders.Clear();
                    client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                    client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", _credentials);
                    HttpResponseMessage response = client.GetAsync("_apis/projects/" + obj.SelectedID + "/teams?api-version=2.2").Result;
                    if (response.IsSuccessStatusCode)
                    {
                        string res = response.Content.ReadAsStringAsync().Result;
                        teamObj = JsonConvert.DeserializeObject<Teams.TeamList>(res);
                        if (teamObj.count > 0)
                        {
                            //int count = 0;
                            foreach (var team in teamObj.value)
                            {

                                BoardColumns boardcolobj = new BoardColumns();
                                //string boardname = boardcolobj.GetListBoardbyTeam(URL, _credentials, obj.SelectedID, team.name);

                                Teams.TargetTemasCreated newTeams = new Teams.TargetTemasCreated();
                                jsonString = System.IO.File.ReadAllText(Server.MapPath("~") + @"\JSON\CreateTeams.json");
                                //foreach (var teamCount in teamObj.value) {
                                jsonString = jsonString.Replace("$name$", team.name).Replace("$description$", team.description);
                                //}
                                newTeams = CreateTeams(jsonString, obj);

                                if (!(string.IsNullOrEmpty(newTeams.id)))
                                {
                                    //commented bcz, creating only teams, not setting areas paths for the team

                                    //string updateAreaJSON = System.IO.File.ReadAllText(Server.MapPath("~") + @"\JSON\TeamArea.json");

                                    //string areaName = teamMethods.CreateArea(newprojectID, team.name, URL, _credentials);
                                    //if (areaName != null)
                                    //{
                                    //    updateAreaJSON = updateAreaJSON.Replace("$ProjectName$", newname.name).Replace("$AreaName$", areaName);
                                    //    bool IsUpdated = teamMethods.SetAreaForTeams(newname.name, newTeams.name, updateAreaJSON, URL, _credentials);
                                    //    if (IsUpdated == true)
                                    //    {
                                    //        count++;
                                    //    }
                                    //}

                                    //if (IsUpdated == true)
                                    //{
                                    //    if (boardname != "-1")
                                    //    {
                                    //        var colObj = new GetListofColumns.Columns();
                                    //        colObj = boardcolobj.GetBoardColumbyBoard(URL, _credentials, obj.SelectedID, team.name, boardname);

                                    //        string coljson = JsonConvert.SerializeObject(colObj); 
                                    //        coljson = JsonConvert.SerializeObject(colObj);
                                    //        coljson = coljson.Substring(0, coljson.Length - 1); 
                                    //        coljson = coljson.Remove(0,9);

                                    //        bool IsUpdateBoard = boardcolobj.UpdateBoardColumn(URL, _credentials, coljson, newname.name, team.name, boardname);
                                    //    }
                                    //}
                                }
                            }
                            //sucermsg.successmsg.Add("Created " + count + " team(s) of " + teamObj.count + " team(s)");
                            AddMessage(obj.uid, "Created team(s)");
                           
                            return true;

                        }
                    }
                    else
                    {
                        var errorMessage = response.Content.ReadAsStringAsync();
                        string error = Utility.GeterroMessage(errorMessage.Result.ToString());

                        logFileName = logPath + "\\CopyProjectErrors\\GetListofTeams_Errors_" + DateTime.Now.ToString("yyyyMMddHHmmss") + ".txt";
                        LogData("Vsts call has been made at:" + DateTime.Now + Environment.NewLine + Environment.NewLine + Environment.NewLine + Environment.NewLine + error);

                        this.lastFailureMessage = error;
                        sucermsg.errormsg.Add("Error in geeting list of teams" + Environment.NewLine + error);
                        AddMessage(obj.uid.ErrorId(), "Error in geeting list of teams" + Environment.NewLine + error);

                    }
                }

            }
            catch (Exception ex)
            {
                logFileName = logPath + "\\CopyProjectErrors\\GetListofTeams_Errors_" + DateTime.Now.ToString("yyyyMMddHHmmss") + ".txt";
                LogData("Vsts call has been made at:" + DateTime.Now + Environment.NewLine + Environment.NewLine + Environment.NewLine + Environment.NewLine + ex.Message);

                sucermsg.errormsg.Add(" Error while getting teams" + Environment.NewLine + ex.Message);
                AddMessage(obj.uid.ErrorId(), "Error in geeting list of teams" + Environment.NewLine + ex.Message);
            }
            return false;

        }

        public Teams.TargetTemasCreated CreateTeams(string jsonString, Dashboard obj)
        {
            try
            {
                logPath = HostingEnvironment.MapPath("~/ApiLog");
                string targetaccount = obj.TargetAccountName;
                string url = "https://" + targetaccount + ".visualstudio.com/DefaultCollection/";
                using (var client = new HttpClient())
                {
                    client.BaseAddress = new Uri(url);
                    client.DefaultRequestHeaders.Clear();
                    client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                    client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", _credentials);

                    var jsonContent = new StringContent(jsonString, Encoding.UTF8, "application/json");
                    var method = new HttpMethod("POST");
                    var request = new HttpRequestMessage(method, "_apis/projects/" + newprojectID + "/teams?api-version=2.2") { Content = jsonContent };
                    var response = client.SendAsync(request).Result;
                    if (response.IsSuccessStatusCode)
                    {
                        Teams.TargetTemasCreated newteams = new Teams.TargetTemasCreated();
                        string result = response.Content.ReadAsStringAsync().Result;
                        newteams = JsonConvert.DeserializeObject<Teams.TargetTemasCreated>(result);
                        return newteams;
                    }
                    else
                    {
                        var errorMessage = response.Content.ReadAsStringAsync();
                        string error = Utility.GeterroMessage(errorMessage.Result.ToString());
                        this.lastFailureMessage = error;
                        logFileName = logPath + "\\CopyProjectErrors\\CreateTeams_Errors_" + DateTime.Now.ToString("yyyyMMddHHmmss") + ".txt";
                        LogData("Vsts call has been made at:" + DateTime.Now + Environment.NewLine + Environment.NewLine + Environment.NewLine + Environment.NewLine + error);


                        sucermsg.errormsg.Add(" Error while creating team(s)" + Environment.NewLine + errorMessage);
                        AddMessage(obj.uid.ErrorId(), " Error while creating team(s)" + Environment.NewLine + errorMessage);



                        return new Teams.TargetTemasCreated();
                    }
                }

            }
            catch (Exception ex)
            {
                logFileName = logPath + "\\CopyProjectErrors\\CreateTeams_Errors_" + DateTime.Now.ToString("yyyyMMddHHmmss") + ".txt";
                LogData("Vsts call has been made at:" + DateTime.Now + Environment.NewLine + Environment.NewLine + Environment.NewLine + Environment.NewLine + ex.Message);

                sucermsg.errormsg.Add(" Error while creating team(s)" + Environment.NewLine + ex.Message);
                AddMessage(obj.uid.ErrorId(), " Error while creating team(s)" + Environment.NewLine + ex.Message);

            }
            return new Teams.TargetTemasCreated();

        }

        public bool DeleteTeam(Config con)
        {
            try
            {
                using (var client = new HttpClient())
                {
                    client.BaseAddress = new Uri(con.targetURL);
                    client.DefaultRequestHeaders.Clear();
                    client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                    client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", con._credentials);

                    var method = new HttpMethod("DELETE");

                    var request = new HttpRequestMessage(method, con.targetURL + "_apis/projects/" + con.targetProject + "/teams/" + con.project + "%20Team?api-version=2.2");
                    var response = client.SendAsync(request).Result;
                    if (response.IsSuccessStatusCode)
                    {
                        return response.IsSuccessStatusCode;
                    }
                    else
                    {
                        return false;
                    }
                }
            }
            catch (Exception ex)
            {

            }
            return false;
        }

        //Get team Members but not adding yet.
        //public string GetTeamMembers(Dashboard obj, string teamName)
        //{
        //    try
        //    {
        //        string jsonString = string.Empty;

        //        TeamMembers.Members MemberObj = new TeamMembers.Members();
        //        using (var client = new HttpClient())
        //        {
        //            client.BaseAddress = new Uri(URL);
        //            client.DefaultRequestHeaders.Clear();
        //            client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
        //            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", _credentials);
        //            HttpResponseMessage response = client.GetAsync("_apis/projects/" + obj.SelectedID + "/teams/" + teamName + "/members/?api-version=2.2").Result;

        //            if (response.IsSuccessStatusCode)
        //            {
        //                string res = response.Content.ReadAsStringAsync().Result;
        //                MemberObj = JsonConvert.DeserializeObject<TeamMembers.Members>(res);
        //                if (MemberObj.count > 0)
        //                {

        //                }

        //                return null;
        //            }
        //            else
        //            {
        //                var errorMessage = response.Content.ReadAsStringAsync();
        //                string error = Utility.GeterroMessage(errorMessage.Result.ToString());
        //                this.lastFailureMessage = error;
        //            }
        //        }

        //    }
        //    catch (Exception ex)
        //    {
        //        sucermsg.errormsg.Add(" Error while fetching team member(s)" + Environment.NewLine + ex.Message);
        //        AddMessage(obj.uid.ErrorId(), " Error while fetching team member(s)" + Environment.NewLine + ex.Message);

        //    }
        //    return "-1";

        //}

        #endregion

        #region Import SourceCode

        public void ImportSourceCode(Dashboard obj)
        {
            try
            {
                logPath = HostingEnvironment.MapPath("~/ApiLog");
                SourceCodeImport Srcobj = new SourceCodeImport();
                RepositoryResponse.Repository repoObj = new RepositoryResponse.Repository();
                repoObj = Srcobj.GetRepository(obj.SrcProjectName, URL, _credentials);
                if (repoObj.count > 0)
                {
                    int codeFetchedcount = 0;
                    int repocreatedCount = 0;
                    sucermsg.successmsg.Add("Successfully fetched repository");
                    AddMessage(obj.uid, "Successfully fetched repository");

                    foreach (var repo in repoObj.value)
                    {
                        string jsonString = System.IO.File.ReadAllText(Server.MapPath("~") + @"\JSON\CreateRepo.json");
                        jsonString = jsonString.Replace("$reponame$", repo.name).Replace("$TargetProId$", newprojectID);
                        RepositoryCreated.Created RepoCreated = Srcobj.CreateRepository(repo.name, obj.TargetAccountName, _credentials, jsonString);
                        if (RepoCreated.id != null)
                        {

                            repocreatedCount++;

                            string EndPointString = System.IO.File.ReadAllText(Server.MapPath("~") + @"\JSON\ScServiceEndpoint.json");
                            //email replace with obj.Email;
                            EndPointString = EndPointString.Replace("$versioncontrol$", obj.VersionControl).Replace("$email$", obj.Email).Replace("$token$", _credentials).Replace("$SrcAccname$", obj.accountName).Replace("$Repo$", RepoCreated.name);
                            string SEPid = Srcobj.CreateServiceEndPoint(EndPointString, _credentials, targetURL, newname.name);
                            if (SEPid != null)
                            {
                                string ImportCodeString = System.IO.File.ReadAllText(Server.MapPath("~") + @"\JSON\ImportCode.json");
                                ImportCodeString = ImportCodeString.Replace("$SrcAccname$", obj.accountName).Replace("$Repo$", RepoCreated.name).Replace("$project$", obj.SrcProjectName).Replace("$endpointID$", SEPid);

                                bool isImported = Srcobj.getSourceCodeFromGitHub(ImportCodeString, newname.name, RepoCreated.id, _credentials, targetURL, RepoCreated.name);
                                if (isImported)
                                {
                                    codeFetchedcount++;
                                }
                            }

                        }
                    }
                    sucermsg.successmsg.Add("Created " + repocreatedCount + " of existing " + repoObj.count + " repository and fetched source code of " + codeFetchedcount + " repository(ies)");
                    AddMessage(obj.uid, "Created " + repocreatedCount + " of existing " + repoObj.count + " repository and fetched source code of " + codeFetchedcount + " repository(ies)");
                    System.Threading.Thread.Sleep(1000);

                }
            }
            catch (Exception ex)
            {
                logFileName = logPath + "\\CopyProjectErrors\\ImportSourceCode_Errors_" + DateTime.Now.ToString("yyyyMMddHHmmss") + ".txt";
                LogData("Vsts call has been made at:" + DateTime.Now + Environment.NewLine + Environment.NewLine + Environment.NewLine + Environment.NewLine + ex.Message);

                sucermsg.errormsg.Add(" Error in  importing source code" + Environment.NewLine + ex.Message);
                AddMessage(obj.uid.ErrorId(), "Error in  importing source code" + Environment.NewLine + ex.Message);

            }
        }
        #endregion

        #region Import Workitems--Handled with refresh Token

        public void WorkItemcalls(Config con)
        {
            try
            {
                logPath = HostingEnvironment.MapPath("~/ApiLog");
                WorkItem wit = new WorkItem();
                string ProjectSettingjson = System.IO.File.ReadAllText(Server.MapPath("~") + @"\JSON\Projectsetting.json");
                SuccErrorMsg scu = new SuccErrorMsg();
                con.projectSettingJson = ProjectSettingjson;

                scu = wit.GetWorkItemDetails(con);
                sucermsg.successmsg.Add("Imported Workitems");
                System.Threading.Thread.Sleep(1000);
                AddMessage(con.guid, "Migrated Workitems");

                foreach (var sc in scu.successmsg)
                {
                    sucermsg.successmsg.Add(sc);
                    AddMessage(con.guid, sc);


                }
                foreach (var er in scu.errormsg)
                {
                    sucermsg.errormsg.Add(er);
                    AddMessage(con.guid.ErrorId(), er);
                }
            }
            catch (Exception ex)
            {
                logFileName = logPath + "\\CopyProjectErrors\\WorkItemcalls_Errors_" + DateTime.Now.ToString("yyyyMMddHHmmss") + ".txt";
                LogData("Vsts call has been made at:" + DateTime.Now + Environment.NewLine + Environment.NewLine + Environment.NewLine + Environment.NewLine + ex.Message);


                sucermsg.errormsg.Add(" Error in  Work item calls" + Environment.NewLine + ex.Message);
                AddMessage(con.guid.ErrorId(), "Error in  Work item calls " + Environment.NewLine + ex.Message);
            }
        }
        #endregion

        #region Iteration--Handled with refresh Token

        public string importIterations(Config con, Dashboard obj)
        {
            try
            {
                logPath = HostingEnvironment.MapPath("~/ApiLog");
                ClassificationNodes ObjClassification = new ClassificationNodes();
                GetNodesResponse.Nodes nodes = new GetNodesResponse.Nodes();
                nodes = ObjClassification.GetIterations(con);

                if (nodes.name != null)
                {
                    if (nodes.hasChildren)
                    {
                        foreach (var child in nodes.children)
                        {
                            CreateIterationNode(con, ObjClassification, child, nodes);
                        }
                    }

                    if (nodes.hasChildren)
                    {
                        foreach (var child in nodes.children)
                        {
                            path = string.Empty;
                            MoveIterationNode(con, ObjClassification, child);
                            //ObjClassification.UpdateMoreIterationDates(con.targetProject, con.Template, con, child.name);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                logFileName = logPath + "\\CopyProjectErrors\\importIterations_Errors_" + DateTime.Now.ToString("yyyyMMddHHmmss") + ".txt";
                LogData("Vsts call has been made at:" + DateTime.Now + Environment.NewLine + Environment.NewLine + Environment.NewLine + Environment.NewLine + ex.Message);

                sucermsg.errormsg.Add(" Error in creating iterations" + Environment.NewLine + ex.Message);
                AddMessage(con.guid.ErrorId(), " Error in creating iterations" + Environment.NewLine + ex.Message);

            }
            return null;
        }

        private void CreateIterationNode(Config model, ClassificationNodes ObjClassification, GetNodesResponse.Child child, GetNodesResponse.Nodes currentIterations)
        {
            try
            {

                string[] defaultSprints = new string[] { "Sprint 1", "Sprint 2", "Sprint 3", "Sprint 4", "Sprint 5", "Sprint 6", };
                if (defaultSprints.Contains(child.name))
                {
                    var nd = (currentIterations.hasChildren) ? currentIterations.children.FirstOrDefault(i => i.name == child.name) : null;
                    if (nd != null) child.id = nd.id;
                }
                else
                {
                    var node = ObjClassification.CreateIteration(model.targetProject, child.name, model);
                    child.id = node.id;
                }

                if (child.hasChildren && child.children != null)
                {
                    foreach (var ch in child.children)
                    {
                        CreateIterationNode(model, ObjClassification, ch, currentIterations);
                    }
                }
            }
            catch (Exception ex)
            {
                sucermsg.errormsg.Add(" Error in creating iterations nodes" + Environment.NewLine + ex.Message);
                AddMessage(con.guid.ErrorId(), " Error in creating iterations nodes" + Environment.NewLine + ex.Message);
            }
        }

        string path = string.Empty;

        private void MoveIterationNode(Config model, ClassificationNodes ObjClassification, GetNodesResponse.Child child)
        {
            try
            {
                if (child.hasChildren && child.children != null)
                {
                    foreach (var c in child.children)
                    {
                        path += child.name + "\\";
                        var nd = ObjClassification.MoveIteration(model.targetProject, path, c.id, model);

                        if (c.hasChildren)
                        {
                            MoveIterationNode(model, ObjClassification, c);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                sucermsg.errormsg.Add(" Error in moving iterations nodes" + Environment.NewLine + ex.Message);
                AddMessage(con.guid.ErrorId(), " Error in moving iterations nodes" + Environment.NewLine + ex.Message);
            }
        }

        private void UpdateSprintItems(Config model)
        {
            try
            {
                ClassificationNodes ObjClassification = new ClassificationNodes();
                bool ClassificationNodesResult = ObjClassification.UpdateIterationDates(model.targetProject, model.Template, model);

                //if (!(string.IsNullOrEmpty(ObjClassification.LastFailureMessage)))
                //{
                //    AddMessage(model.id.ErrorId(), "Error while updating sprint items: " + ObjClassification.LastFailureMessage + Environment.NewLine);
                //}
            }
            catch (Exception ex)
            {
                sucermsg.errormsg.Add(" Error in updating spring items" + Environment.NewLine + ex.Message);
                AddMessage(con.guid.ErrorId(), " Error in updating spring items" + Environment.NewLine + ex.Message);
            }
        }

        #endregion

        #region Get and set areas--Handled with Refresh Token

        public void CreateMoveAreas(Config con)
        {
            AreaMigrate mig = new AreaMigrate();
            Areas.AreaList obj = mig.GetAreas(con);

            if (obj != null)
            {
                if (obj.hasChildren == true)
                {
                    foreach (var suite in obj.children)
                    {
                        string suiteName = suite.name;
                        int suitid = mig.CreateAreaPath(con.targetProject, suiteName, con);

                        var families = suite.children;
                        if (families != null)
                        {
                            if (families.Count > 0)
                            {
                                foreach (var family in families)
                                {
                                    string familyname = family.name;
                                    int familyid = mig.CreateAreaPath(con.targetProject, familyname, con);
                                    mig.MoveArePath(con.targetProject, familyid, suiteName, con);

                                    var products = family.children;
                                    if (products != null)
                                    {
                                        if (products.Count > 0)
                                        {
                                            foreach (var product in products)
                                            {
                                                string productname = product.name;
                                                int productid = mig.CreateAreaPath(con.targetProject, productname, con);
                                                mig.MoveArePath(con.targetProject, productid, suiteName + "\\" + familyname, con);

                                                var areas = product.children;
                                                if (areas != null)
                                                {
                                                    if (areas.Count > 0)
                                                    {
                                                        foreach (var area in areas)
                                                        {
                                                            string areaname = area.name;
                                                            int areaid = mig.CreateAreaPath(con.targetProject, areaname, con);
                                                            mig.MoveArePath(con.targetProject, productid, suiteName + "\\" + familyname + "\\" + productname, con);

                                                            var subareas = area.children;
                                                            if (subareas != null)
                                                            {
                                                                if (subareas.Count > 0)
                                                                {
                                                                    foreach (var subarea in subareas)
                                                                    {
                                                                        string subareaname = subarea.name;
                                                                        int subareaid = mig.CreateAreaPath(con.targetProject, subareaname, con);
                                                                        mig.MoveArePath(con.targetProject, productid, suiteName + "\\" + familyname + "\\" + productname + "\\" + areaname, con);
                                                                    }
                                                                }
                                                            }
                                                        }
                                                    }
                                                }
                                            }
                                        }
                                    }
                                }
                            }
                        }

                    }
                    AddMessage(con.guid, "Imported Areas");

                }
                else
                {
                    AddMessage(con.guid.ErrorId(), " Could not fetch areas or Area paths are not present");
                }
            }

        }
        #endregion

        #region Enable Epic--Handled with refresh Token
        public bool EnableEpic(Config con)
        {
            try
            {
                logPath = HostingEnvironment.MapPath("~/ApiLog");

                string jsonString = System.IO.File.ReadAllText(Server.MapPath("~") + @"\JSON\EnableEpic.json");

                using (var client = new HttpClient())
                {
                    client.DefaultRequestHeaders.Accept.Clear();
                    client.DefaultRequestHeaders.Accept.Add(new System.Net.Http.Headers.MediaTypeWithQualityHeaderValue("application/json"));
                    client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", con._credentials);

                    var jsonContent = new StringContent(jsonString, Encoding.UTF8, "application/json");
                    var method = new HttpMethod("PATCH");
                    string teamName = con.targetProject + " Team";
                    var request = new HttpRequestMessage(method, con.targetURL + con.targetProject + "/" + teamName + "/_apis/work/teamsettings?api-version=3.0-preview") { Content = jsonContent };
                    var response = client.SendAsync(request).Result;
                    if (response.StatusCode == HttpStatusCode.NonAuthoritativeInformation)
                    {
                        details = refreshToken.Refresh_AccessToken(con.refresh_token);
                        con._credentials = details.access_token;
                        con.refresh_token = details.refresh_token;
                        EnableEpic(con);
                    }
                    else if (response.IsSuccessStatusCode)
                    {
                        AddMessage(con.guid, " Enabled Epic");

                        return true;
                    }
                    else
                    {
                        var errorMessage = response.Content.ReadAsStringAsync();
                        string error = Utility.GeterroMessage(errorMessage.Result.ToString());

                        logFileName = logPath + "\\CopyProjectErrors\\EnableEpic_Errors_" + DateTime.Now.ToString("yyyyMMddHHmmss") + ".txt";
                        LogData("Vsts call has been made at:" + DateTime.Now + Environment.NewLine + Environment.NewLine + Environment.NewLine + Environment.NewLine + error);

                        AddMessage(con.guid.ErrorId(), error);
                    }
                }
            }
            catch (Exception ex)
            {
                logFileName = logPath + "\\CopyProjectErrors\\EnableEpic_Errors_" + DateTime.Now.ToString("yyyyMMddHHmmss") + ".txt";
                LogData("Vsts call has been made at:" + DateTime.Now + Environment.NewLine + Environment.NewLine + Environment.NewLine + Environment.NewLine + ex.Message);

                AddMessage(con.guid.ErrorId(), ex.Message);
            }
            return false;
        }
        #endregion

        #region VALIDATE WITcount and REPOcount

        public int GetTotalWorkItemCount(Dashboard con)
        {
            try
            {
                logPath = HostingEnvironment.MapPath("~/ApiLog");

                string aUrl = "https://" + con.accountName + ".visualstudio.com/DefaultCollection/";
                WorkItemCountResponse.Count viewModel = new WorkItemCountResponse.Count();
                Object wiql = new
                {
                    query = "select [System.Id], [System.WorkItemType], [System.Title], [System.AssignedTo], [System.State], [System.Tags] from WorkItems where [System.TeamProject] ='" + con.SrcProjectName + "' and [System.WorkItemType] <> '' and [System.State] <> ''"
                };
                //Checking requested User
                bool userexist = false;
                string Users = System.IO.File.ReadAllText(Server.MapPath("~") + @"\JSON\RequestedUser.json");
                UserRequest user = new UserRequest();
                user = JsonConvert.DeserializeObject<UserRequest>(Users);
                foreach (var requser in user.RequestUser)
                {
                    if (requser.ToLower() == con.Email.ToLower())
                        userexist = true;
                    return 1;
                }
                //  }
                if (userexist == false)
                {
                    //getting work item count, checking for 100+
                    using (var client = new HttpClient())
                    {
                        client.DefaultRequestHeaders.Accept.Clear();
                        client.DefaultRequestHeaders.Accept.Add(new System.Net.Http.Headers.MediaTypeWithQualityHeaderValue("application/json"));
                        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", con.accessToken);

                        var postValue = new StringContent(JsonConvert.SerializeObject(wiql), Encoding.UTF8, "application/json"); // mediaType needs to be application/json-patch+json for a patch call

                        // set the httpmethod to Patch
                        var method = new HttpMethod("POST");

                        // send the request               
                        var request = new HttpRequestMessage(method, aUrl + "_apis/wit/wiql?api-version=2.2") { Content = postValue };
                        var response = client.SendAsync(request).Result;

                        if (response.IsSuccessStatusCode)
                        {
                            string result = response.Content.ReadAsStringAsync().Result;
                            viewModel = JsonConvert.DeserializeObject<WorkItemCountResponse.Count>(result);
                            if (viewModel.workItems.Count > 100)
                            {
                                return 101;
                            }
                        }
                        else
                        {
                            var errorMessage = response.Content.ReadAsStringAsync().Result;
                            //string error = Utility.GeterroMessage(errorMessage.Result.ToString());
                            logFileName = logPath + "\\CopyProjectErrors\\GetTotalWorkItemCount_Errors_" + DateTime.Now.ToString("yyyyMMddHHmmss") + ".txt";
                            LogData("Vsts call has been made at:" + DateTime.Now + Environment.NewLine + Environment.NewLine + Environment.NewLine + Environment.NewLine + errorMessage);
                        }
                    }
                    //getting repository count, checking for 1+
                    using (var client1 = new HttpClient())
                    {
                        client1.BaseAddress = new Uri(aUrl);
                        client1.DefaultRequestHeaders.Accept.Clear();
                        client1.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("Application/json"));
                        client1.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", con.accessToken);
                        HttpResponseMessage response1 = client1.GetAsync(aUrl + con.SrcProjectName + "/_apis/git/repositories?api-version=1.0").Result;
                        if (response1.IsSuccessStatusCode)
                        {
                            string res = response1.Content.ReadAsStringAsync().Result;
                            var result = JsonConvert.DeserializeObject<GetRepoCount.Count>(res);
                            if (result.count > 1)
                            {
                                return 101;
                            }
                        }
                        else
                        {
                            var errorMessage = response1.Content.ReadAsStringAsync();
                            string error = Utility.GeterroMessage(errorMessage.Result.ToString());
                            logFileName = logPath + "\\CopyProjectErrors\\GetTotalWorkItemCount_Errors_" + DateTime.Now.ToString("yyyyMMddHHmmss") + ".txt";
                            LogData("Vsts call has been made at:" + DateTime.Now + Environment.NewLine + Environment.NewLine + Environment.NewLine + Environment.NewLine + errorMessage);
                        }

                    }
                }
                return 1;

            }
            catch (Exception ex)
            {
                logFileName = logPath + "\\CopyProjectErrors\\GetTotalWorkItemCount_Errors_" + DateTime.Now.ToString("yyyyMMddHHmmss") + ".txt";
                LogData("Vsts call has been made at:" + DateTime.Now + Environment.NewLine + Environment.NewLine + Environment.NewLine + Environment.NewLine + ex.Message);

            }
            return -1;
        }

        public string SendAdminMail(string mailid, string name)
        {
            SendEmail reqmail = new SendEmail();
            string retmsg = string.Empty;
            bool msg = reqmail.AdminEmail(mailid, name);
            if (msg == true)
            {
                return retmsg = "success";
            }
            return retmsg = "unsuccess";
        }

        public void SendEmailNote(Email model)
        {
            SendEmail visitormail = new SendEmail();
            var bodyContent = System.IO.File.ReadAllText(System.Web.HttpContext.Current.Server.MapPath("~/Views/Account/SiteVisitor.cshtml"));
            string datetime = "Visited time: " + DateTime.Now.ToString();
            bodyContent = bodyContent.Replace("$body$", datetime);
            bodyContent = bodyContent.Replace("$Name$", model.DisplayName);
            bodyContent = bodyContent.Replace("$Email$", model.DisplayEmail);
            bool isMailSent = visitormail.VisitorEmail(bodyContent);
        }
        public class Email
        {
            public string DisplayName { get; set; }
            public string DisplayEmail { get; set; }
        }

        #endregion

        #region LOGdATA
        private void LogData(string message)
        {
            //File.Create(logFileName);
            System.IO.File.AppendAllText(logFileName, message);
        }

        #endregion
    }
}