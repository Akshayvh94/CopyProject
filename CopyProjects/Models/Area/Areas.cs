using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Web;

namespace CopyProjects.Models.Area
{
    public class Areas
    {
        public class Child
        {

            public string name { get; set; }
            public string structureType { get; set; }
            public bool hasChildren { get; set; }
            public IList<Child1> children { get; set; }

            public string url { get; set; }
        }

        public class Child1
        {

            public string name { get; set; }
            public string structureType { get; set; }
            public bool hasChildren { get; set; }
            public IList<Child2> children { get; set; }
            public string url { get; set; }
        }

        public class Child2
        {
            public string name { get; set; }
            public string structureType { get; set; }
            public bool hasChildren { get; set; }
            public IList<Child3> children { get; set; }
            public string url { get; set; }
        }
        public class Child3
        {

            public string name { get; set; }
            public string structureType { get; set; }
            public bool hasChildren { get; set; }
            public IList<Child4> children { get; set; }
            public string url { get; set; }
        }
        public class Child4
        {
            public string name { get; set; }
            public string structureType { get; set; }
            public bool hasChildren { get; set; }
            public IList<Child5> children { get; set; }
            public string url { get; set; }
        }
        public class Child5
        {
            public string name { get; set; }
            public string structureType { get; set; }
            public bool hasChildren { get; set; }
            public IList<Child6> children { get; set; }
            public string url { get; set; }
        }
        public class Child6
        {
            //public int id { get; set; }
            //public string identifier { get; set; }
            public string name { get; set; }
            public string structureType { get; set; }
            public bool hasChildren { get; set; }
            public string url { get; set; }
        }
        public class Self
        {
            public string href { get; set; }
        }

        public class Links
        {
            public Self self { get; set; }
        }

        public class AreaList
        {
            // public int id { get; set; }
            // public string identifier { get; set; }
            //public string name { get; set; }
            public string structureType { get; set; }
            public bool hasChildren { get; set; }
            public IList<Child> children { get; set; }
            public Links _links { get; set; }
            public string url { get; set; }
        }

        public class Configuration : IConfiguration
        {
            public string UriString { get; set; }
            public string PersonalAccessToken { get; set; }
        }
        public interface IConfiguration
        {
            string PersonalAccessToken { get; set; }
            string UriString { get; set; }
        }
        public class GetNodeResponse
        {
            public class Node
            {
                public string name { get; set; }
                public int id { get; set; }
                public string Message { get; set; }
                public object innerException { get; set; }
                public string message { get; set; }
                public string typeName { get; set; }
                public string typeKey { get; set; }
                public int errorCode { get; set; }
                public int eventId { get; set; }
                public HttpStatusCode HttpStatusCode { get; internal set; }
            }
        }

        public class CreateUpdateNodeViewModel
        {
            public class Node
            {
                public int id { get; set; }
                public string name { get; set; }

            }

        }
    }

}