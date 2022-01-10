using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.IO;
using Umbraco.Core;
using Umbraco.Core.Composing;
using Umbraco.Core.Events;
using Umbraco.Core.IO;
using Umbraco.Core.Models;
using Umbraco.Core.Services;
using Umbraco.Core.Services.Implement;

namespace HappyPorch.UmbracoExtensions.Core.Components
{
    [RuntimeLevel(MinLevel = RuntimeLevel.Run)]
    public class MediaDeletionComposer : IUserComposer
    {
        public void Compose(Composition composition)
        {
            composition.Components().Append<MediaDeletionComponent>();
        }
    }

    public class MediaDeletionComponent : IComponent
    {
        private readonly IMediaService _mediaService;
        private readonly IMediaFileSystem _mediaFileSystem;

        private const string deletedMediaPrefix = "deleted---";

        public MediaDeletionComponent(IMediaService mediaService, IMediaFileSystem mediaFileSystem)
        {
            _mediaService = mediaService;
            _mediaFileSystem = mediaFileSystem;
        }

        public void Initialize()
        {
            MediaService.Trashed += ObfuscateDeletedMediaFiles;
            MediaService.Moving += DeobfuscateRestoredMediaFiles;
        }

        private void ObfuscateDeletedMediaFiles(IMediaService sender, MoveEventArgs<IMedia> e)
        {
            foreach (var item in e.MoveInfoCollection)
            {
                UpdateMediaFile(item.Entity, true);
            }
        }

        private void DeobfuscateRestoredMediaFiles(IMediaService sender, MoveEventArgs<IMedia> e)
        {
            foreach (var item in e.MoveInfoCollection)
            {
                if (!item.Entity.Trashed || item.NewParentId == Constants.System.RecycleBinMedia)
                {
                    // only check media items that are moved from the recycling bin
                    continue;
                }

                UpdateMediaFile(item.Entity, false);
            }
        }

        private void UpdateMediaFile(IMedia media, bool obfuscate)
        {
            var file = media.Properties[Constants.Conventions.Media.File];

            var fileSrc = file.GetValue()?.ToString();

            if (fileSrc.DetectIsJson())
            {
                fileSrc = JsonConvert.DeserializeObject<JObject>(fileSrc).GetValue("src")?.ToString();
            }

            var fileDirectory = Path.GetDirectoryName(fileSrc);
            var fileName = Path.GetFileName(fileSrc);

            if (obfuscate && fileName.StartsWith(deletedMediaPrefix))
            {
                // already renamed, nothing to do here
                return;
            }
            else if (!obfuscate && !fileName.StartsWith(deletedMediaPrefix))
            {
                // doesn't have the prefix, nothing to do here
                return;
            }

            var oldFilePath = Path.Combine(fileDirectory, fileName);
            var newFilePath = Path.Combine(fileDirectory, (obfuscate ? deletedMediaPrefix + fileName : fileName.TrimStart(deletedMediaPrefix)));

            _mediaFileSystem.CopyFile(oldFilePath, newFilePath);
            _mediaFileSystem.DeleteFile(oldFilePath);

            file.SetValue(newFilePath);

            _mediaService.Save(media, raiseEvents: false);
        }

        public void Terminate()
        {
        }
    }
}
