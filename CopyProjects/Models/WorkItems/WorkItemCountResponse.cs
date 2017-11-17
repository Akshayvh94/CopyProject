using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace CopyProjects.Models.WorkItems
{
    public class WorkItemCountResponse
    {
        public class Column
        {
            public string referenceName { get; set; }
            public string name { get; set; }
            public string url { get; set; }
        }

        public class WorkItem
        {
            public int id { get; set; }
            public string url { get; set; }
        }

        public class Count
        {
            public string queryType { get; set; }
            public string queryResultType { get; set; }
            public DateTime asOf { get; set; }
            public IList<Column> columns { get; set; }
            public IList<WorkItem> workItems { get; set; }
        }


    }
}