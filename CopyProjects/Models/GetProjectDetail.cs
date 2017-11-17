using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace CopyProjects.Models
{
    public class GetProjectDetail
    {
        public class ProcessTemplate
        {
            public string templateName { get; set; }
            public string templateTypeId { get; set; }
        }

        public class Versioncontrol
        {
            public string sourceControlType { get; set; }
            public string gitEnabled { get; set; }
            public string tfvcEnabled { get; set; }
        }

        public class Capabilities
        {
            public ProcessTemplate processTemplate { get; set; }
            public Versioncontrol versioncontrol { get; set; }
        }
        public class Project
        {
            public string id { get; set; }
            public string name { get; set; }
            public string state { get; set; }
            public string description { get; set; }
            public Capabilities capabilities { get; set; }
        }

        public class ProjectStaus
        {
            public string id { get; set; }
            public string name { get; set; }
            public string state { get; set; }
            public string description { get; set; }
            public Capabilities capabilities { get; set; }
        }
    }

    public class NewProject
    {
        public class Versioncontrol
        {
            public string sourceControlType { get; set; }
        }

        public class ProcessTemplate
        {
            public string templateTypeId { get; set; }
        }

        public class Capabilities
        {
            public Versioncontrol versioncontrol { get; set; }
            public ProcessTemplate processTemplate { get; set; }
        }

        public class NewPro
        {
            public string name { get; set; }
            public string description { get; set; }
            public Capabilities capabilities { get; set; }
        }


    }
}