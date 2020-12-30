using System.IO;
using System.Security.Cryptography;
using System.Text;
using System.Web.Hosting;
using Umbraco.Core.Cache;
using Umbraco.Web;
using Umbraco.Web.Composing;

namespace HappyPorch.UmbracoExtensions.Core.Extensions
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

        /// <summary>
        /// Gets SHA256 file hash for the specified file. Used to add a hash to the static assets URLs to clear the browser cache:
        /// /assets/css/main.css?hash=@Umbraco.GetFileHash("/assets/css/main.css") =>
        /// /assets/css/main.css?hash=DA1CC0B5FB4E777FE556FB8D61168C
        /// </summary>
        /// <param name="helper"></param>
        /// <param name="filePath"></param>
        /// <returns></returns>
        public static string GetFileHash(this UmbracoHelper helper, string filePath)
        {
            return Current.AppCaches.RuntimeCache.GetCacheItem($"FileHash-{filePath}", () =>
            {
                if (string.IsNullOrEmpty(filePath))
                {
                    return null;
                }

                var serverFilePath = HostingEnvironment.MapPath(filePath);

                if (!File.Exists(serverFilePath))
                {
                    return null;
                }

                byte[] hash;

                using (var sha = SHA256.Create())
                using (var stream = File.OpenRead(serverFilePath))
                {
                    hash = sha.ComputeHash(stream);
                }

                var hashOutput = new StringBuilder(hash.Length);

                for (var i = 0; i < hash.Length - 1; i++)
                {
                    hashOutput.Append(hash[i].ToString("X2"));
                }

                return hashOutput.ToString();
            });
        }
    }
}
