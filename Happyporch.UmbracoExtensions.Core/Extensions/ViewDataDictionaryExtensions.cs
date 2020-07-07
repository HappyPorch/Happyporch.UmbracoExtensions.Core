using System.Collections.Generic;
using System.Linq;

namespace HappyPorch.UmbracoExtensions.Extensions
{
    public static class ViewDataDictionaryExtensions
    {
        /// <summary>
        /// Clones the existing ViewDataDictionary into a new object.
        /// </summary>
        /// <param name="viewData"></param>
        /// <returns></returns>
        public static System.Web.Mvc.ViewDataDictionary Clone<T>(this System.Web.Mvc.ViewDataDictionary<T> viewData)
        {
            return Clone(viewData);
        }

        /// <summary>
        /// Clones the existing ViewDataDictionary into a new object.
        /// </summary>
        /// <param name="viewData"></param>
        /// <returns></returns>
        public static System.Web.Mvc.ViewDataDictionary Clone(this System.Web.Mvc.ViewDataDictionary viewData)
        {
            if (viewData == null)
            {
                return null;
            }

            return new System.Web.Mvc.ViewDataDictionary(viewData);
        }

        /// <summary>
        /// Extends the existing ViewDataDictionary with the specified items.
        /// Example:
        /// Pass a new ViewData object to a partial view and extend it with a custom value:
        /// @Html.Partial("MyPartial", Model, ViewData.Clone().Extend(new KeyValuePair&lt;string, object&gt;("IsMobileNav", true))
        /// </summary>
        /// <param name="viewData"></param>
        /// <param name="items"></param>
        /// <returns></returns>
        public static System.Web.Mvc.ViewDataDictionary Extend<T>(this System.Web.Mvc.ViewDataDictionary<T> viewData, params KeyValuePair<string, object>[] items)
        {
            return Extend(viewData, items);
        }

        /// <summary>
        /// Extends the existing ViewDataDictionary with the specified items.
        /// Example:
        /// Pass a new ViewData object to a partial view and extend it with a custom value:
        /// @Html.Partial("MyPartial", Model, ViewData.Clone().Extend(new KeyValuePair&lt;string, object&gt;("IsMobileNav", true))
        /// </summary>
        /// <param name="viewData"></param>
        /// <param name="items"></param>
        /// <returns></returns>
        public static System.Web.Mvc.ViewDataDictionary Extend(this System.Web.Mvc.ViewDataDictionary viewData, params KeyValuePair<string, object>[] items)
        {
            if (viewData == null || items == null || !items.Any())
            {
                return viewData;
            }

            foreach (var item in items)
            {
                if (viewData.ContainsKey(item.Key))
                {
                    // replace the existing item
                    viewData[item.Key] = item.Value;
                }
                else
                {
                    // add the new item
                    viewData.Add(item);
                }
            }

            return viewData;
        }
    }
}
