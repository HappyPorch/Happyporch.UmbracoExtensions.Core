using ImageProcessor.Web.Helpers;
using ImageProcessor.Web.HttpModules;
using System.Linq;
using System.Web;
using Umbraco.Core;
using Umbraco.Core.Composing;
using Umbraco.Core.Events;
using Umbraco.Core.Services;
using Umbraco.Core.Services.Implement;


namespace HappyPorch.UmbracoExtensions.Core.Components
{
    [RuntimeLevel(MinLevel = RuntimeLevel.Run)]
    public class ImageProcessorComposer : IUserComposer
    {
        public void Compose(Composition composition)
        {
            composition.Components().Append<ImageProcessorComponent>();
        }
    }

    public class ImageProcessorComponent : IComponent
    {
        public void Initialize()
        {
            // Validate incoming image processor requests
            ImageProcessingModule.ValidatingRequest += ImageProcessingModule_ValidatingRequest;
            // Clear the cached image quality whenever website settings is published
            ContentService.Published += ClearImageCompression;
        }

        /// <summary>
        /// Check that incoming ImageProcessor URLs have got valid querystring settings.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ImageProcessingModule_ValidatingRequest(object sender, ValidatingRequestEventArgs e)
        {
            var queryCollection = HttpUtility.ParseQueryString(e.QueryString);

            // remove any date stamp added by Foundation JS to avoid new crops having to be generated
            if (queryCollection.AllKeys.Contains(null))
            {
                queryCollection.Remove(null);

                e.QueryString = queryCollection.ToString();
            }
        }

        /// <summary>
        /// Clear the cached image quality whenever website settings is published
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ClearImageCompression(IContentService sender, ContentPublishedEventArgs e)
        {
            if (e.PublishedEntities.Any(x => x.HasProperty("imageCompression")))
            {
                Current.AppCaches.RuntimeCache.Clear(ApplicationConstants.ImageQuality);
            }
        }

        public void Terminate()
        {
        }
    }
}
