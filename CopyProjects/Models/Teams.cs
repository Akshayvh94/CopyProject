﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace CopyProjects.Models
{
    public class Teams
    {
        public class Value
        {
            public string id { get; set; }
            public string name { get; set; }
            public string url { get; set; }
            public string description { get; set; }
            public string identityUrl { get; set; }
        }

        public class TeamList
        {
            public IList<Value> value { get; set; }
            public int count { get; set; }
        }

        public class TargetTemasCreated
        {
            public string id { get; set; }
            public string name { get; set; }
            public string url { get; set; }
            public string description { get; set; }
            public string identityUrl { get; set; }
        }


    }
}