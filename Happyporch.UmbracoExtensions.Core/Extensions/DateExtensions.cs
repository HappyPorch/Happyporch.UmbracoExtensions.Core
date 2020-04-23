using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HappyPorch.UmbracoExtensions.Core.Extensions
{
    public static class DateExtensions
    {
        /// <summary>
        /// Datetime formatter that also handles suffixes (th, st, nd and rd).
        /// Use 'nn' or 'NN' for the suffix token.
        /// </summary>
        /// <param name="dateTime"></param>
        /// <param name="format"></param>
        /// <returns></returns>
        public static string ToSuffixString(this DateTime dateTime, string format)
        {
            return dateTime.ToString(format)
                .Replace("nn", dateTime.Day.ToOccurrenceSuffix().ToLower())
                .Replace("NN", dateTime.Day.ToOccurrenceSuffix().ToUpper());
        }

        private static string ToOccurrenceSuffix(this int integer)
        {
            switch (integer % 100)
            {
                case 11:
                case 12:
                case 13:
                    return "th";
            }
            switch (integer % 10)
            {
                case 1:
                    return "st";
                case 2:
                    return "nd";
                case 3:
                    return "rd";
                default:
                    return "th";
            }
        }

    }
}
