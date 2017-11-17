using CopyProjects.Models;
using CopyProjects.Models.BoardColumnMod;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Web;

namespace CopyProjects.Logic
{
    public class BoardColumns
    {
        public string lastFailureMessage;

        public string GetListBoardbyTeam(string URL, string _credentials, string project, string Team)
        {
            using (var client = new HttpClient())
            {
                client.BaseAddress = new Uri(URL);
                client.DefaultRequestHeaders.Clear();
                client.DefaultRequestHeaders.Accept.Add(new System.Net.Http.Headers.MediaTypeWithQualityHeaderValue("application/json"));
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", _credentials);
                HttpResponseMessage response = client.GetAsync(URL + project + "/" + Team + "/_apis/work/boards/?api-version=2.0-preview").Result;
                if (response.IsSuccessStatusCode)
                {
                    GetListofBoards.BoardList boardObj = new GetListofBoards.BoardList();
                    string result = response.Content.ReadAsStringAsync().Result;
                    boardObj = JsonConvert.DeserializeObject<GetListofBoards.BoardList>(result);
                    return boardObj.value.FirstOrDefault().name;
                }
                else
                {
                    var errorMessage = response.Content.ReadAsStringAsync();
                    string error = Utility.GeterroMessage(errorMessage.Result.ToString());
                    this.lastFailureMessage = error;
                    return "-1";
                }
            }
        }

        public GetListofColumns.Columns GetBoardColumbyBoard(string URL, string _credentials, string project, string team, string boardName)
        {

            using (var client = new HttpClient())
            {
                client.BaseAddress = new Uri(URL);
                client.DefaultRequestHeaders.Clear();
                client.DefaultRequestHeaders.Accept.Add(new System.Net.Http.Headers.MediaTypeWithQualityHeaderValue("application/json"));
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", _credentials);
                HttpResponseMessage response = client.GetAsync(URL + project + "/" + team + "/_apis/work/boards/" + boardName + "/columns?api-version=2.0-preview").Result;
                if (response.IsSuccessStatusCode)
                {
                    GetListofColumns.Columns ColObj = new GetListofColumns.Columns();
                    string result = response.Content.ReadAsStringAsync().Result;

                    ColObj = JsonConvert.DeserializeObject<GetListofColumns.Columns>(result);
                    //bool resp = UpdateBoardColumn(URL, _credentials, result, project, team, boardName);
                    return ColObj;
                }
                else
                {
                    var errorMessage = response.Content.ReadAsStringAsync();
                    string error = Utility.GeterroMessage(errorMessage.Result.ToString());
                    this.lastFailureMessage = error;
                    return new GetListofColumns.Columns();
                }
            }
        }

        public bool UpdateBoardColumn(string URL, string _credentials, string jsonString, string project, string team, string boardName)
        {
            using (var client = new HttpClient())
            {

                
                //return true;
                client.BaseAddress = new Uri(URL);
                client.DefaultRequestHeaders.Clear();
                client.DefaultRequestHeaders.Accept.Add(new System.Net.Http.Headers.MediaTypeWithQualityHeaderValue("application/json"));
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", _credentials);
                //PUT https://fabrikam-fiber-inc.visualstudio.com/DefaultCollection/Fabrikam/Fabrikam%20Team/_apis/work/boards/Backlog%20items/columns?api-version=2.0-preview
                //HttpResponseMessage response = client.GetAsync(URL + project + "/" + team + "/_apis/work/boards/" + boardName + "/columns?api-version=2.0-preview").Result;

                var patchValue = new StringContent(JsonConvert.SerializeObject(jsonString), Encoding.UTF8, "application/json"); // mediaType needs to be application/json-patch+json for a patch call
                var method = new HttpMethod("PUT");

                var request = new HttpRequestMessage(method, URL + project + "/" + team + "/_apis/work/boards/" + boardName + "/columns?api-version=2.0-preview") { Content = patchValue };
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
                    return false;
                }
            }
        }
        
        //private bool UpdateBoardColumn(string templatesFolder, Project model, string BoardColumnsJSON, Configuration _defaultConfiguration, string id)
        //{
        //    bool res = false;
        //    try
        //    {
        //        string jsonBoardColumns = string.Format(templatesFolder + @"{0}\{1}", model.SelectedTemplate, BoardColumnsJSON);
        //        if (System.IO.File.Exists(jsonBoardColumns))
        //        {

        //            jsonBoardColumns = model.ReadJsonFile(jsonBoardColumns);
        //            bool BoardColumnResult = objBoard.UpdateBoard(model.ProjectName, jsonBoardColumns);

        //            if (BoardColumnResult)
        //            {
        //                model.Environment.BoardRowFieldName = objBoard.rowFieldName;
        //                res = true;
        //            }

        //        }
        //    }
        //    catch (Exception ex)
        //    {
        //        string error = Utility.GeterroMessage(ex.Message);
        //        this.lastFailureMessage = "Error while updating board column " + ex.Message + ex.StackTrace + Environment.NewLine;
        //    }
        //    return res;
        //}

        //public bool UpdateBoard(string projectName, string fileName)
        //{
        //    string teamName = projectName + " Team";
        //    List<ColumnPost> Columns = JsonConvert.DeserializeObject<List<ColumnPost>>(fileName);

        //    GetBoardColumnResponse.ColumnResponse currColumns = getBoardColumns(projectName, teamName);
        //    if (currColumns.columns == null) return false;

        //    string newColID = "";
        //    string doneColID = "";
        //    foreach (GetBoardColumnResponse.Value col in currColumns.columns)
        //    {
        //        if (col.name == "New")
        //        {
        //            newColID = col.id;
        //        }
        //        else if (col.name == "Done")
        //        {
        //            doneColID = col.id;
        //        }
        //    }
        //    foreach (ColumnPost col in Columns)
        //    {
        //        if (col.name == "New")
        //        {
        //            col.id = newColID;

        //        }
        //        else if (col.name == "Done")
        //        {
        //            col.id = doneColID;
        //        }
        //    }

        //    using (var client = new HttpClient())
        //    {
        //        client.DefaultRequestHeaders.Accept.Clear();
        //        client.DefaultRequestHeaders.Accept.Add(new System.Net.Http.Headers.MediaTypeWithQualityHeaderValue("application/json"));
        //        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Basic", _credentials);

        //        var patchValue = new StringContent(JsonConvert.SerializeObject(Columns), Encoding.UTF8, "application/json"); // mediaType needs to be application/json-patch+json for a patch call
        //        var method = new HttpMethod("PUT");

        //        var request = new HttpRequestMessage(method, _configuration.UriString + "/" + projectName + "/" + teamName + "/_apis/work/boards/Backlog%20items/columns?api-version=" + _configuration.VersionNumber + "-preview") { Content = patchValue };
        //        var response = client.SendAsync(request).Result;
        //        if (response.IsSuccessStatusCode)
        //        {
        //            return true;
        //        }
        //        else
        //        {
        //            var errorMessage = response.Content.ReadAsStringAsync();
        //            string error = Utility.GeterroMessage(errorMessage.Result.ToString());
        //            this.lastFailureMessage = error;
        //            return false;
        //        }
        //    }
        //}

    }
}