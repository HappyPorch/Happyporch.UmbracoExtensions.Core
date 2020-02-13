using Umbraco.Core;
using Umbraco.Core.Composing;
using Umbraco.Web;
using Umbraco.Web.Dashboards;

namespace HappyPorch.UmbracoExtensions.Core.Components
{
    /// <summary>
    /// Removes the Umbraco dashboard that shows by default.
    /// </summary>
    [RuntimeLevel(MinLevel = RuntimeLevel.Run)]
    public class RemoveDashboardComponent : IUserComposer
    {
        public void Compose(Composition composition)
        {
            composition.Dashboards().Remove<ContentDashboard>();
        }
    }
}
