using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;

namespace CopyProjects.Models
{
    public static class Extension
    {
        public static string ErrorId(this string str)
        {
            str = str + "_Errors";
            return str;
        }
    }
}