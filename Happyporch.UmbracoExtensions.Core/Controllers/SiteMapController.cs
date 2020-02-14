using System.Text;
using System.Web.Mvc;
using Umbraco.Web.Mvc;
using HappyPorch.UmbracoExtensions.Core.Services;

namespace HappyPorch.UmbracoExtensions.Core.Controllers
{
    public class SiteMapController : PluginController
    {
        [HttpGet]
        public ActionResult Xml(int? rootNodeId = null)
        {
            var xml = new SiteMapService(UmbracoContext, Logger).GetSiteMapXmlString(rootNodeId);
            return Content(xml, "text/xml", Encoding.UTF8);
        }
    }
}
