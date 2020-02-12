using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Umbraco.Web.Models;

namespace Happyporch.UmbracoExtensions.Core.Extensions
{
    public static class LinkExtensions
    {
        /// <summary>
        /// Displays a link tag.
        /// </summary>
        /// <param name="links"></param>
        /// <param name="innerText"></param>
        /// <param name="cssClass"></param>
        /// <returns></returns>
        public static HtmlString GetLink(this IEnumerable<Link> links, string innerText = null, string cssClass = null)
        {
            if (links == null || !links.Any())
            {
                return null;
            }

            var link = links.First();

            return link.GetLink(innerText, cssClass);
        }

        /// <summary>
        /// Displays a link tag.
        /// </summary>
        /// <param name="link"></param>
        /// <param name="innerText"></param>
        /// <param name="cssClass"></param>
        /// <returns></returns>
        public static HtmlString GetLink(this Link link, string innerText = null, string cssClass = null)
        {
            if (!link.IsValidLink())
            {
                return null;
            }

            var tagBuilder = new TagBuilder("a");

            tagBuilder.MergeAttribute("href", link?.Url);
            tagBuilder.MergeAttribute("target", link?.Target);
            if (cssClass != null)
            {
                tagBuilder.MergeAttribute("class", cssClass);
            }
            tagBuilder.SetInnerText(innerText ?? link?.Name);

            return new HtmlString(tagBuilder.ToString());
        }

        /// <summary>
        /// Displays a link url if it is set. Works for Single Url Picker.
        /// </summary>
        /// <param name="links"></param>
        /// <returns></returns>
        public static HtmlString GetLinkUrl(this IEnumerable<Link> links)
        {
            if (links == null || !links.Any())
            {
                return null;
            }

            var link = links.First();

            return link.GetLinkUrl();
        }

        /// <summary>
        /// Displays a link url if it is set. Works for Single Url Picker.
        /// </summary>
        /// <param name="link"></param>
        /// <returns></returns>
        public static HtmlString GetLinkUrl(this Link link)
        {
            if (link == null)
            {
                return null;
            }

            var url = link.Url;
            return new HtmlString(url);
        }

        /// <summary>
        /// Surrounds this HtmlString with a link tag.
        /// If link is not defined then only the HtmlString will display.
        /// </summary>
        /// <param name="str"></param>
        /// <param name="link"></param>
        /// <returns></returns>
        public static HtmlString SurroundWithLink(this HtmlString str, Link link)
        {
            if (link == null || str == null)
            {
                return str;
            }

            var tagBuilder = new TagBuilder("a");

            tagBuilder.MergeAttribute("href", link?.Url);
            tagBuilder.MergeAttribute("target", link?.Target);
            tagBuilder.InnerHtml = str.ToString();

            return new HtmlString(tagBuilder.ToString());
        }

        /// <summary>
        /// Surrounds this HtmlString with a link tag.
        /// </summary>
        /// <param name="str"></param>
        /// <param name="url"></param>
        /// <param name="target"></param>
        /// <returns></returns>
        public static HtmlString SurroundWithLink(this IHtmlString str, string url, string target = null)
        {
            if (str == null)
            {
                return null;
            }
            var tagBuilder = new TagBuilder("a");

            tagBuilder.MergeAttribute("href", url);
            tagBuilder.MergeAttribute("target", target);
            tagBuilder.InnerHtml = str.ToString();

            return new HtmlString(tagBuilder.ToString());
        }

        /// <summary>
        /// Works for Single Url Picker. Adds a link around some content. Will show only the content if link is not set.
        /// </summary>
        /// <param name="item"></param>
        /// <param name="content"></param>
        /// <param name="cssClass"></param>
        public static IHtmlString SurroundWithLink(this IHtmlString content, IEnumerable<Link> item, string cssClass = null)
        {
            if (item == null || content == null || !item.Any())
            {
                return content;
            }
            var link = item.First();

            return content.SurroundWithLink(link, cssClass);
        }

        /// <summary>
        /// Works for SingleUrlPicker. Adds a link around some content. Will show only the content if link is not set.
        /// </summary>
        /// <param name="link"></param>
        /// <param name="content"></param>
        /// <param name="cssClass"></param>
        public static IHtmlString SurroundWithLink(this IHtmlString content, Link link, string cssClass = null)
        {
            if (link == null || content == null)
            {
                return content;
            }
            var tagBuilder = new TagBuilder("a");

            tagBuilder.MergeAttribute("href", link.Url);
            tagBuilder.MergeAttribute("target", link.Target);
            tagBuilder.MergeAttribute("class", cssClass);
            tagBuilder.InnerHtml = content.ToString();

            return new HtmlString(tagBuilder.ToString());
        }

        /// <summary>
        /// Surrounds this string with a link tag.
        /// </summary>
        /// <param name="str"></param>
        /// <param name="url"></param>
        /// <param name="target"></param>
        /// <returns></returns>
        public static IHtmlString SurroundWithLink(this string content, Link link, string cssClass = null)
        {
            return SurroundWithLink(new HtmlString(content), link, cssClass);
        }

        /// <summary>
        /// Verifies that the first Link in the picker is set and that the page it links to is published.
        /// </summary>
        /// <returns></returns>
        public static bool IsValidLink(this IEnumerable<Link> item)
        {
            if (item == null || !item.Any())
            {
                //takes care of unpublished pages
                return false;
            }
            var link = item.First();

            return link.IsValidLink();
        }

        /// <summary>
        /// Verifies that the first Link in the picker is set and that the page it links to is published.
        /// </summary>
        /// <returns></returns>
        public static bool IsValidLink(this Link link)
        {
            if (link == null)
            {
                //takes care of unpublished pages
                return false;
            }
            if (link.Type == LinkType.External)
            {
                return !string.IsNullOrEmpty(link.Url);
            }
            return true;
        }
    }
}
