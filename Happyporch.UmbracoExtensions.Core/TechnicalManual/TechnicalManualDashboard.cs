using System;
using Umbraco.Core.Composing;
using Umbraco.Core.Dashboards;

namespace HappyPorch.UmbracoExtensions.Core.TechnicalManual
{
    [Weight(100)]
    public class TechnicalManualDashboard : IDashboard
    {
        public string[] Sections => new[] {
            Umbraco.Core.Constants.Applications.Settings
        };

        public IAccessRule[] AccessRules => Array.Empty<IAccessRule>();

        public string Alias => "technicalManualDashboard";

        public string View => "/Umbraco/backoffice/Api/TechnicalManual/Index";
    }
}
