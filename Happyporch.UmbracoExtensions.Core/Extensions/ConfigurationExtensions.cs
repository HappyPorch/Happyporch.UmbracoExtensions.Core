using System;
using System.Collections.Generic;
using System.Linq;
using Umbraco.Core.Cache;
using Umbraco.Core.Models.PublishedContent;
using Umbraco.Web;
using Umbraco.Web.Composing;

namespace Happyporch.UmbracoExtensions.Core.Extensions
{
    public static class ConfigurationExtensions
    {
        /// <summary>
        /// Gets the strongly typed auxiliary content (e.g. website settings) for the current site.
        /// </summary>
        /// <example>
        /// Homepage node has a composition doc type of AuxiliaryFolder (strongly-typed interface: IAuxiliaryFolder)
        /// which has a property called WebsiteSettings (MNTP limited to doc type WebsiteSettings).
        /// To get the website settings on the current node call: 
        /// Model.GetAuxiliaryContent&lt;IAuxiliaryFolder, WebsiteSettings&gt;(x => x.WebsiteSettings)
        /// This will return the selected WebsiteSettings node with its strongly-typed model.
        /// </example>
        /// <param name="content"></param>
        /// <param name="property">The strongly typed property that contains the MNTP value for the auxiliary content</param>
        /// <returns></returns>
        public static TAuxiliaryType GetAuxiliaryContent<TAuxiliaryFolder, TAuxiliaryType>(this UmbracoHelper helper, Func<TAuxiliaryFolder, IPublishedContent> property)
            where TAuxiliaryFolder : class, IPublishedContent
            where TAuxiliaryType : class, IPublishedContent
        {
            var content = helper.AssignedContentItem;
            var currentPageId = content.Id;
            var auxiliaryFolderNode = content?.AncestorOrSelf<TAuxiliaryFolder>();

            if (auxiliaryFolderNode == null)
            {
                if (Current.UmbracoContext == null || currentPageId == content?.Id)
                {
                    // no Umbraco context found or it's for the same node we already checked
                    return default(TAuxiliaryType);
                }

                // try to get it based on current page ID, in case 'content' is a Nested Content node.
                var currentPage = Current.AppCaches.RuntimeCache.GetCacheItem<IPublishedContent>(currentPageId.ToString());

                auxiliaryFolderNode = currentPage?.AncestorOrSelf<TAuxiliaryFolder>();
            }

            if (auxiliaryFolderNode == null)
            {
                return default(TAuxiliaryType);
            }

            return property(auxiliaryFolderNode) as TAuxiliaryType;
        }

        /// <summary>
        /// See previous GetAuxiliaryContent method.
        /// This one was added with hardcoded values because this package does not know 
        /// about the types used for the doc types on the website.
        /// </summary>
        /// <param name="content"></param>
        /// <returns></returns>
        public static IPublishedContent GetAuxiliaryContent(this IPublishedContent content)
        {
            var propertyName = "websiteSettings";
            var auxiliaryFolderNode = content?.AncestorsOrSelf()?.FirstOrDefault(c => c.HasProperty(propertyName));
            var currentPage = GetAssignedContentItem();

            if (auxiliaryFolderNode == null)
            {
                if (currentPage == null || currentPage.Id == content?.Id)
                {
                    // no Umbraco context found or it's for the same node we already checked
                    return default(IPublishedContent);
                }

                // try to get it based on current page ID, in case 'content' is a Nested Content node.
                currentPage = Current.AppCaches.RuntimeCache.GetCacheItem<IPublishedContent>(currentPage.Id.ToString());

                auxiliaryFolderNode = currentPage?.AncestorsOrSelf()?.FirstOrDefault(c => c.HasProperty(propertyName));
            }

            return auxiliaryFolderNode?.Value<IPublishedContent>(propertyName);
        }

        /// <summary>
        /// Gets the page being browsed by the user.
        /// </summary>
        /// <returns></returns>
        public static IPublishedContent GetAssignedContentItem()
        {
            return Current.UmbracoHelper.AssignedContentItem;
        }
    }
}