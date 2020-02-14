using Umbraco.Core;
using Umbraco.Core.Composing;
using System.Web.Mvc;
using System.Web.Routing;
using HappyPorch.UmbracoExtensions.Core.Controllers;

namespace HappyPorch.UmbracoExtensions.Core.Components
{
    [RuntimeLevel(MinLevel = RuntimeLevel.Run)]
    public class RoutingComposer : IUserComposer
    {
        public void Compose(Composition composition)
        {
            // Every custom MVC controller needs to be registered.
            composition.Register(typeof(RobotsController), Lifetime.Request);
            composition.Components().Append<RoutingComponent>();
        }
    }

    public class RoutingComponent : IComponent
    {
        public void Initialize()
        {
            // register route for site map XML
            RouteTable.Routes.MapRoute(
                "HappyPorch.UmbracoExtensions.Core.Controllers.SiteMapController",
                "sitemapxml",
                new
                {
                    controller = "SiteMap",
                    action = "Xml",
                    rootNodeId = UrlParameter.Optional
                }
            );

            // register route for robots.txt
            RouteTable.Routes.MapRoute(
                "HappyPorch.UmbracoExtensions.Core.Controllers.RobotsController",
                "robots.txt",
                new
                {
                    controller = "Robots",
                    action = "Txt",
                    rootNodeId = UrlParameter.Optional
                }
            );
        }

        public void Terminate()
        {
        }
    }
}
