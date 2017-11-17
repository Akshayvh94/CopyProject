using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace CopyProjects.Models.BoardColumnMod
{
    public class GetListofBoards
    {
        public class Value
        {
            public string id { get; set; }
            public string url { get; set; }
            public string name { get; set; }
        }

        public class BoardList
        {
            public int count { get; set; }
            public IList<Value> value { get; set; }
        }

    }
}