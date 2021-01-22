using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Web.Hosting;
using Umbraco.Core;
using Umbraco.Core.Logging;
using Umbraco.Core.Models.PublishedContent;
using Umbraco.Core.Services;
using Umbraco.Web;

namespace HappyPorch.UmbracoExtensions.Core.Services
{
    public class ErrorPageService : IDisposable
    {
        private const string localDomain = ".80d-local.com";
        private const string stagingDomain = ".80d-stage.com";
        private const string tempLiveDomain = ".80d-live.com";
        private readonly IUmbracoContextFactory _contextFactory;
        private readonly IDomainService _domainService;
        private readonly ILocalizationService _localizationService;
        private readonly ILogger _logger;
        private readonly IVariationContextAccessor _variantContextAccessor;
        private readonly UmbracoContextReference _contextReference;
        private readonly Uri _originalRequest;

        public ErrorPageService(IUmbracoContextFactory contextFactory, IDomainService domainService, ILocalizationService localizationService, ILogger logger, IVariationContextAccessor variationContextAccessor, Uri originalRequest = null)
        {
            _contextFactory = contextFactory;
            _domainService = domainService;
            _localizationService = localizationService;
            _logger = logger;
            _variantContextAccessor = variationContextAccessor;
            _contextReference = _contextFactory.EnsureUmbracoContext();
            _originalRequest = originalRequest ?? _contextReference.UmbracoContext.HttpContext.Request.Url;
        }

        /// <summary>
        /// Ensure that the correct English variant context is being used.
        /// </summary>
        private void EnsureEnglishVariantContext()
        {
            var englishLanguage = _localizationService.GetAllLanguages()?.FirstOrDefault(l => l.IsoCode.InvariantStartsWith("en"));

            if (englishLanguage != null)
            {
                _variantContextAccessor.VariationContext = new VariationContext(englishLanguage.IsoCode);
            }
        }

        /// <summary>
        /// Gets a list of all published error pages.
        /// </summary>
        /// <returns></returns>
        public IEnumerable<IPublishedContent> GetErrorPages()
        {
            EnsureEnglishVariantContext();

            var cache = _contextReference.UmbracoContext.Content;
            var pages = cache.GetByXPath("//errorPage").ToList();
            return pages;
        }

        /// <summary>
        /// Checks if any other error pages exist with the same status code.
        /// </summary>
        /// <param name="statusCode"></param>
        /// <param name="nodeId"></param>
        /// <param name="parentNodeId"></param>
        /// <returns></returns>
        public bool HasDuplicateErrorPage(string statusCode, int nodeId, int parentNodeId)
        {
            var hasDuplicates = true;
            try
            {
                var errorPages = GetErrorPages();
                if (errorPages == null || !errorPages.Any())
                {
                    // no error pages, so no duplicates
                    return false;
                }

                var cache = _contextReference.UmbracoContext.Content;
                var node = cache.GetById(parentNodeId);
                var nodeSiteId = node.AncestorOrSelf(1).Id;

                // check for any other pages with the same status code
                hasDuplicates = errorPages.Any(x => x.Value<string>("StatusCode") == statusCode && x.Id != nodeId && x.AncestorOrSelf(1).Id == nodeSiteId);
            }
            catch (Exception ex)
            {
                _logger.Error(typeof(ErrorPageService), ex, $"Failed to check duplicate error pages.");
            }
            return hasDuplicates;
        }

        /// <summary>
        /// Generates static version of all published error pages.
        /// </summary>
        public void GenerateStaticErrorPages()
        {
            try
            {
                var errorPages = GetErrorPages();
                if (errorPages == null || !errorPages.Any())
                {
                    // nothing to generate
                    return;
                }
                foreach (var errorPage in errorPages)
                {
                    foreach (var culture in errorPage.Cultures)
                    {
                        var output = RenderErrorPage(errorPage, culture.Key);
                        var siteName = errorPage.AncestorOrSelf(1).Name.ToSafeAlias();
                        SaveStaticErrorPage(errorPage.Value<string>("StatusCode"), siteName, culture.Key, output);
                    }
                }

                foreach (var group in errorPages.GroupBy(x => x.Value<string>("StatusCode")))
                {
                    GenerateStaticErrorHandlingPage(group.Key, group.ToList());
                }
            }
            catch (Exception ex)
            {
                _logger.Error(typeof(ErrorPageService), ex, $"Failed to generate error pages.");
            }
        }

        /// <summary>
        /// Renders the error page and returns the HTML output.
        /// </summary>
        /// <param name="page"></param>
        /// <returns></returns>
        private string RenderErrorPage(IPublishedContent page, string culture)
        {
            string output = null;
            //TODO: probably needs some more tweaking
            //var requestOtherUrls = contextReference.UmbracoContext.UrlProvider.GetOtherUrls(page.Id);
            using (var webClient = new WebClient())
            {
                var uriBuilder = new UriBuilder();
                uriBuilder.Scheme = _originalRequest.Scheme;
                uriBuilder.Host = _originalRequest.Host;

                var requestUri = new Uri(uriBuilder.Uri, page.UrlSegment(culture));
                webClient.Encoding = Encoding.UTF8;
                output = webClient.DownloadString(requestUri.AbsoluteUri);
            }

            return output;
        }

        /// <summary>
        /// Save the HTML content into an HTML file.
        /// </summary>
        /// <param name="statusCode"></param>
        /// <param name="siteName"></param>
        /// <param name="html"></param>
        private void SaveStaticErrorPage(string statusCode, string siteName, string culture, string html)
        {
            if (string.IsNullOrEmpty(statusCode) || string.IsNullOrEmpty(html))
            {
                // empty file shouldn't be saved
                return;
            }

            var pagePath = HostingEnvironment.MapPath($"~/{statusCode}-{siteName}.html");

            try
            {
                System.IO.File.WriteAllText(pagePath, html, Encoding.UTF8);
            }
            catch (Exception ex)
            {
                _logger.Error(typeof(ErrorPageService), ex, $"Failed to save error page: {statusCode}");
            }
        }

        /// <summary>
        /// Generates the .aspx page that will handle the rendering of the right error page for each site.
        /// </summary>
        /// <param name="statusCode"></param>
        /// <param name="errorPages"></param>
        private void GenerateStaticErrorHandlingPage(string statusCode, IReadOnlyCollection<IPublishedContent> errorPages)
        {
            if (string.IsNullOrEmpty(statusCode) || errorPages?.Any() != true)
            {
                // empty file shouldn't be saved
                return;
            }

            // generate content for the ASPX page.
            var html = new StringBuilder();

            html.AppendLine("<%@ page trace = \"false\" validateRequest=\"false\" %>");
            html.AppendLine();
            html.AppendLine("<%-- set correct site name based on requested domain and culture --%>");
            html.AppendLine("<% string siteName = \"\"; %>");
            html.AppendLine("<% string hostName = Request.Url.Host; %>");
            html.AppendLine("<% string culture = \"\"; %>");
            html.AppendLine();
            var domains = _domainService.GetAll(false).ToArray();
            var hasNoDomainErrorPages = true;

            foreach (var errorPage in errorPages)
            {
                // get list of domains for this site
                var siteNode = errorPage.AncestorOrSelf(1);
                var siteName = siteNode.Name.ToSafeAlias();

                var siteDomains = domains.Where(d => d.RootContentId.GetValueOrDefault() == siteNode.Id).ToList();

                if (siteDomains.Count == 0)
                {
                    // no domains found to add
                    continue;
                }

                hasNoDomainErrorPages = false;

                html.Append("<% if (");
                html.Append(string.Join(" || ", siteDomains.Select(d =>
                {
                    var hostName = string.Empty;
                    var uri = new Uri(d.DomainName, UriKind.RelativeOrAbsolute);

                    // check if domain is set with scheme or not, and just use the hostname
                    if (uri.IsAbsoluteUri)
                    {
                        hostName = uri.Host;
                    }
                    else
                    {
                        hostName = uri.OriginalString;
                    }

                    return $"hostName == \"{hostName}\"";
                })));
                html.Append($") {{ siteName = \"{siteName}\"; }} %>");
                html.AppendLine();
            }

            if (hasNoDomainErrorPages)
            {
                // no domains set up, so assume it's a single website and use the root node name
                var siteNode = errorPages.FirstOrDefault().AncestorOrSelf(1);
                var siteName = siteNode.Name.ToSafeAlias();

                html.AppendLine($"<% siteName = \"{siteName}\"; %>");
            }

            html.AppendLine();
            html.AppendLine($"<%-- return file content with a {statusCode} status code --%>");
            html.AppendLine($"<% Response.StatusCode = {statusCode}; %>");
            html.AppendLine($"<% if (!string.IsNullOrEmpty(siteName)) {{ Response.WriteFile(\"{statusCode}-\" + siteName + \".html\"); }} %>");

            var pagePath = HostingEnvironment.MapPath($"~/{statusCode}.aspx");

            try
            {
                System.IO.File.WriteAllText(pagePath, html.ToString(), Encoding.UTF8);
            }
            catch (Exception ex)
            {
                _logger.Error(typeof(ErrorPageService), ex, $"Failed to save error handling page: {statusCode}");
            }
        }

        public void Dispose()
        {
            _contextReference.Dispose();
        }
    }
}
