using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace CopyProjects.Models.WorkItems
{
    public class Config
    {
        public string guid { get; set; }
        public string guidstring { get; set; }
        public string srcURL { get; set; }
        public string targetURL { get; set; }
        public string _credentials { get; set; }
        public string refresh_token { get; set; }
        public string project { get; set; }
        public string srcprojectID { get; set; }

        public string targetProject { get; set; }
        public string targetProjectID { get; set; }

        public string projectSettingJson { get; set; }

        public string Template { get; set; }
        public string TemplateID { get; set; }

        public string VersionControl { get; set; }
        public string srcAccount { get; set; }
        public string targetAccount { get; set; }

    }
    public enum TemplateType
    {
        Agile,
        Scrum,
        CMMI
    }
}