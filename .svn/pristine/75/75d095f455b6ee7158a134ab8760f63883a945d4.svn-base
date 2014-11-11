using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;

namespace IPS.Tracker.Web.Utils
{
    public class HtmlUtils
    {
        public static string ParseHash(string content)
        {
            if (String.IsNullOrEmpty(content))
                return "";

            System.Text.RegularExpressions.Regex r = new System.Text.RegularExpressions.Regex(@"#(\d+)\b");
            string endResult = new string(content.ToCharArray());
            List<string> matchedTasks = new List<string>();

            foreach (System.Text.RegularExpressions.Match match in r.Matches(content))
            {
                if (match.Success && match.Groups.Count == 2)
                {
                    string taskNumber = match.Groups[1].Value;

                    if (!matchedTasks.Contains(taskNumber))
                    {
                        UrlHelper helper = new UrlHelper(HttpContext.Current.Request.RequestContext);
                        string url = helper.Action("Details", "Home", new { id = taskNumber });
                        endResult = endResult.Replace(match.Groups[0].Value, "<a href='" + url + "'>#" + taskNumber + "</a>");
                        matchedTasks.Add(taskNumber);
                    }
                }
            }

            endResult = endResult.Replace("\n", "<br/>");
            return endResult;
        }
    }
}