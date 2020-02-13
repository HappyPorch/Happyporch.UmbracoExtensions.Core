using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Web;
using System.Web.Configuration;
using Umbraco.Core.Composing;

namespace HappyPorch.UmbracoExtensions.Core.Recaptcha
{
    public static class Recaptcha
    {
        const string verifyUri = "https://www.google.com/recaptcha/api/siteverify?secret={0}&response={1}";
        public static RecaptchaResponseObject Validate()
        {
            string secretKey = WebConfigurationManager.AppSettings["RecaptchaSecretKey"];
            string gRecatchaResponse = HttpContext.Current.Request["g-recaptcha-response"];

            try
            {
                HttpWebRequest req = (HttpWebRequest)WebRequest.Create(String.Format(verifyUri, secretKey, gRecatchaResponse));

                WebResponse response = req.GetResponse();

                using (StreamReader readStream = new StreamReader(response.GetResponseStream()))
                {
                    string jsonResponse = readStream.ReadToEnd();

                    RecaptchaResponseObject jobj = JsonConvert.DeserializeObject<RecaptchaResponseObject>(jsonResponse);

                    if (!jobj.Success)
                    {
                        Current.Logger.Warn(typeof(Recaptcha), "Unable to deserialize Recaptcha json payload: {json}", jsonResponse);
                    }

                    return jobj;
                }
            }
            catch (Exception ex)
            {
                Current.Logger.Warn(typeof(Recaptcha), ex, ex.Message);
                return new RecaptchaResponseObject { Success = false };
            }
        }

        /// <summary>
        /// See https://developers.google.com/recaptcha/docs/verify
        /// </summary>
        public class RecaptchaResponseObject
        {
            [JsonProperty("success")]
            public bool Success { get; set; }
            [JsonProperty("error-codes")]
            public List<string> ErrorCodes { get; set; }
        }
    }
}