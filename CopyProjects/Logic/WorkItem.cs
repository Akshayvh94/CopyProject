using CopyProjects.Models;
using CopyProjects.Models.WorkItems;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Web;
using System.Web.Hosting;

namespace CopyProjects.Logic
{
    public class WorkItem
    {
        public string boardRowFieldName;
        public string lastFailureMessage;

        public string logFileName;
        public string logPath;

        List<WIMapData> WIData = new List<WIMapData>();
        List<string> listAssignToUsers = new List<string>();
        string[] relTypes = { "Microsoft.VSTS.Common.TestedBy-Reverse", "System.LinkTypes.Hierarchy-Forward", "System.LinkTypes.Related" };
        string attachmentFolder = string.Empty;
        string repositoryId = string.Empty;
        string projectId = string.Empty;
        Dictionary<string, string> pullRequests = new Dictionary<string, string>();

        SuccErrorMsg sucerrmsg = new SuccErrorMsg();
        Config conf = new Config();
        AccessDetails accessDetails = new AccessDetails();
        RefreshToken refreshToken = new RefreshToken();
        public SuccErrorMsg GetWorkItemDetails(Config confi)
        {
            try
            {
                logPath = HostingEnvironment.MapPath("~/ApiLog");

                conf = confi;
                WorkItemNameResponse.WorkItem WITname = GetWorkItemNames(confi);
                List<WorkItemFetchResponse.WorkItems> fetched = new List<WorkItemFetchResponse.WorkItems>();
                WorkItemFetchResponse.WorkItems fetchedTasks = new WorkItemFetchResponse.WorkItems();
                List<WorkItemFetchResponse.WorkItems> fetchedTasks2 = new List<WorkItemFetchResponse.WorkItems>();

                foreach (var witname in WITname.value)
                {
                    fetchedTasks2 = getWorkItemsfromSource(witname.name);
                    if (fetchedTasks2.Count > 0)
                    {
                        foreach (var f in fetchedTasks2)
                        {
                            if (f.value != null)
                            {
                                fetched.Add(f);
                            }
                        }
                    }

                }

                if (fetched.Count > 0)
                {
                    //string newWitString = "";
                    foreach (var w in fetched)
                    {
                        string witstring = "";
                        witstring += JsonConvert.SerializeObject(w, Formatting.Indented);

                        //this function call was after PrepareAndUpdateTarget() method.
                        UpdateWorkItemLinks(witstring);
                    }
                }

                return sucerrmsg;
            }
            catch (Exception ex)
            {
                logFileName = logPath + "\\CopyProjectErrors\\GetWorkItemDetails_Errors_" + DateTime.Now.ToString("yyyyMMddHHmmss") + ".txt";
                LogData("Vsts call has been made at:" + DateTime.Now + Environment.NewLine + Environment.NewLine + Environment.NewLine + Environment.NewLine + ex.Message);

                sucerrmsg.errormsg.Add("Error in fetching work item(s)" + Environment.NewLine + ex.Message);
            }
            return sucerrmsg;
        }

        public List<WorkItemFetchResponse.WorkItems> getWorkItemsfromSource(string workItemType)
        {
            try
            {
                logPath = HostingEnvironment.MapPath("~/ApiLog");

                GetWorkItemsResponse.Results viewModel = new GetWorkItemsResponse.Results();
                List<WorkItemFetchResponse.WorkItems> fetchedWIs = new List<WorkItemFetchResponse.WorkItems>();
                // create wiql object
                Object wiql = new
                {
                    query = "Select [State], [Title] ,[Effort]" +
                            "From WorkItems " +
                            "Where [Work Item Type] = '" + workItemType + "'" +
                            "And [System.TeamProject] = '" + conf.project + "' " +
                            "Order By [State] Asc, [Changed Date] Desc"
                };
                using (var client = new HttpClient())
                {
                    client.DefaultRequestHeaders.Accept.Clear();
                    client.DefaultRequestHeaders.Accept.Add(new System.Net.Http.Headers.MediaTypeWithQualityHeaderValue("application/json"));
                    client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", conf._credentials);

                    var postValue = new StringContent(JsonConvert.SerializeObject(wiql), Encoding.UTF8, "application/json"); // mediaType needs to be application/json-patch+json for a patch call

                    // set the httpmethod to Patch
                    var method = new HttpMethod("POST");

                    // send the request               
                    var request = new HttpRequestMessage(method, conf.srcURL + "_apis/wit/wiql?api-version=2.2") { Content = postValue };
                    var response = client.SendAsync(request).Result;

                    if (response.StatusCode == HttpStatusCode.NonAuthoritativeInformation)
                    {
                        accessDetails = refreshToken.Refresh_AccessToken(conf.refresh_token);
                        conf.refresh_token = accessDetails.refresh_token;
                        conf._credentials = accessDetails.access_token;
                        getWorkItemsfromSource(workItemType);
                    }
                    else if (response.IsSuccessStatusCode)
                    {
                        string result = response.Content.ReadAsStringAsync().Result;
                        viewModel = JsonConvert.DeserializeObject<GetWorkItemsResponse.Results>(result);

                    }
                    else
                    {
                        var errorMessage = response.Content.ReadAsStringAsync();
                        string error = Utility.GeterroMessage(errorMessage.Result.ToString());

                        logFileName = logPath + "\\CopyProjectErrors\\getWorkItemsfromSource_Errors_" + DateTime.Now.ToString("yyyyMMddHHmmss") + ".txt";
                        LogData("Vsts call has been made at:" + DateTime.Now + Environment.NewLine + Environment.NewLine + Environment.NewLine + Environment.NewLine + error);

                        sucerrmsg.errormsg.Add("Error in fetching work item(s) by query " + workItemType + Environment.NewLine + error);

                    }
                    List<string> WitIDs = new List<string>();
                    string[] splitWITs = new string[] { };

                    string workitemIDstoFetch = ""; int WICtr = 0;
                    foreach (GetWorkItemsResponse.Workitem WI in viewModel.workItems)
                    {
                        workitemIDstoFetch = WI.id + "," + workitemIDstoFetch;
                        WICtr++;
                    }
                    workitemIDstoFetch = workitemIDstoFetch.TrimEnd(',');
                    string newWITidstring = "";

                    int splitWitCount = 0;
                    if (WICtr > 0)
                    {
                        if (WICtr >= 500)
                        {
                            splitWITs = workitemIDstoFetch.Split(',');
                            int i = 0, j = 0;
                            for (i = j; i < splitWITs.Length; i++)
                            {
                                newWITidstring += splitWITs[i] + ",";
                                splitWitCount++;
                                if (splitWitCount == 500)
                                {
                                    newWITidstring = newWITidstring.TrimEnd(',');
                                    WitIDs.Add(newWITidstring);
                                    splitWitCount = 0;
                                    newWITidstring = "";
                                }
                            }
                            if (i == WICtr)
                            {
                                newWITidstring = newWITidstring.TrimEnd(',');
                                WitIDs.Add(newWITidstring);
                                splitWitCount = 0;
                                newWITidstring = "";
                            }
                        }
                        else
                        {
                            WitIDs.Add(workitemIDstoFetch);
                        }
                    }
                    fetchedWIs = GetWorkItemsDetailinBatch(WitIDs);
                    if (fetchedWIs.Count > 0)
                    {
                        foreach (var fetchedWI in fetchedWIs)
                        {
                            string fetchedPBIsJSON = JsonConvert.SerializeObject(fetchedWI, Formatting.Indented);

                            string filPath = AppDomain.CurrentDomain.BaseDirectory; filPath += "Templates\\WorkItemAttachments";

                            ImportWorkitems(fetchedPBIsJSON, workItemType, "", filPath, "", conf.targetProjectID);
                        }
                    }
                }

                return fetchedWIs;
            }
            catch (Exception ex)
            {
                logFileName = logPath + "\\CopyProjectErrors\\getWorkItemsfromSource_Errors_" + DateTime.Now.ToString("yyyyMMddHHmmss") + ".txt";
                LogData("Vsts call has been made at:" + DateTime.Now + Environment.NewLine + Environment.NewLine + Environment.NewLine + Environment.NewLine + ex.Message);
                sucerrmsg.errormsg.Add("Exception: " + workItemType + Environment.NewLine + ex.Message);
            }
            return new List<WorkItemFetchResponse.WorkItems>();
        }

        public List<WorkItemFetchResponse.WorkItems> GetWorkItemsDetailinBatch(List<string> workitemstoFetchList)
        {
            try
            {
                logPath = HostingEnvironment.MapPath("~/ApiLog");
                if (workitemstoFetchList.Count > 0)
                {
                    List<WorkItemFetchResponse.WorkItems> viewmodelList = new List<WorkItemFetchResponse.WorkItems>();

                    foreach (var workitemstoFetch in workitemstoFetchList)
                    {
                        WorkItemFetchResponse.WorkItems viewModel = new WorkItemFetchResponse.WorkItems();

                        using (var client = new HttpClient())
                        {
                            client.BaseAddress = new Uri(conf.srcURL);
                            client.DefaultRequestHeaders.Accept.Clear();
                            client.DefaultRequestHeaders.Accept.Add(new System.Net.Http.Headers.MediaTypeWithQualityHeaderValue("application/json"));
                            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", conf._credentials);

                            HttpResponseMessage response = client.GetAsync(conf.srcURL + "_apis/wit/workitems?api-version=2.2&ids=" + workitemstoFetch + "&$expand=relations").Result;
                            if (response.StatusCode == HttpStatusCode.NonAuthoritativeInformation)
                            {
                                accessDetails = refreshToken.Refresh_AccessToken(conf.refresh_token);
                                conf.refresh_token = accessDetails.refresh_token;
                                conf._credentials = accessDetails.access_token;
                                GetWorkItemsDetailinBatch(workitemstoFetchList);
                            }
                            else if (response.IsSuccessStatusCode)
                            {
                                string result = response.Content.ReadAsStringAsync().Result;
                                viewModel = JsonConvert.DeserializeObject<WorkItemFetchResponse.WorkItems>(result);
                               // CreateWorkItemUsingByPassRules(conf.targetProject, result, conf);
                            }
                            //this line was commented-attachments
                            //if (viewModel.count > 0) { DownloadAttachedFiles(viewModel, conf); }
                            //
                            else
                            {
                                var errorMessage = response.Content.ReadAsStringAsync();
                                string error = Utility.GeterroMessage(errorMessage.Result.ToString());
                                
                                logFileName = logPath + "\\CopyProjectErrors\\GetWorkItemsDetailinBatch_Errors_" + DateTime.Now.ToString("yyyyMMddHHmmss") + ".txt";
                                LogData("Vsts call has been made at:" + DateTime.Now + Environment.NewLine + Environment.NewLine + Environment.NewLine + Environment.NewLine + error);
                            }
                            viewmodelList.Add(viewModel);
                        }
                    }
                    return viewmodelList;
                }
            }
            catch (Exception ex)
            {
                logFileName = logPath + "\\CopyProjectErrors\\GetWorkItemsDetailinBatch_Errors_" + DateTime.Now.ToString("yyyyMMddHHmmss") + ".txt";
                LogData("Vsts call has been made at:" + DateTime.Now + Environment.NewLine + Environment.NewLine + Environment.NewLine + Environment.NewLine + ex.Message);

                sucerrmsg.errormsg.Add("Exception Get WIT Details in batch " + Environment.NewLine + ex.Message);
            }
            return new List<WorkItemFetchResponse.WorkItems>();
        }
        //this method was commented
        //public void DownloadAttachedFiles(WorkItemFetchResponse.WorkItems workItems, Config con)
        //{
        //    try
        //    {
        //        if (!Directory.Exists(@"Templates\WorkItemAttachments\"))
        //        {
        //            Directory.CreateDirectory(@"Templates\WorkItemAttachments\");
        //        }
        //        foreach (var wi in workItems.value)
        //        {
        //            if (wi.relations != null)
        //            {
        //                foreach (var rel in wi.relations)
        //                {
        //                    if (rel.rel == "AttachedFile")
        //                    {
        //                        string remoteUri = rel.url;
        //                        string fileName = rel.attributes["id"] + rel.attributes["name"];
        //                        string pathToSave = HostingEnvironment.MapPath("~/Templates/WorkItemAttachments/" + fileName);

        //                        using (var client = new HttpClient())
        //                        {
        //                            client.DefaultRequestHeaders.Accept.Clear();
        //                            client.DefaultRequestHeaders.Accept.Add(new System.Net.Http.Headers.MediaTypeWithQualityHeaderValue("application/json"));
        //                            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", con._credentials);
        //                            HttpResponseMessage response = client.GetAsync(remoteUri + "?api-version=1.0").Result;

        //                            if (response.IsSuccessStatusCode)
        //                            {
        ////System.IO.File.WriteAllText//
        //                              File.WriteAllBytes(pathToSave, response.Content.ReadAsByteArrayAsync().Result);
        //                            }
        //                            else
        //                            {
        //                                var errorMessage = response.Content.ReadAsStringAsync();
        //                                string error = Utility.GeterroMessage(errorMessage.Result.ToString());
        //                                sucerrmsg.errormsg.Add("API fail for download attachment " + Environment.NewLine + error);
        //                            }
        //                        }
        //                    }
        //                }
        //            }
        //        }
        //    }
        //    catch (Exception ex)
        //    {
        //        sucerrmsg.errormsg.Add("Exception Downloading Attachments and storing " + Environment.NewLine + ex.Message);
        //    }
        //}

        public List<WIMapData> ImportWorkitems(string WorkItemsJson, string WitType, string uniqueUser, string attachmentFolderPath, string repositoryID, string projectID)
        {
            try
            {
                logPath = HostingEnvironment.MapPath("~/ApiLog");

                uniqueUser = "romsak@outlook.com";
                attachmentFolder = attachmentFolderPath;
                repositoryId = repositoryID;
                projectId = projectID;
                //  pullRequests = dictPullRequests;
                JArray UserList = new JArray();


                var jitems = JObject.Parse(conf.projectSettingJson);
                UserList = jitems["users"].Value<JArray>();

                if (UserList.Count > 0)
                {
                    listAssignToUsers.Add(uniqueUser);
                }
                foreach (var data in UserList.Values())
                {
                    listAssignToUsers.Add(data.ToString());
                }
                PrepareAndUpdateTarget(WitType, WorkItemsJson, conf.targetProject);

                return WIData;
            }
            catch (Exception ex)
            {
                logFileName = logPath + "\\CopyProjectErrors\\ImportWorkitems_Errors_" + DateTime.Now.ToString("yyyyMMddHHmmss") + ".txt";
                LogData("Vsts call has been made at:" + DateTime.Now + Environment.NewLine + Environment.NewLine + Environment.NewLine + Environment.NewLine + ex.Message);

                sucerrmsg.errormsg.Add("unable to Import work item(s)" + Environment.NewLine + ex.Message);
            }
            return new List<WIMapData>();
        }

        public bool PrepareAndUpdateTarget(string workItemType, string workImport, string ProjectName)
        {
            try
            {
                logPath = HostingEnvironment.MapPath("~/ApiLog");

                ImportWorkItemModel.WorkItems fetchedWIs = JsonConvert.DeserializeObject<ImportWorkItemModel.WorkItems>(workImport);

                if (fetchedWIs.count > 0)
                {
                    foreach (ImportWorkItemModel.Value newWI in fetchedWIs.value)
                    {
                        newWI.fields.SystemCreatedDate = DateTime.Now.AddDays(-3);
                        Dictionary<string, object> dicWIFields = new Dictionary<string, object>();
                        string assignToUser = string.Empty;
                        if (listAssignToUsers.Count > 0)
                        {
                            assignToUser = listAssignToUsers[new Random().Next(0, listAssignToUsers.Count)];
                        }


                        //Test cases have different fields compared to other items like bug, Epics, etc.                     
                        if ((workItemType == "Test Case"))
                        {

                            string areaPath = ProjectName;

                            if (newWI.fields.SystemAreaPath.Contains("\\"))
                            {
                                //areaPath = "";
                                string[] areapathlist = new string[] { };
                                areapathlist = newWI.fields.SystemAreaPath.Split('\\');
                                areaPath = ProjectName;
                                for (int i = 1; i < areapathlist.Length; i++)
                                {
                                    areaPath += '\\' + areapathlist[i];
                                }
                            }
                            //replacing null values with Empty strngs; creation fails if the fields are null
                            if (newWI.fields.MicrosoftVSTSTCMParameters == null) newWI.fields.MicrosoftVSTSTCMParameters = string.Empty;
                            if (newWI.fields.MicrosoftVSTSTCMSteps == null) newWI.fields.MicrosoftVSTSTCMSteps = string.Empty;
                            if (newWI.fields.MicrosoftVSTSTCMLocalDataSource == null) newWI.fields.MicrosoftVSTSTCMLocalDataSource = string.Empty;

                            dicWIFields.Add("/fields/System.Title", newWI.fields.SystemTitle);
                            dicWIFields.Add("/fields/System.State", newWI.fields.SystemState);
                            dicWIFields.Add("/fields/System.Reason", newWI.fields.SystemReason);
                            dicWIFields.Add("/fields/System.AreaPath", areaPath);
                            dicWIFields.Add("/fields/Microsoft.VSTS.Common.Priority", newWI.fields.MicrosoftVSTSCommonPriority);
                            dicWIFields.Add("/fields/Microsoft.VSTS.TCM.Steps", newWI.fields.MicrosoftVSTSTCMSteps);
                            dicWIFields.Add("/fields/Microsoft.VSTS.TCM.Parameters", newWI.fields.MicrosoftVSTSTCMParameters);
                            dicWIFields.Add("/fields/Microsoft.VSTS.TCM.LocalDataSource", newWI.fields.MicrosoftVSTSTCMLocalDataSource);
                            dicWIFields.Add("/fields/Microsoft.VSTS.TCM.AutomationStatus", newWI.fields.MicrosoftVSTSTCMAutomationStatus);

                            if (newWI.fields.SystemTags != null) dicWIFields.Add("/fields/System.Tags", newWI.fields.SystemTags);
                            dicWIFields.Add("/fields/Microsoft.VSTS.Scheduling.RemainingWork", newWI.fields.MicrosoftVSTSSchedulingRemainingWork);

                        }
                        else
                        {
                            string iterationPath = ProjectName;
                            string boardRowField = string.Empty;
                            string areaPath = ProjectName;
                            if (newWI.fields.SystemIterationPath.Contains("\\"))
                            {
                                iterationPath = string.Format(@"{0}\{1}", ProjectName, newWI.fields.SystemIterationPath.Split('\\')[1]);

                            }

                            if (!string.IsNullOrWhiteSpace(boardRowFieldName))
                            {
                                boardRowField = string.Format("/fields/{0}", boardRowFieldName);
                            }

                            if (newWI.fields.SystemAreaPath.Contains("\\"))
                            {
                                //areaPath = "";
                                string[] areapathlist = new string[] { };
                                areapathlist = newWI.fields.SystemAreaPath.Split('\\');
                                areaPath = ProjectName;
                                for (int i = 1; i < areapathlist.Length; i++)
                                {
                                    areaPath += '\\' + areapathlist[i];
                                }
                            }

                            if (newWI.fields.SystemDescription == null) newWI.fields.SystemDescription = newWI.fields.SystemTitle;
                            if (string.IsNullOrEmpty(newWI.fields.SystemBoardLane)) newWI.fields.SystemBoardLane = string.Empty;

                            dicWIFields.Add("/fields/System.Title", newWI.fields.SystemTitle);
                            dicWIFields.Add("/fields/System.Description", newWI.fields.SystemDescription);
                            dicWIFields.Add("/fields/System.State", newWI.fields.SystemState);
                            dicWIFields.Add("/fields/System.AreaPath", areaPath);
                            dicWIFields.Add("/fields/System.Reason", newWI.fields.SystemReason);
                            dicWIFields.Add("/fields/Microsoft.VSTS.Common.Priority", newWI.fields.MicrosoftVSTSCommonPriority);
                            dicWIFields.Add("/fields/System.AssignedTo", assignToUser);
                            dicWIFields.Add("/fields/System.IterationPath", iterationPath);
                            dicWIFields.Add("/fields/Microsoft.VSTS.Scheduling.RemainingWork", newWI.fields.MicrosoftVSTSSchedulingRemainingWork);
                            dicWIFields.Add("/fields/Microsoft.VSTS.Scheduling.Effort", newWI.fields.MicrosoftVSTSSchedulingEffort);


                            if (newWI.fields.SystemTags != null) dicWIFields.Add("/fields/System.Tags", newWI.fields.SystemTags);
                            if (newWI.fields.MicrosoftVSTSTCMParameters != null) dicWIFields.Add("/fields/Microsoft.VSTS.TCM.Parameters", newWI.fields.MicrosoftVSTSTCMParameters);
                            if (newWI.fields.MicrosoftVSTSTCMSteps != null) dicWIFields.Add("/fields/Microsoft.VSTS.TCM.Steps", newWI.fields.MicrosoftVSTSTCMSteps);
                            if (!string.IsNullOrWhiteSpace(boardRowField)) dicWIFields.Add(boardRowField, newWI.fields.SystemBoardLane);
                        }
                        UpdateWorkIteminTarget(workItemType, newWI.id.ToString(), ProjectName, dicWIFields);
                    }

                    return true;
                }
            }
            catch (Exception ex)
            {
                logFileName = logPath + "\\CopyProjectErrors\\PrepareAndUpdateTarget_Errors_" + DateTime.Now.ToString("yyyyMMddHHmmss") + ".txt";
                LogData("Vsts call has been made at:" + DateTime.Now + Environment.NewLine + Environment.NewLine + Environment.NewLine + Environment.NewLine + ex.Message);

                sucerrmsg.errormsg.Add("Unable to update work item(s) in Target" + Environment.NewLine + ex.Message);
            }
            return false;
        }

        public bool UpdateWorkIteminTarget(string workItemType, string old_wi_ID, string ProjectName, Dictionary<string, object> dicWIFields)
        {
            try
            {
                logPath = HostingEnvironment.MapPath("~/ApiLog");
                //int pathCount = paths.Count();
                List<WorkItemPatch.Field> lstFields = new List<WorkItemPatch.Field>();
                WorkItemPatchResponse.WorkItem viewModel = new WorkItemPatchResponse.WorkItem();
                // change some values on a few fields
                foreach (string key in dicWIFields.Keys)
                {
                    lstFields.Add(new WorkItemPatch.Field() { op = "add", path = key, value = dicWIFields[key] });
                }
                WorkItemPatch.Field[] fields = lstFields.ToArray();
                using (var client = new HttpClient())
                {
                    client.DefaultRequestHeaders.Accept.Clear();
                    client.DefaultRequestHeaders.Accept.Add(new System.Net.Http.Headers.MediaTypeWithQualityHeaderValue("application/json"));
                    client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", conf._credentials);

                    //var postValue = new StringContent(JsonConvert.SerializeObject(wI), Encoding.UTF8, "application/json"); // mediaType needs to be application/json-patch+json for a patch call
                    var postValue = new StringContent(JsonConvert.SerializeObject(fields), Encoding.UTF8, "application/json-patch+json"); // mediaType needs to be application/json-patch+json for a patch call
                                                                                                                                          // set the httpmethod to Patch
                    var method = new HttpMethod("PATCH");

                    // send the request               
                    var request = new HttpRequestMessage(method, conf.targetURL + conf.targetProject + "/_apis/wit/workitems/$" + workItemType + "?bypassRules=true&api-version=2.2") { Content = postValue };
                    var response = client.SendAsync(request).Result;

                    if (response.StatusCode == HttpStatusCode.NonAuthoritativeInformation)
                    {
                        accessDetails = refreshToken.Refresh_AccessToken(conf.refresh_token);
                        conf.refresh_token = accessDetails.refresh_token;
                        conf._credentials = accessDetails.access_token;
                        UpdateWorkIteminTarget(workItemType, old_wi_ID, ProjectName, dicWIFields);
                    }
                    else if (response.IsSuccessStatusCode)
                    {
                        string result = response.Content.ReadAsStringAsync().Result;
                        viewModel = JsonConvert.DeserializeObject<WorkItemPatchResponse.WorkItem>(result);
                        WIData.Add(new WIMapData() { oldID = old_wi_ID, newID = viewModel.id.ToString(), WIType = workItemType });
                    }
                    else
                    {
                        var errorMessage = response.Content.ReadAsStringAsync();
                        string error = Utility.GeterroMessage(errorMessage.Result.ToString());

                        logFileName = logPath + "\\CopyProjectErrors\\UpdateWorkIteminTarget_Errors_" + DateTime.Now.ToString("yyyyMMddHHmmss") + ".txt";
                        LogData("Vsts call has been made at:" + DateTime.Now + Environment.NewLine + Environment.NewLine + Environment.NewLine + Environment.NewLine + error);

                        sucerrmsg.errormsg.Add(" Error in Update Work Item in Target" + Environment.NewLine + error);
                        this.lastFailureMessage = error;
                    }

                    return response.IsSuccessStatusCode;

                }
            }
            catch (Exception ex)
            {
                logFileName = logPath + "\\CopyProjectErrors\\UpdateWorkIteminTarget_Errors_" + DateTime.Now.ToString("yyyyMMddHHmmss") + ".txt";
                LogData("Vsts call has been made at:" + DateTime.Now + Environment.NewLine + Environment.NewLine + Environment.NewLine + Environment.NewLine + ex.Message);

                sucerrmsg.errormsg.Add("Unable to update work item type" + workItemType + " in Target" + Environment.NewLine + ex.Message);
            }
            return false;
        }

        public bool UpdateWorkItemLinks(string workItemTemplateJson)
        {
            try
            {
                logPath = HostingEnvironment.MapPath("~/ApiLog");
                ImportWorkItemModel.WorkItems fetchedPBIs = JsonConvert.DeserializeObject<ImportWorkItemModel.WorkItems>(workItemTemplateJson);
                //ImportWorkItemModel.WorkItems fetchedPBIs
                string WIToUpdate = "";
                WIMapData findIDforUpdate;
                if (fetchedPBIs.count > 0)
                {

                    foreach (ImportWorkItemModel.Value newWI in fetchedPBIs.value)
                    {
                        //continue next iteration if there is no relation
                        if (newWI.relations == null) continue;
                        int relCount = newWI.relations.Length;
                        string oldWIID = newWI.id.ToString();

                        findIDforUpdate = WIData.Find(t => t.oldID == oldWIID);
                        if (findIDforUpdate != null)
                        {
                            WIToUpdate = findIDforUpdate.newID;
                            foreach (ImportWorkItemModel.Relations rel in newWI.relations)
                            {
                                if (relTypes.Contains(rel.rel.Trim()))
                                {
                                    oldWIID = rel.url.Substring(rel.url.LastIndexOf("/") + 1);
                                    WIMapData findIDforlink = WIData.Find(t => t.oldID == oldWIID);

                                    if (findIDforlink != null)
                                    {
                                        string newWIID = findIDforlink.newID;
                                        Object[] patchWorkItem = new Object[1];
                                        // change some values on a few fields
                                        patchWorkItem[0] = new
                                        {
                                            op = "add",
                                            path = "/relations/-",
                                            value = new
                                            {
                                                rel = rel.rel,
                                                url = conf.targetURL + "_apis/wit/workitems/" + newWIID,
                                                attributes = new
                                                {
                                                    comment = "Making a new link for the dependency"
                                                }
                                            }
                                        };
                                        //UpdateWorkIteminTarget("Product Backlog Item", newWI.id.ToString(), new String[] { "/relations/-" }, new Object[] { newWI.fields.SystemTitle, newWI.fields.SystemDescription });
                                        if (UpdateLink("Product Backlog Item", WIToUpdate, patchWorkItem))
                                        {
                                            //Console.WriteLine("Updated WI with link from {0} to {1}", oldWIID, newWIID);
                                        }
                                    }
                                }
                                if (rel.rel == "Hyperlink")
                                {
                                    Object[] patchWorkItem = new Object[1];
                                    patchWorkItem[0] = new
                                    {
                                        op = "add",
                                        path = "/relations/-",
                                        value = new
                                        {
                                            rel = "Hyperlink",
                                            url = rel.url
                                        }
                                    };
                                    bool isHyperLinkCreated = UpdateLink(string.Empty, WIToUpdate, patchWorkItem);
                                }

                                //this was commented
                                //if (rel.rel == "AttachedFile")
                                //{
                                //    Object[] patchWorkItem = new Object[1];
                                //    string filPath = string.Format(attachmentFolder + @"\{0}{1}", rel.attributes["id"], rel.attributes["name"]);
                                //    string fileName = rel.attributes["name"];
                                //    string attchmentURl = uploadAttchment(filPath, fileName);
                                //    if (!string.IsNullOrEmpty(attchmentURl))
                                //    {
                                //        patchWorkItem[0] = new
                                //        {
                                //            op = "add",
                                //            path = "/relations/-",
                                //            value = new
                                //            {
                                //                rel = "AttachedFile",
                                //                url = attchmentURl
                                //            }
                                //        };
                                //        bool isAttachmemntCreated = UpdateLink(string.Empty, WIToUpdate, patchWorkItem);
                                //    }
                                //}

                                //
                                if (rel.rel == "ArtifactLink")
                                {
                                    rel.url = rel.url.Replace("$projectId$", projectId).Replace("$RepositoryId$", repositoryId);
                                    foreach (var pullReqest in pullRequests)
                                    {
                                        string key = string.Format("${0}$", pullReqest.Key);
                                        rel.url = rel.url.Replace(key, pullReqest.Value);
                                    }
                                    Object[] patchWorkItem = new Object[1];
                                    patchWorkItem[0] = new
                                    {
                                        op = "add",
                                        path = "/relations/-",
                                        value = new
                                        {
                                            rel = "ArtifactLink",
                                            url = rel.url,
                                            attributes = new
                                            {
                                                name = rel.attributes["name"]
                                            }
                                        }

                                    };
                                    bool isArtifactLinkCreated = UpdateLink(string.Empty, WIToUpdate, patchWorkItem);
                                }
                            }
                        }
                    }
                    return true;
                }
            }
            catch (Exception ex)
            {
                logFileName = logPath + "\\CopyProjectErrors\\UpdateWorkItemLinks_Errors_" + DateTime.Now.ToString("yyyyMMddHHmmss") + ".txt";
                LogData("Vsts call has been made at:" + DateTime.Now + Environment.NewLine + Environment.NewLine + Environment.NewLine + Environment.NewLine + ex.Message);

                sucerrmsg.errormsg.Add("Unable to update work item link" + Environment.NewLine + ex.Message);
            }
            return false;

        }
        public bool UpdateLink(string workItemType, string WItoUpdate, object[] patchWorkItem)
        {
            try
            {
                logPath = HostingEnvironment.MapPath("~/ApiLog");
                using (var client = new HttpClient())
                {
                    client.DefaultRequestHeaders.Accept.Clear();
                    client.DefaultRequestHeaders.Accept.Add(new System.Net.Http.Headers.MediaTypeWithQualityHeaderValue("application/json"));
                    client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", conf._credentials);

                    // serialize the fields array into a json string          
                    var patchValue = new StringContent(JsonConvert.SerializeObject(patchWorkItem), Encoding.UTF8, "application/json-patch+json"); // mediaType needs to be application/json-patch+json for a patch call

                    var method = new HttpMethod("PATCH");
                    var request = new HttpRequestMessage(method, conf.targetURL + "_apis/wit/workitems/" + WItoUpdate + "?bypassRules=true&api-version=2.2") { Content = patchValue };
                    var response = client.SendAsync(request).Result;
                    if (response.StatusCode == HttpStatusCode.NonAuthoritativeInformation)
                    {
                        accessDetails = refreshToken.Refresh_AccessToken(conf.refresh_token);
                        conf.refresh_token = accessDetails.refresh_token;
                        conf._credentials = accessDetails.access_token;
                        UpdateLink(workItemType, WItoUpdate, patchWorkItem);
                    }
                    else if (response.IsSuccessStatusCode)
                    {
                        // viewModel = response.Content.ReadAsAsync<WorkItemPatchResponse.WorkItem>().Result;
                    }
                    //viewModel.HttpStatusCode = response.StatusCode;
                    else
                    {
                        var errorMessage = response.Content.ReadAsStringAsync();
                        string error = Utility.GeterroMessage(errorMessage.Result.ToString());
                        logFileName = logPath + "\\CopyProjectErrors\\UpdateLink_Errors_" + DateTime.Now.ToString("yyyyMMddHHmmss") + ".txt";
                        LogData("Vsts call has been made at:" + DateTime.Now + Environment.NewLine + Environment.NewLine + Environment.NewLine + Environment.NewLine + error);

                        sucerrmsg.errormsg.Add(" Error in Update Link" + Environment.NewLine + error);
                    }

                    return response.IsSuccessStatusCode;
                }
            }
            catch (Exception ex)
            {
                logFileName = logPath + "\\CopyProjectErrors\\UpdateLink_Errors_" + DateTime.Now.ToString("yyyyMMddHHmmss") + ".txt";
                LogData("Vsts call has been made at:" + DateTime.Now + Environment.NewLine + Environment.NewLine + Environment.NewLine + Environment.NewLine + ex.Message);

                sucerrmsg.errormsg.Add("Unable to update work item link to work item type" + workItemType + Environment.NewLine + ex.Message);
            }
            return false;
        }

        public WorkItemNameResponse.WorkItem GetWorkItemNames(Config con)
        {
            WorkItemNameResponse.WorkItem work = new WorkItemNameResponse.WorkItem();
            try
            {
                logPath = HostingEnvironment.MapPath("~/ApiLog");
                using (var client = new HttpClient())
                {
                    client.BaseAddress = new Uri(con.srcURL);
                    client.DefaultRequestHeaders.Accept.Clear();
                    client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("appication/json"));
                    client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", con._credentials);

                    HttpResponseMessage response = client.GetAsync(con.srcprojectID + "/_apis/wit/workItemTypes?api-version=2.0").Result;
                    if (response.StatusCode == HttpStatusCode.NonAuthoritativeInformation)
                    {
                        accessDetails = refreshToken.Refresh_AccessToken(conf.refresh_token);
                        conf.refresh_token = accessDetails.refresh_token;
                        conf._credentials = accessDetails.access_token;
                        GetWorkItemNames(conf);
                    }
                    else if (response.IsSuccessStatusCode)
                    {
                        string res = response.Content.ReadAsStringAsync().Result;
                        work = JsonConvert.DeserializeObject<WorkItemNameResponse.WorkItem>(res);
                        return work;
                    }
                    else
                    {
                        var errorMessage = response.Content.ReadAsStringAsync();
                        string error = Utility.GeterroMessage(errorMessage.Result.ToString());
                        logFileName = logPath + "\\CopyProjectErrors\\GetWorkItemNames_Errors_" + DateTime.Now.ToString("yyyyMMddHHmmss") + ".txt";
                        LogData("Vsts call has been made at:" + DateTime.Now + Environment.NewLine + Environment.NewLine + Environment.NewLine + Environment.NewLine + error);

                        sucerrmsg.errormsg.Add(" Error Getting Work Item Names" + Environment.NewLine + error);
                    }
                }
            }
            catch (Exception ex)
            {
                logFileName = logPath + "\\CopyProjectErrors\\GetWorkItemNames_Errors_" + DateTime.Now.ToString("yyyyMMddHHmmss") + ".txt";
                LogData("Vsts call has been made at:" + DateTime.Now + Environment.NewLine + Environment.NewLine + Environment.NewLine + Environment.NewLine + ex.Message);

                sucerrmsg.errormsg.Add("Unable to get work item names" + Environment.NewLine + ex.Message);
            }
            return null;
        }



        private void LogData(string message)
        {
            System.IO.File.AppendAllText(logFileName, message);
        }

        //this method was commented

        //public string uploadAttchment(string filePath, string fileName)
        //{
        //    try
        //    {
        //        string _filePath = filePath;
        //        string _fileName = fileName;

        //        if (File.Exists(filePath))
        //        {
        //            //read file bytes and put into byte array        
        //            Byte[] bytes = File.ReadAllBytes(filePath);

        //            using (var client = new HttpClient())
        //            {
        //                client.BaseAddress = new Uri(conf.targetURL);
        //                client.DefaultRequestHeaders.Accept.Clear();
        //                client.DefaultRequestHeaders.Accept.Add(new System.Net.Http.Headers.MediaTypeWithQualityHeaderValue("application/octet-stream"));
        //                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", conf._credentials);

        //                ByteArrayContent content = new ByteArrayContent(bytes);
        //                content.Headers.ContentType = new MediaTypeHeaderValue("application/octet-stream");
        //                HttpResponseMessage uploadResponse = client.PostAsync("_apis/wit/attachments?fileName=" + _fileName + "&api-version=2.2", content).Result;

        //                if (uploadResponse.IsSuccessStatusCode)
        //                {
        //                    //get the result, we need this to get the url of the attachment
        //                    string attachmentURL = JObject.Parse(uploadResponse.Content.ReadAsStringAsync().Result)["url"].ToString();
        //                    return attachmentURL;
        //                }
        //                else
        //                {
        //                    var errorMessage = uploadResponse.Content.ReadAsStringAsync();
        //                    string error = Utility.GeterroMessage(errorMessage.Result.ToString());
        //                    sucerrmsg.errormsg.Add(" Error in Update Work Item in Target" + Environment.NewLine + error);
        //                }
        //            }
        //        }
        //    }
        //    catch (Exception ex)
        //    {
        //        sucerrmsg.errormsg.Add("Unable to updaload attachment" + Environment.NewLine + ex.Message);
        //    }
        //    return string.Empty;
        //}

    }
    public class WIMapData
    {
        public string oldID { get; set; }
        public string newID { get; set; }
        public string WIType { get; set; }
    }
}

//public bool CreateWorkItemUsingByPassRules(string projectName, string json, Config con)
//{
//    List<BatchRequest> batchRequests = new List<BatchRequest>();
//    BatchRequest batchReq = JsonConvert.DeserializeObject<BatchRequest>(json);

//    batchRequests.Add(batchReq);

//    foreach (BatchRequest batchRequest in batchRequests)
//    {
//        string currURI = batchRequest.uri;
//        batchRequest.uri = '/' + projectName + currURI;

//        JArray newRel = new JArray(2);
//        int i = 0;
//        foreach (object obj in batchRequest.body)
//        {
//            JObject code = JObject.Parse(obj.ToString());
//            i++;

//            //checking if the object has relations key
//            if (code["path"].ToString() == "/relations/-")
//            {
//                JObject hero = (JObject)code["value"];
//                hero["url"] = con.targetURL + code["value"]["url"].ToString();

//                batchRequest.body[i - 1] = code;
//            }
//        }
//    }

//    using (var client = new HttpClient())
//    {
//        client.DefaultRequestHeaders.Accept.Clear();
//        client.DefaultRequestHeaders.Accept.Add(new System.Net.Http.Headers.MediaTypeWithQualityHeaderValue("application/json"));
//        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", con._credentials);

//        var newbatchRequest = new StringContent(JsonConvert.SerializeObject(batchRequests), Encoding.UTF8, "application/json");
//        var method = new HttpMethod("POST");
//        // send the request
//        var request = new HttpRequestMessage(method, con.targetURL + "_apis/wit/$batch?api-version=1.0") { Content = newbatchRequest };
//        var response = client.SendAsync(request).Result;
//        if (response.IsSuccessStatusCode)
//        {
//            return response.IsSuccessStatusCode;
//        }
//        else
//        {
//            var errorMessage = response.Content.ReadAsStringAsync();
//            string error = Utility.GeterroMessage(errorMessage.Result.ToString());
//            this.lastFailureMessage = error;
//        }
//    }

//    return false;
//}
