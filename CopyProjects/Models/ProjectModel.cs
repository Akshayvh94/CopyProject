using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace CopyProjects.Models
{
    public class ProjectModel
    {
        public class Value
        {
            public string id { get; set; }
            public string name { get; set; }
            public string description { get; set; }
            public string url { get; set; }
            public string state { get; set; }
        }

        public class ProjectList
        {
            public int count { get; set; }
            public IList<Value> value { get; set; }
        }
    }

}