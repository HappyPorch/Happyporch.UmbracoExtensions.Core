using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Umbraco.Web;

namespace Happyporch.UmbracoExtensions.Core.Extensions
{
    public static class UmbracoHelperExtensions
    {
        public static string Coalesce(this UmbracoHelper helper, string string1, string string2)
        {
            if (!string.IsNullOrWhiteSpace(string1))
            {
                return string1;
            }
            return string2;
        }
    }
}
