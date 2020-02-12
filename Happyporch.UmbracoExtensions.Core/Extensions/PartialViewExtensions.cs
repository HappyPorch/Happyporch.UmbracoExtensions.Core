using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.Mvc.Html;
using Umbraco.Core.Models.PublishedContent;
using Umbraco.Web.Mvc;

namespace Happyporch.UmbracoExtensions.Core.Extensions
{
    public static class PartialViewExtensions
    {
        /// <summary>
        /// Renders a partial view from a IPublishedElement by convention.
        /// </summary>
        /// <param name="page"></param>
        /// <param name="element"></param>
        /// <param name="folder"></param>
        public static void RenderElement<T>(this UmbracoViewPage<T> page, IPublishedElement element, string folder = null)
        {
            if (element != null)
            {
                var folderString = folder == null ? "" : folder + "/";
                var partialView = $"{folderString}{element.ContentType.Alias}";
                page.Html.RenderPartial(partialView, element);
            }
        }

        /// <summary>
        /// Renders partial views from a set of IPublishedElement(s) by convention.
        /// </summary>
        /// <param name="page"></param>
        /// <param name="element"></param>
        /// <param name="folder"></param>
        public static void RenderElements<T>(this UmbracoViewPage<T> page, IEnumerable<IPublishedElement> elements, string folder = null)
        {
            if (elements != null && elements.Any())
            {
                var folderString = folder == null ? "" : folder + "/";
                foreach (var element in elements)
                {
                    var partialView = $"{folderString}{element.ContentType.Alias}";
                    page.Html.RenderPartial(partialView, element, new System.Web.Mvc.ViewDataDictionary { { "Elements", elements } });
                }
            }
        }
    }
}
