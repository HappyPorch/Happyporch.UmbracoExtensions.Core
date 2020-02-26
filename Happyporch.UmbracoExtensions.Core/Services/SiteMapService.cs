using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;
using Umbraco.Core.Models;
using Umbraco.Core.Models.PublishedContent;
using Umbraco.Web;
using Umbraco.Core.Logging;

namespace HappyPorch.UmbracoExtensions.Core.Services
{
    public class SiteMapService
    {
        private readonly UmbracoContext _context;
        private readonly ILogger _logger;

        public SiteMapService(UmbracoContext context, ILogger logger)
        {
            _context = context;
            _logger = logger;
        }

        /// <summary>
        /// Gets a list of all the site's pages and descendant pages to be included in the site map.
        /// </summary>
        /// <param name="umbracoContext"></param>
        /// <param name="rootNodeId"></param>
        /// <returns></returns>
        public IEnumerable<IPublishedContent> GetSiteMapPages(int? rootNodeId = null)
        {
            if (_context == null)
                return null;

            // get all root nodes, or specific root node
            var rootNodes = rootNodeId.HasValue
                ? new List<IPublishedContent> { _context.Content.GetById(rootNodeId.Value) }
                : _context.Content.GetAtRoot()
                    .Where(x => (x.TemplateId > 0) && x.Value<bool>("HideInXmlsitemap") == false);

            if (!rootNodes.Any())
                return null;

            var pageList = new List<IPublishedContent>();

            foreach (var page in rootNodes)
            {
                pageList.Add(page);
                // add root node and descendants (with template and not hidden from sitemap)
                var descendants = page.Descendants().Where(x => x.TemplateId > 0 && !x.Value<bool>("HideInXmlsitemap"));
                pageList.AddRange(descendants);
            }
            return pageList;
        }

        /// <summary>
        /// Gets an XML site map of all the website's pages and descendant pages.
        /// </summary>
        /// <param name="umbracoContext"></param>
        /// <param name="rootNodeId"></param>
        /// <returns></returns>
        public XDocument GetSiteMapXml(int? rootNodeId = null)
        {
            // create the XML sitemap
            var doc = new XDocument
            {
                Declaration = new XDeclaration("1.0", "utf-8", null)
            };

            // create URL set
            XNamespace xmlns = "http://www.sitemaps.org/schemas/sitemap/0.9";

            var urlSet = new XElement(XName.Get("urlset", xmlns.NamespaceName));

            // get all pages for sitemap
            var pages = GetSiteMapPages(rootNodeId).ToList();

            if (pages.Any())
            {
                // add each page as a Url element
                foreach (var page in pages)
                {
                    foreach (var culture in page.Cultures)
                    {
                        var url = new XElement(xmlns + "url");

                        url.Add(new XElement(xmlns + "loc", page.Url(culture.Key, mode: UrlMode.Absolute)));
                        url.Add(new XElement(xmlns + "lastmod", page.UpdateDate.ToString("yyyy-MM-dd")));

                        urlSet.Add(url);
                    }
                }
            }
            doc.Add(urlSet);

            return doc;
        }

        /// <summary>
        /// Gets an XML site map of all the site's pages and descendant pages in a UTF-8 encoded string.
        /// </summary>
        /// <param name="umbracoContext"></param>
        /// <param name="rootNodeId"></param>
        /// <returns></returns>
        public string GetSiteMapXmlString(int? rootNodeId = null)
        {
            var doc = GetSiteMapXml(rootNodeId);

            return $"{doc.Declaration}{Environment.NewLine}{doc}";
        }
    }
}
