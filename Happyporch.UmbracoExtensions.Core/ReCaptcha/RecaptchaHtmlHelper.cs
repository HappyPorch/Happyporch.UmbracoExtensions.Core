using System.Configuration;
using System.Text;
using System.Web;
using System.Web.Mvc;

namespace HappyPorch.UmbracoExtensions.Core.Recaptcha
{
    public static class RecaptchaHtmlHelper
    {
        const string scriptTag = "<script type=\"text/javascript\" src=\"https://www.google.com/recaptcha/api.js\" ></script> ";
        const string divTag = " <div class=\"g-recaptcha\" data-sitekey =\"{0}\"></div>";
        public static IHtmlString reCaptcha(this HtmlHelper helper, bool includeScriptTag = true)
        {
            StringBuilder sb = new StringBuilder();
            string siteKey = ConfigurationManager.AppSettings["RecaptchaSiteKey"];
            if (includeScriptTag)
                sb.AppendLine(scriptTag);
            sb.AppendFormat(divTag, siteKey);
            return MvcHtmlString.Create(sb.ToString());
        }

        public static IHtmlString reCaptchaScript(this HtmlHelper helper)
        {
            return MvcHtmlString.Create(scriptTag);
        }
    }
}