using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace CopyProjects.Models.BoardColumnMod
{
    public class GetListofColumns
    {
        public class StateMappings
        {
            public string ProductBacklogItem { get; set; }
            public string Bug { get; set; }
        }

        public class Value
        {
            public string name { get; set; }
            public int itemLimit { get; set; }
            public StateMappings stateMappings { get; set; }
            public string columnType { get; set; }
            public bool? isSplit { get; set; }
            public string description { get; set; }
        }

        public class Columns
        {           
            public IList<Value> value { get; set; }
        }


    }
}