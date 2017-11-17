using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace CopyProjects.Models
{
    public class Utility
    {
        public static string GeterroMessage(string Exception)
        {
            string message = string.Empty;
            try
            {
                JObject jItems = JObject.Parse(Exception);
                message = jItems["message"].ToString();

                return message;
            }
            catch (Exception ex)
            {
                return message;
            }
        }
    }

}