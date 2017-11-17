using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace CopyProjects.Models.WorkItems
{
    public class WorkItemNameResponse
    {
        public class Icon
        {
            public string id { get; set; }
            public string url { get; set; }
        }

        public class FieldInstance
        {
            public bool alwaysRequired { get; set; }
            public string referenceName { get; set; }
            public string name { get; set; }
            public string url { get; set; }
            public string helpText { get; set; }
        }


        public class Value
        {
            public string name { get; set; }
            public IList<FieldInstance> fieldInstances { get; set; }
            public string url { get; set; }
        }

        public class WorkItem
        {
            public int count { get; set; }
            public IList<Value> value { get; set; }
        }


    }

}