using System.Collections.Generic;
using System.Linq;
using Umbraco.Core.Models;
using Umbraco.Core.Models.PublishedContent;
using Umbraco.Web;

namespace Happyporch.UmbracoExtensions.Core.Extensions
{
    public static class CollectionExtensions
    {
        /// <summary>
        /// Gets all visible elements in the collection (using umbracoNaviHide)
        /// </summary>
        /// <param name="content"></param>
        /// <returns></returns>
        public static IEnumerable<T> Visible<T>(this IEnumerable<T> content) where T : IPublishedElement
        {
            content = content.Where(o => o.Value<bool>("umbracoNaviHide") != true);
            return content;
        }

        /// <summary>
        /// Splits a collection into collections of the same size
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="source"></param>
        /// <param name="chunkSize"></param>
        /// <returns></returns>
        public static IEnumerable<IEnumerable<T>> ChunkBy<T>(this IEnumerable<T> source, int chunkSize)
        {
            return source
                .Select((x, i) => new { Index = i, Value = x })
                .GroupBy(x => x.Index / chunkSize)
                .Select(x => x.Select(v => v.Value).ToList())
                .ToList();
        }

        /// <summary>
        /// Splits a collection into x parts.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="list"></param>
        /// <param name="parts"></param>
        /// <returns></returns>
        public static IEnumerable<IEnumerable<T>> SplitInto<T>(this IEnumerable<T> list, int parts)
        {
            int i = 0;
            var splits = from item in list
                group item by i++ % parts into part
                select part.AsEnumerable();
            return splits;
        }
    }
}
