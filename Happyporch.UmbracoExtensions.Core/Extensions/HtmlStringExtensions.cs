using System.Web;
using Umbraco.Core;

namespace HappyPorch.UmbracoExtensions.Core.Extensions
{
    public static class HtmlStringExtensions
    {
        /// <summary>
        /// Removes the first surrounding tag of the HTML string.
        /// </summary>
        /// <param name="html">HTML string</param>
        /// <param name="tag">Name of the tag to remove (e.g. "p")</param>
        /// <returns></returns>
        public static IHtmlString RemoveSurroundingTag(this IHtmlString html, string tag)
        {
            if (html == null)
            {
                return html;
            }

            return new HtmlString(html.ToString().TrimStart($"<{tag}>").TrimEnd($"</{tag}>"));
        }
    }
}
