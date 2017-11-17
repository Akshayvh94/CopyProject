using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace CopyProjects.Models
{
    public class TeamMembers
    {
        public class Value
        {
            public string id { get; set; }
            public string displayName { get; set; }
            public string uniqueName { get; set; }
            public string url { get; set; }
            public string imageUrl { get; set; }
        }

        public class Members
        {
            public IList<Value> value { get; set; }
            public int count { get; set; }
        }


    }
}