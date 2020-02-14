using System;
using System.Text;
using System.Web.Mvc;
using Umbraco.Web.Mvc;
using Umbraco.Core.Logging;

namespace HappyPorch.UmbracoExtensions.Core.Controllers
{
    public class RobotsController : Controller
    {
        [HttpGet]
        public ActionResult Txt()
        {
            var sb = new StringBuilder();
            var robotsTxtPath = Server.MapPath("robots.txt");
            var encoding = new UTF8Encoding(false);
            var websiteUrl = Request.Url.Host;
            var isStagingOrLocal = websiteUrl.Contains("80d-stage.com") || websiteUrl.Contains("80d-local.com");

            if (System.IO.File.Exists(robotsTxtPath))
            {
                // use actual robots.txt file
                sb.Append(System.IO.File.ReadAllText(robotsTxtPath, encoding));
            }
            else if (isStagingOrLocal)
            {
                // no robots.txt file found, create one
                sb.AppendLine("User-Agent: *");
                sb.AppendLine("Disallow: /");
            }
            else if (!isStagingOrLocal)
            {
                // no robots.txt file found, create one
                sb.AppendLine("User-Agent: *");
                // add sitemap link
                sb.AppendFormat("Sitemap: {0}/sitemap.xml", Request.Url?.GetLeftPart(UriPartial.Authority));

                sb.AppendLine();
                sb.AppendLine();

                sb.AppendLine("Disallow: /aspnet_client/");
                sb.AppendLine("Disallow: /bin/");
                sb.AppendLine("Disallow: /config/");
                sb.AppendLine("Disallow: /data/");
                sb.AppendLine("Disallow: /macroScripts/");
                sb.AppendLine("Disallow: /umbraco/");
                sb.AppendLine("Disallow: /umbraco_client/");
                sb.AppendLine("Disallow: /usercontrols/");
                sb.AppendLine("Disallow: /xslt/");
            }

            return Content(sb.ToString(), "text/plain", encoding);
        }
    }
}
