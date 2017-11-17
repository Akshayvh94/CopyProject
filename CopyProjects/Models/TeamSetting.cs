using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace CopyProjects.Models
{
    public class TeamSetting
    {
        public class BacklogIteration
        {
            public string id { get; set; }
            public string name { get; set; }
            public string path { get; set; }
            public string url { get; set; }
        }

        public class BacklogVisibilities
        {
            [JsonProperty(PropertyName = "Microsoft.EpicCategory")]
            public bool EpicCategory { get; set; }
            [JsonProperty(PropertyName = "Microsoft.FeatureCategory")]
            public bool FeatureCategory { get; set; }
            [JsonProperty(PropertyName = "Microsoft.RequirementCategory")]
            public bool RequirementCategory { get; set; }
        }

        public class DefaultIteration
        {
            public string id { get; set; }
            public string name { get; set; }
            public string path { get; set; }
            public string url { get; set; }
        }
        
        public class Setting
        {
            public BacklogIteration backlogIteration { get; set; }
            public string bugsBehavior { get; set; }
            public IList<string> workingDays { get; set; }
            public BacklogVisibilities backlogVisibilities { get; set; }
            public DefaultIteration defaultIteration { get; set; }
            public string defaultIterationMacro { get; set; }
            public string url { get; set; }
        }


    }
}