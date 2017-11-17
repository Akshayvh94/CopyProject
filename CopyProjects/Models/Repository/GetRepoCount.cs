using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace CopyProjects.Models.Repository
{
    public class GetRepoCount
    {
        public class Count
        {
            public int count { get; set; }
            public IList<Value> value { get; set; }
        }


    }
}