using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using System.Web.Mvc.Html;
using Umbraco.Core.Models.PublishedContent;
using Umbraco.Web.Mvc;

namespace HappyPorch.UmbracoExtensions.Core.Extensions
{
    public static class PartialViewExtensions
    {
        /// <summary>
        /// Renders a partial view from a IPublishedElement by convention.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="page"></param>
        /// <param name="element"></param>
        /// <param name="folder"></param>
        /// <param name="viewData"></param>
        public static void RenderElement<T>(this UmbracoViewPage<T> page, IPublishedElement element, string folder = null, ViewDataDictionary viewData = null)
        {
            if (element != null)
            {
                var folderString = folder == null ? "" : folder + "/";
                var partialView = $"{folderString}{element.ContentType.Alias}";
                page.Html.RenderPartial(partialView, element, viewData);
            }
        }

        /// <summary>
        /// Renders partial views from a set of IPublishedElement(s) by convention.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="page"></param>
        /// <param name="elements"></param>
        /// <param name="folder"></param>
        public static void RenderElements<T>(this UmbracoViewPage<T> page, IEnumerable<IPublishedElement> elements, string folder = null)
        {
            if (elements == null || !elements.Any())
            {
                return;
            }
            foreach (var element in elements)
            {
                page.RenderElement(element, folder, new System.Web.Mvc.ViewDataDictionary { { "Elements", elements }, { "ViewModel", page.Model } });
            }
        }
    }
}
