using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Net;
using System.Net.Mail;
using System.Net.Security;
using System.Security.Cryptography.X509Certificates;
using System.Web;

namespace CopyProjects.Logic
{
    public class SendEmail
    {
        public bool AdminEmail(string reqestMailid, string name)
        {
            MailMessage newmsg = new MailMessage();
            newmsg.From = new MailAddress(ConfigurationManager.AppSettings["from"]);
            string toEmail = "vststoolssupport@ecanarys.com";


            newmsg.IsBodyHtml = true;
            newmsg.Subject = "VSTS Copy Project";
            newmsg.To.Add(toEmail);


            SmtpClient smtp = new SmtpClient();
            smtp.Host = Convert.ToString(ConfigurationManager.AppSettings["mailhost"]);
            smtp.Port = Convert.ToInt16(ConfigurationManager.AppSettings["port"]);

            smtp.UseDefaultCredentials = false;
            smtp.Credentials = new System.Net.NetworkCredential
           (Convert.ToString(ConfigurationManager.AppSettings["username"]), Convert.ToString(ConfigurationManager.AppSettings["password"]));

            smtp.EnableSsl = Convert.ToBoolean(ConfigurationManager.AppSettings["enableSSL"]);

            string newdata = System.IO.File.ReadAllText(System.Web.HttpContext.Current.Server.MapPath("~/Views/Account/AdminMailTemplate.cshtml"));
            newdata = newdata.Replace("$mailme$", reqestMailid).Replace("$mymail$", reqestMailid).Replace("$Name$", name);
            newmsg.Body = newdata;
            try
            {
                ServicePointManager.ServerCertificateValidationCallback = delegate (object s, X509Certificate certificate, X509Chain chain, SslPolicyErrors sslPolicyErrors)
                { return true; };
                smtp.Send(newmsg);
            }
            catch (Exception ex)
            {
                //throw ex;
                return false;
            }
            return true;
        }

        public bool VisitorEmail(string bodyContent)
        {
            MailMessage newmsg = new MailMessage();
            newmsg.From = new MailAddress(ConfigurationManager.AppSettings["from"]);
            string toEmail = "vststoolssupport@ecanarys.com";

            newmsg.IsBodyHtml = true;
            newmsg.Subject = "Canarys site visit notification";
            newmsg.To.Add(toEmail);


            SmtpClient smtp = new SmtpClient();
            smtp.Host = Convert.ToString(ConfigurationManager.AppSettings["mailhost"]);
            smtp.Port = Convert.ToInt16(ConfigurationManager.AppSettings["port"]);

            smtp.UseDefaultCredentials = false;
            smtp.Credentials = new System.Net.NetworkCredential
           (Convert.ToString(ConfigurationManager.AppSettings["username"]), Convert.ToString(ConfigurationManager.AppSettings["password"]));

            smtp.EnableSsl = Convert.ToBoolean(ConfigurationManager.AppSettings["enableSSL"]);

            newmsg.Body = bodyContent;
            try
            {
                ServicePointManager.ServerCertificateValidationCallback = delegate (object s, X509Certificate certificate, X509Chain chain, SslPolicyErrors sslPolicyErrors)
                { return true; };
                smtp.Send(newmsg);
            }
            catch (Exception ex)
            {
                //throw ex;
                return false;
            }
            return true;
        }

    }
}