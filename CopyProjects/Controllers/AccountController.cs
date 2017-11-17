using CopyProjects.Models;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Web;
using System.Web.Mvc;

namespace CopyProjects.Controllers
{
    public class AccountController : Controller
    {
        AccessDetails AccessDetails = new AccessDetails();

        // GET: Account
        public ActionResult Index()
        {
            return View();
        }

        public ActionResult AdminMailTemplate()
        {
            return View();
        }
        public ActionResult Messageview(SuccErrorMsg retmsg)
        {
            return View(retmsg);
        }
        public ActionResult Test(AccessDetails model)
        {
            return View(model);
        }
        public ActionResult Verify()
        {
            //vsts copy project
            //vso.agentpools_manage vso.build_execute vso.code_full vso.code_status vso.codesearch vso.connected_server vso.dashboards vso.dashboards_manage vso.entitlements vso.extension.data_write vso.extension_manage vso.gallery_acquire vso.gallery_manage vso.identity_manage vso.loadtest_write vso.notification_manage vso.packaging_manage vso.profile_write vso.project_manage vso.release_manage vso.security_manage vso.serviceendpoint_manage vso.taskgroups_manage vso.test_write vso.wiki_write vso.work_full vso.workitemsearch
            //string url = "https://app.vssps.visualstudio.com/oauth2/authorize?client_id={0}&response_type=Assertion&state=User1&scope=vso.agentpools_manage%20vso.build_execute%20vso.code_full%20vso.code_status%20vso.codesearch%20vso.connected_server%20vso.dashboards%20vso.dashboards_manage%20vso.entitlements%20vso.extension.data_write%20vso.extension_manage%20vso.gallery_acquire%20vso.gallery_manage%20vso.identity_manage%20vso.loadtest_write%20vso.notification_manage%20vso.packaging_manage%20vso.profile_write%20vso.project_manage%20vso.release_manage%20vso.security_manage%20vso.serviceendpoint_manage%20vso.taskgroups_manage%20vso.test_write%20vso.wiki_write%20vso.work_full%20vso.workitemsearch&redirect_uri={1}";
            //copyPro
            //vso.agentpools_manage vso.build_execute vso.chat_manage vso.code_full vso.code_status vso.codesearch vso.connected_server vso.dashboards vso.dashboards_manage vso.entitlements vso.extension.data_write vso.extension_manage vso.gallery_acquire vso.gallery_manage vso.identity_manage vso.loadtest_write vso.notification_manage vso.packaging_manage vso.profile_write vso.project_manage vso.release_manage vso.security_manage vso.serviceendpoint_manage vso.taskgroups_manage vso.test_write vso.wiki_write vso.work_full vso.workitemsearch
            string url = "https://app.vssps.visualstudio.com/oauth2/authorize?client_id={0}&response_type=Assertion&state=User1&scope=vso.agentpools_manage%20vso.build_execute%20vso.chat_manage%20vso.code_full%20vso.code_status%20vso.codesearch%20vso.connected_server%20vso.dashboards%20vso.dashboards_manage%20vso.entitlements%20vso.extension.data_write%20vso.extension_manage%20vso.gallery_acquire%20vso.gallery_manage%20vso.identity_manage%20vso.loadtest_write%20vso.notification_manage%20vso.packaging_manage%20vso.profile_write%20vso.project_manage%20vso.release_manage%20vso.security_manage%20vso.serviceendpoint_manage%20vso.taskgroups_manage%20vso.test_write%20vso.wiki_write%20vso.work_full%20vso.workitemsearch&redirect_uri={1}";

            string redirectUrl = System.Configuration.ConfigurationManager.AppSettings["RedirectUri"];
            string clientId = System.Configuration.ConfigurationManager.AppSettings["ClientId"];
            url = string.Format(url, clientId, redirectUrl);


            return Redirect(url);
        }
        public ActionResult Callback(Dashboard model)
        {
            try
            {
                string code = Request.QueryString["code"];

                string redirectUrl = System.Configuration.ConfigurationManager.AppSettings["RedirectUri"];
                string clientId = System.Configuration.ConfigurationManager.AppSettings["ClientSecret"];

                string accessRequestBody = GenerateRequestPostData(clientId, code, redirectUrl);

                //AccessDetails = GetAccessToken(accessRequestBody);

                //return View("../account/test", AccessDetails);

                AccessDetails AccessDetails = new AccessDetails();
                AccessDetails.access_token = "eyJ0eXAiOiJKV1QiLCJhbGciOiJSUzI1NiIsIng1dCI6Im9PdmN6NU1fN3AtSGpJS2xGWHo5M3VfVjBabyJ9.eyJuYW1laWQiOiI5ZjNlMTMyOS0yNzE3LTYxZWMtOTE1Yy04ODdlZDRjY2YxZjEiLCJzY3AiOiJ2c28uYWdlbnRwb29sc19tYW5hZ2UgdnNvLmJ1aWxkX2V4ZWN1dGUgdnNvLmNoYXRfbWFuYWdlIHZzby5jb2RlX2Z1bGwgdnNvLmNvZGVfc3RhdHVzIHZzby5jb2Rlc2VhcmNoIHZzby5jb25uZWN0ZWRfc2VydmVyIHZzby5kYXNoYm9hcmRzIHZzby5kYXNoYm9hcmRzX21hbmFnZSB2c28uZW50aXRsZW1lbnRzIHZzby5leHRlbnNpb24uZGF0YV93cml0ZSB2c28uZXh0ZW5zaW9uX21hbmFnZSB2c28uZ2FsbGVyeV9hY3F1aXJlIHZzby5nYWxsZXJ5X21hbmFnZSB2c28uaWRlbnRpdHlfbWFuYWdlIHZzby5sb2FkdGVzdF93cml0ZSB2c28ubm90aWZpY2F0aW9uX21hbmFnZSB2c28ucGFja2FnaW5nX21hbmFnZSB2c28ucHJvZmlsZV93cml0ZSB2c28ucHJvamVjdF9tYW5hZ2UgdnNvLnJlbGVhc2VfbWFuYWdlIHZzby5zZWN1cml0eV9tYW5hZ2UgdnNvLnNlcnZpY2VlbmRwb2ludF9tYW5hZ2UgdnNvLnRhc2tncm91cHNfbWFuYWdlIHZzby50ZXN0X3dyaXRlIHZzby53aWtpX3dyaXRlIHZzby53b3JrX2Z1bGwgdnNvLndvcmtpdGVtc2VhcmNoIiwiaXNzIjoiYXBwLnZzc3BzLnZpc3VhbHN0dWRpby5jb20iLCJhdWQiOiJhcHAudnNzcHMudmlzdWFsc3R1ZGlvLmNvbSIsIm5iZiI6MTUxMDc1MTE5MCwiZXhwIjoxNTEwNzU0NzkwfQ.D9PD2Lu-WxTAqGrLjHL9WfmRob0NZE3iBTcTlprbTG7X0o3GBpB__Jq0JqvUdTJK7alDVdsBgbNqH53IFBZG_veOME4HSLpfwT4OmZ8xMZb2xxF9-cVMC2WzYM3n2ob6EWkIxLQAwhsfATK_sNBYLJ9iv0QMoS_Zo3x3c6f4xXAKOM6l19R0fazzgIFvmDaSeSZ9Ox-JQUvAXAXQX5CQzILqYFcN6mNjexGp9tQTJutJdiXOrJfsl2gJgBuqY2GAuU3VKYZP-01TPfoc_t2AtWu17AAXWszRxFrcKQrzr96MRU2J3Plk_4GRDRjwFPM0itywDZZ1KicY8N3BulUVOQ";
                //AccessDetails.refresh_token = "eyJ0eXAiOiJKV1QiLCJhbGciOiJSUzI1NiIsIng1dCI6Im9PdmN6NU1fN3AtSGpJS2xGWHo5M3VfVjBabyJ9.eyJuYW1laWQiOiI5ZjNlMTMyOS0yNzE3LTYxZWMtOTE1Yy04ODdlZDRjY2YxZjEiLCJhY2kiOiJiZmU5Njk1OC00OTczLTQ0ODQtYTk2My1iYzE1ZTYxM2JkYTgiLCJzY3AiOiJ2c28uYWdlbnRwb29sc19tYW5hZ2UgdnNvLmJ1aWxkX2V4ZWN1dGUgdnNvLmNoYXRfbWFuYWdlIHZzby5jb2RlX2Z1bGwgdnNvLmNvZGVfc3RhdHVzIHZzby5jb2Rlc2VhcmNoIHZzby5jb25uZWN0ZWRfc2VydmVyIHZzby5kYXNoYm9hcmRzIHZzby5kYXNoYm9hcmRzX21hbmFnZSB2c28uZW50aXRsZW1lbnRzIHZzby5leHRlbnNpb24uZGF0YV93cml0ZSB2c28uZXh0ZW5zaW9uX21hbmFnZSB2c28uZ2FsbGVyeV9hY3F1aXJlIHZzby5nYWxsZXJ5X21hbmFnZSB2c28uaWRlbnRpdHlfbWFuYWdlIHZzby5sb2FkdGVzdF93cml0ZSB2c28ubm90aWZpY2F0aW9uX21hbmFnZSB2c28ucGFja2FnaW5nX21hbmFnZSB2c28ucHJvZmlsZV93cml0ZSB2c28ucHJvamVjdF9tYW5hZ2UgdnNvLnJlbGVhc2VfbWFuYWdlIHZzby5zZWN1cml0eV9tYW5hZ2UgdnNvLnNlcnZpY2VlbmRwb2ludF9tYW5hZ2UgdnNvLnRhc2tncm91cHNfbWFuYWdlIHZzby50ZXN0X3dyaXRlIHZzby53aWtpX3dyaXRlIHZzby53b3JrX2Z1bGwgdnNvLndvcmtpdGVtc2VhcmNoIiwiaXNzIjoiYXBwLnZzc3BzLnZpc3VhbHN0dWRpby5jb20iLCJhdWQiOiJhcHAudnNzcHMudmlzdWFsc3R1ZGlvLmNvbSIsIm5iZiI6MTUwOTA4NjkyNywiZXhwIjoxNTQwNjIyOTI3fQ.NiKU5c1LhIZL5nAnyC5y98EEmQjduXLfUoa9_l9A8kElLwBAXStxAa_FK6V64PoXt2nAT50l8gHviYIS-uhDl1mLdixSm4bn4D3dCcPwRmMrDb_FvWBZne_J0aDiwbG4P6-yz6xZK4IJ_DCqotpYaCR0vi8mv4xb7A5uTY3Ygjl2Ivr_G0Jn0Xvjvgi9vusvGKeptBd35uMzOnFybcbB2prM_RZi14BqjanPdTni6WYEJaOd3lcV5Z7HwmwlP3XdgSGOMwqrpsUJhKN68biOjge2sFkrCmOGb24BwvIN2sqN1UbyZzKh8DTISexsGXxeE013NoHuB8R23UY32UAaDw";

                ProfileDetails Profile = GetProfile(AccessDetails);

                Accounts.AccountList accountList = GetAccounts(Profile.id, AccessDetails);

                model.accessToken = AccessDetails.access_token;
                model.refreshToken = AccessDetails.refresh_token;
                model.Email = Profile.emailAddress;
                model.Name = Profile.displayName;
                model.accountsForDdl = new List<string>();


                if (accountList.count > 0)
                {
                    foreach (var account in accountList.value)
                    {
                        model.accountsForDdl.Add(account.accountName);
                    }
                    model.accountsForDdl.Sort();
                    model.hasAccount = true;
                }
                return View(model);
            }
            catch (Exception)
            {
                return View();
            }

        }
        public string GenerateRequestPostData(string appSecret, string authCode, string callbackUrl)
        {
            return String.Format("client_assertion_type=urn:ietf:params:oauth:client-assertion-type:jwt-bearer&client_assertion={0}&grant_type=urn:ietf:params:oauth:grant-type:jwt-bearer&assertion={1}&redirect_uri={2}",
                        HttpUtility.UrlEncode(appSecret),
                        HttpUtility.UrlEncode(authCode),
                        callbackUrl
                 );
        }
        public AccessDetails GetAccessToken(string body)
        {
            var client = new HttpClient();
            client.BaseAddress = new Uri("https://app.vssps.visualstudio.com");

            var request = new HttpRequestMessage(HttpMethod.Post, "/oauth2/token");

            var requestContent = body;
            request.Content = new StringContent(requestContent, Encoding.UTF8, "application/x-www-form-urlencoded");

            var response = client.SendAsync(request).Result;
            if (response.IsSuccessStatusCode)
            {
                string result = response.Content.ReadAsStringAsync().Result;
                AccessDetails details = Newtonsoft.Json.JsonConvert.DeserializeObject<AccessDetails>(result);
                return details;
            }
            return new AccessDetails();
        }

        public ProfileDetails GetProfile(AccessDetails accessDetails)
        {
            ProfileDetails Profile = new ProfileDetails();

            var client = new HttpClient();
            client.BaseAddress = new Uri("https://app.vssps.visualstudio.com");
            var request = new HttpRequestMessage(HttpMethod.Get, "/_apis/profile/profiles/me");

            var requestContent = string.Format(
                "site={0}&api-version={1}", Uri.EscapeDataString("https://app.vssps.visualstudio.com"), Uri.EscapeDataString("1.0"));

            client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
            request.Headers.Add("Authorization", string.Format("Bearer {0}", accessDetails.access_token));
            try
            {
                var response = client.SendAsync(request).Result;
                if (response.StatusCode == HttpStatusCode.NonAuthoritativeInformation)
                {
                    AccessDetails = Refresh_AccessToken(accessDetails.refresh_token);
                    GetProfile(AccessDetails);
                }
                else if (response.IsSuccessStatusCode)
                {
                    string result = response.Content.ReadAsStringAsync().Result;
                    Profile = JsonConvert.DeserializeObject<ProfileDetails>(result);
                }
                else
                {
                    var errorMessage = response.Content.ReadAsStringAsync();
                    Profile = null;
                }
            }
            catch (Exception ex)
            {
                Profile.ErrorMessage = ex.Message;
            }
            return Profile;
        }

        public AccessDetails Refresh_AccessToken(string refreshToken)
        {
            using (var client = new HttpClient())
            {
                string redirectUri = System.Configuration.ConfigurationManager.AppSettings["RedirectUri"];
                string ClientSecret = System.Configuration.ConfigurationManager.AppSettings["ClientSecret"];
                var request = new HttpRequestMessage(HttpMethod.Post, "https://app.vssps.visualstudio.com/oauth2/token");
                var requestContent = string.Format(
                    "client_assertion_type=urn:ietf:params:oauth:client-assertion-type:jwt-bearer&client_assertion={0}&grant_type=refresh_token&assertion={1}&redirect_uri={2}",
                    HttpUtility.UrlEncode(ClientSecret),
                    HttpUtility.UrlEncode(refreshToken), redirectUri
                    );

                request.Content = new StringContent(requestContent, Encoding.UTF8, "application/x-www-form-urlencoded");
                try
                {
                    var response = client.SendAsync(request).Result;
                    if (response.IsSuccessStatusCode)
                    {
                        string result = response.Content.ReadAsStringAsync().Result;
                        AccessDetails accesDetails = JsonConvert.DeserializeObject<AccessDetails>(result);
                        return accesDetails;
                    }
                    else
                    {
                        return null;
                    }
                }
                catch (Exception ex)
                {
                    return null;
                }
            }
        }
        public Accounts.AccountList GetAccounts(string MemberID, AccessDetails Details)
        {
            Accounts.AccountList Accounts = new Accounts.AccountList();
            var client = new HttpClient();
            string requestContent = "https://app.vssps.visualstudio.com/_apis/Accounts?memberId=" + MemberID + "&api-version=3.2-preview";
            var request = new HttpRequestMessage(HttpMethod.Get, requestContent);
            request.Headers.Add("Authorization", "Bearer " + Details.access_token);
            try
            {
                var response = client.SendAsync(request).Result;
                if (response.StatusCode == HttpStatusCode.NonAuthoritativeInformation)
                {
                    Details = Refresh_AccessToken(Details.refresh_token);
                    return GetAccounts(MemberID, Details);
                }
                else if (response.IsSuccessStatusCode)
                {
                    string result = response.Content.ReadAsStringAsync().Result;
                    Accounts = JsonConvert.DeserializeObject<Accounts.AccountList>(result);
                }
                else
                {
                    var errorMessage = response.Content.ReadAsStringAsync();
                    Accounts = null;
                }
            }
            catch (Exception ex)
            {
                return Accounts;
            }
            return Accounts;
        }

        public ActionResult Logout()
        {
            string clientId = System.Configuration.ConfigurationManager.AppSettings["ClientId"];

            string logout = "https://login.live.com/oauth20_logout.srf?client_id={0}&redirect_uri=https://vstscopyproject.ecanarys.com/";
            logout = string.Format(logout, clientId);
            return Redirect("https://copypro.azurewebsites.net");
            //return Redirect("https://vstscopyproject.ecanarys.com");

        }
    }
}