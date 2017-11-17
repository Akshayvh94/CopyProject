using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace CopyProjects.Models.ClassificationNodes
{
    public class GetNodesResponse
    {
        public class Nodes
        {
            public int id { get; set; }
            public string name { get; set; }
            public string structureType { get; set; }
            public bool hasChildren { get; set; }
            public Child[] children { get; set; }
            public _Links _links { get; set; }
            public string url { get; set; }
        }

        public class _Links
        {
            public Self self { get; set; }
        }

        public class Self
        {
            public string href { get; set; }
        }

        public class Child
        {
            public int id { get; set; }
            public string name { get; set; }
            public string structureType { get; set; }
            public bool hasChildren { get; set; }
            public string url { get; set; }
            public Attributes attributes { get; set; }

            public Child[] children { get; set; }
        }

        public class Child1
        {
            public int id { get; set; }
            public string name { get; set; }
            public string structureType { get; set; }
            public bool hasChildren { get; set; }
            public string url { get; set; }
            public Attributes attributes { get; set; }
            public Child2[] children { get; set; }

        }
        public class Child2
        {
            public int id { get; set; }
            public string name { get; set; }
            public string structureType { get; set; }
            public bool hasChildren { get; set; }
            public string url { get; set; }
            public Attributes attributes { get; set; }
            public Child3[] children { get; set; }

        }
        public class Child3
        {
            public int id { get; set; }
            public string name { get; set; }
            public string structureType { get; set; }
            public bool hasChildren { get; set; }
            public string url { get; set; }
            public Attributes attributes { get; set; }
        }
        public class Attributes
        {
            public DateTime startDate { get; set; }
            public DateTime finishDate { get; set; }
        }
    }
}