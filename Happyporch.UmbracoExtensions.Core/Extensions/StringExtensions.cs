using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Web;

namespace Happyporch.UmbracoExtensions.Core.Extensions
{
    public static class StringExtensions
    {
        private static readonly Regex RegexSquareBrackets = new Regex(@"\[(.*\n*.*)\]");

        /// <summary>
        /// (typed) gets the contents of a textarea in a list split by line breaks
        /// </summary>
        /// <param name="content"></param>
        /// <returns></returns>
        public static IEnumerable<string> GetStringSplitByLines(this string content)
        {
            return content.Split(new string[] {"\r\n", "\n"},
                StringSplitOptions.RemoveEmptyEntries);
        }

        /// <summary>
        /// Turns line breaks in regular text into html markup with line breaks
        /// </summary>
        /// <param name="content"></param>
        /// <returns></returns>
        public static IHtmlString ShowTextWithLineBreaks(this string content)
        {
            var text = content.GetStringSplitByLines();
            var htmlText = string.Join("<br />" + Environment.NewLine, text);
            return new HtmlString(htmlText);
        }

        /// <summary>
        /// Replaces text between square brackets with specified HTML tags.
        /// Example: "this is [bigger] text".ReplaceSquareBracketsWithTag("big") will return "this is <big>bigger</big> text".
        /// </summary>
        /// <param name="text"></param>
        /// <param name="tag"></param>
        /// <param name="attributes"></param>
        /// <returns></returns>
        public static string ReplaceSquareBracketsWithTag(this string text, string tag, string attributes = null)
        {
            if (string.IsNullOrEmpty(text))
            {
                return text;
            }

            if (!string.IsNullOrEmpty(attributes))
            {
                attributes = " " + attributes;
            }

            text = RegexSquareBrackets.Replace(text, $"<{tag}{attributes}>$1</{tag}>");

            return text;

        }
    }
}
