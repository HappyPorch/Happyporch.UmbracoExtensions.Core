using Umbraco.Core.Logging;
using System.Linq;
using System.Threading.Tasks;
using Umbraco.Core;
using Umbraco.Core.Composing;
using Umbraco.Core.Services.Implement;
using Umbraco.Core.Events;
using Umbraco.Web;
using Umbraco.Core.Services;
using System.Text.RegularExpressions;
using HappyPorch.UmbracoExtensions.Core.Services;

namespace HappyPorch.UmbracoExtensions.Core.Components
{
    [RuntimeLevel(MinLevel = RuntimeLevel.Run)]
    public class ErrorPagesComposer : IUserComposer
    {
        public void Compose(Composition composition)
        {
            composition.Components().Append<ErrorPagesComponent>();
        }
    }

    public class ErrorPagesComponent : IComponent
    {
        private readonly IUmbracoContextFactory _contextFactory;
        private readonly IDomainService _domainService;
        private readonly ILogger _logger;

        public ErrorPagesComponent(IUmbracoContextFactory contextFactory, IDomainService domainService, ILogger logger)
        {
            _contextFactory = contextFactory;
            _domainService = domainService;
            _logger = logger;
        }

        public void Initialize()
        {
            ContentService.Saving += CheckForDuplicates;
            ContentService.Published += GenerateErrorPages;
        }

        public void Terminate()
        {
        }

        private void GenerateErrorPages(IContentService sender, ContentPublishedEventArgs e)
        {
            // only when website settings or an error page is published do we want to generate the static version
            var validAliases = new[] { "websiteSettings", "errorPage" };

            if (e.PublishedEntities.All(x => !validAliases.Contains(x.ContentType.Alias)))
            {
                // website settings or error page wasn't published, nothing to update
                return;
            }

            using (var contextReference = _contextFactory.EnsureUmbracoContext())
            {
                var originalRequestUrl = contextReference.UmbracoContext.HttpContext.Request.Url;

                // generate static error pages in the background
                Task.Run(() =>
                {
                    using (var errorService = new ErrorPageService(_contextFactory, _domainService, _logger, originalRequestUrl))
                    {
                        errorService.GenerateStaticErrorPages();
                    }   
                });
                e.Messages.Add(new EventMessage("Generating error page", "The error page(s) are being regenerated, so it might take up to a minute before you see the changes on the website.", EventMessageType.Info));
            }
        }

        private void CheckForDuplicates(IContentService sender, ContentSavingEventArgs e)
        {
            var statusCodeAlias = "statusCode";

            foreach (var content in e.SavedEntities)
            {
                if (content.ContentType.Alias != "errorPage")
                {
                    // not an error page
                    continue;
                }

                var statusCode = content.GetValue<string>(statusCodeAlias);
                statusCode = Regex.Match(statusCode, @"\d+").Value;
                var errorPageService = new ErrorPageService(_contextFactory, _domainService, _logger);

                if (errorPageService.HasDuplicateErrorPage(statusCode, content.Id, content.ParentId))
                {
                    _logger.Info(GetType(), $"Saving of this item ({content.Name}) has been blocked.");
                    e.Cancel = true;
                    e.Messages.Add(new EventMessage("Saving of this item has been blocked", $"There is already an error page for status code {statusCode}.<br /> Please choose a different status code or edit the existing page instead.", EventMessageType.Warning));
                }

            }
        }
    }
}
