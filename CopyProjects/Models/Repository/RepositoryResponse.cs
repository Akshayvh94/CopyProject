using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace CopyProjects.Models.Repository
{
    public class RepositoryResponse
    {
        public class Value
        {
            public string id { get; set; }

            public string name { get; set; }
        }

        public class Repository
        {
            public int count { get; set; }
            public IList<Value> value { get; set; }
        }
    }

    public class RepositoryCreated
    {
        public class Project
        {
            public string id { get; set; }
            public string name { get; set; }
            public string description { get; set; }
            public string url { get; set; }
            public string state { get; set; }
            public int revision { get; set; }
            public string visibility { get; set; }
        }

        public class Created
        {
            public string id { get; set; }
            public string name { get; set; }
            public string url { get; set; }
            public Project project { get; set; }
            public string remoteUrl { get; set; }
            public string sshUrl { get; set; }
        }


    }

    public class ServiceEndPointCreated
    {
        public class Project
        {
            public string id { get; set; }
            public string name { get; set; }
            public string description { get; set; }
            public string url { get; set; }
            public string state { get; set; }
            public int revision { get; set; }
            public string visibility { get; set; }
        }

        public class EndPoint
        {
            public string id { get; set; }
            public string name { get; set; }
            public string url { get; set; }
            public Project project { get; set; }
            public string remoteUrl { get; set; }
            public string sshUrl { get; set; }
        }


    }
}