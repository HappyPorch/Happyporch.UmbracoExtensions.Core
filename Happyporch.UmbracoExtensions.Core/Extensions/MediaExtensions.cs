using System;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;
using Umbraco.Core.Cache;
using Umbraco.Core.Composing;
using Umbraco.Core.Models.PublishedContent;
using Umbraco.Web;
using Umbraco.Web.Models;
using UmbracoWeb = Umbraco.Web;

namespace Happyporch.UmbracoExtensions.Core.Extensions
{
    public static class MediaExtensions
    {
        /// <summary>
        /// Gets the crop URL for the media item using the website's image quality setting.
        /// </summary>
        /// <param name="image"></param>
        /// <param name="cropAlias"></param>
        /// <param name="cropMode"></param>
        /// <returns></returns>
        public static string GetCroppedImageUrl(this IPublishedContent image, string cropAlias, ImageCropMode imageCropMode = ImageCropMode.Crop)
        {
            var url = image?.GetCropUrl(cropAlias: cropAlias, useCropDimensions: true, quality: ImageQuality, imageCropMode: imageCropMode);
            return url;
        }

        /// <summary>
        /// (Works for Images and svg) Gets the img tag with the cropped media item using its alternative text and website's image quality setting.
        /// </summary>
        /// <param name="image"></param>
        /// <param name="cropAlias"></param>
        /// <param name="cssClass"></param>
        /// <param name="htmlAttributes"></param>
        /// <param name="cropMode"></param>
        /// <returns></returns>
        public static HtmlString GetCroppedImage(this IPublishedContent image, string cropAlias, string cssClass = null, object htmlAttributes = null, ImageCropMode imageCropMode = ImageCropMode.Crop)
        {
            if (image == null)
            {
                return null;
            }

            var tagBuilder = new TagBuilder("img");

            tagBuilder.MergeAttribute("src", image.GetCroppedImageUrl(cropAlias, imageCropMode));
            tagBuilder.MergeAttribute("class", cssClass);
            tagBuilder.MergeAttribute("alt", image.Value<string>("AltText"));

            RenderAttributes(htmlAttributes, tagBuilder);

            return new HtmlString(tagBuilder.ToString(TagRenderMode.SelfClosing) + Environment.NewLine);
        }

        /// <summary>
        /// (Works for Images and svg) Gets the img tag with the media item using its alternative text and website's image quality setting and no cropping.
        /// </summary>
        /// <param name="image"></param>
        /// <param name="cssClass"></param>
        /// <returns></returns>
        public static HtmlString GetImage(this IPublishedContent image, string cssClass = null)
        {
            if (image == null)
            {
                return null;
            }

            var tagBuilder = new TagBuilder("img");

            tagBuilder.MergeAttribute("src", image.Url + "?quality=" + ImageQuality);
            tagBuilder.MergeAttribute("class", cssClass);
            tagBuilder.MergeAttribute("alt", image.Value<string>("AltText"));

            return new HtmlString(tagBuilder.ToString(TagRenderMode.SelfClosing) + Environment.NewLine);
        }

        /// <summary>
        /// (Works for Images and svg) Gets the img tag with the media item using its alternative text and website's image quality setting and no cropping.
        /// </summary>
        /// <param name="image"></param>
        /// <returns></returns>
        public static string GetImageUrl(this IPublishedContent image)
        {
            return image?.Url + "?quality=" + ImageQuality;
        }

        /// <summary>
        /// Renders attributes for a specific tag
        /// </summary>
        /// <param name="htmlAttributes"></param>
        /// <param name="tagBuilder"></param>
        private static void RenderAttributes(object htmlAttributes, TagBuilder tagBuilder)
        {
            if (htmlAttributes != null)
            {
                // add attributes to the tag element
                var attributes = (htmlAttributes is RouteValueDictionary ? htmlAttributes as RouteValueDictionary : HtmlHelper.AnonymousObjectToHtmlAttributes(htmlAttributes));

                foreach (var attribute in attributes)
                {
                    // only replace existing attribute when it's not style (otherwise we'll loose the background image)
                    var replaceExisting = (attribute.Key != "style");

                    tagBuilder.MergeAttribute(attribute.Key, attribute.Value?.ToString(), replaceExisting);
                }
            }
        }

        /// <summary>
        /// Gets the image quality from website settings to be used by the image cropper.
        /// </summary>
        private static int ImageQuality
        {
            get
            {
                return Current.AppCaches.RuntimeCache.GetCacheItem(ApplicationConstants.ImageQuality, () =>
                {
                    // get website settings using current request's node
                    var currentNode = UmbracoWeb.Composing.Current.UmbracoContext?.PublishedRequest?.PublishedContent;

                    var websiteSettings = currentNode?.GetAuxiliaryContent();

                    var imageQuality = 100;

                    if (websiteSettings?.HasProperty("imageCompression") == true)
                    {
                        // get the image compression setting
                        if (int.TryParse(websiteSettings?.Value<string>("imageCompression"), out var imageCompression))
                        {
                            // convert the compression value to the quality setting (quality = 100% - compression)
                            imageQuality = 100 - imageCompression;
                        }
                        else
                        {
                            // nothing set, use full quality
                            imageQuality = 100;
                        }
                    }

                    return imageQuality;
                });
            }
        }
    }
}
