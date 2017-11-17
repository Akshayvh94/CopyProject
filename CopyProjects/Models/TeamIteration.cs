using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace CopyProjects.Models
{
    public class TeamIteration
    {
        public class Attributes
        {
            public DateTime startDate { get; set; }
            public DateTime finishDate { get; set; }
        }

        public class Value
        {
            public string id { get; set; }
            public string name { get; set; }
            public string path { get; set; }
            public Attributes attributes { get; set; }
            public string url { get; set; }
        }

        public class Iteration
        {
            public int count { get; set; }
            public IList<Value> value { get; set; }
        }


    }
}