using ImageProcessor.Web.Helpers;
using ImageProcessor.Web.HttpModules;
using System.Linq;
using System.Web;
using Umbraco.Core;
using Umbraco.Core.Composing;

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

        public void Terminate()
        {
        }
    }
}
