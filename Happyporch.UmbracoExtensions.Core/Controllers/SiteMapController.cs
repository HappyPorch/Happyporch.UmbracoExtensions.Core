using HappyPorch.UmbracoExtensions.Core.Services;
using System.Text;
using System.Web.Mvc;
using Umbraco.Core.Models.PublishedContent;
using Umbraco.Web;
using Umbraco.Web.Mvc;

namespace HappyPorch.UmbracoExtensions.Core.Controllers
{
    public class SiteMapController : PluginController
    {
        private readonly IUmbracoContextFactory _contextFactory;
        private readonly IVariationContextAccessor _variantContextAccessor;

        public SiteMapController(IUmbracoContextFactory contextFactory, IVariationContextAccessor variantContextAccessor)
        {
            _contextFactory = contextFactory;
            _variantContextAccessor = variantContextAccessor;
        }

        [HttpGet]
        public ActionResult Xml(int? rootNodeId = null)
        {
            var xml = new SiteMapService(_contextFactory, _variantContextAccessor, Services.LocalizationService, Services.DomainService).GetSiteMapXmlString(rootNodeId);
            return Content(xml, "text/xml", Encoding.UTF8);
        }
    }
}
