using CopyProjects.Models.WorkItems;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Web;

namespace CopyProjects.Logic
{
    public class BoardColCreate
    {
        public bool GetBoardColumnByName(Config con, string teamname)
        {
            try
            {
                using (var client = new HttpClient())
                {
                    client.BaseAddress = new Uri(con.targetURL);
                    client.DefaultRequestHeaders.Accept.Clear();
                    client.DefaultRequestHeaders.Accept.Add(new System.Net.Http.Headers.MediaTypeWithQualityHeaderValue("application/json"));
                    client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", con._credentials);
                    HttpResponseMessage response = client.GetAsync(con.targetProject + teamname + "/_apis/work/boards/" + obj.SelectedID + "/columns?api-version=2.0-preview.1").Result;
                }

            }
            catch (Exception ex)
            {

            }
            return false;

        }
    }
}