using Umbraco.Core.Logging;
using Umbraco.Core;
using Umbraco.Core.Composing;
using Umbraco.Core.Services.Implement;
using Umbraco.Core.Events;

namespace HappyPorch.UmbracoExtensions.Core.Components
{
    [RuntimeLevel(MinLevel = RuntimeLevel.Run)]
    public class AllowDeletionComposer : IUserComposer
    {
        public void Compose(Composition composition)
        {
            composition.Components().Append<AllowDeletionComponent>();
        }
    }

    public class AllowDeletionComponent : IComponent
    {
        private readonly ILogger _logger;

        public AllowDeletionComponent(ILogger logger)
        {
            _logger = logger;
        }

        public void Initialize()
        {
            ContentService.Trashing += ContentService_Trashing;
        }

        private void ContentService_Trashing(Umbraco.Core.Services.IContentService sender, MoveEventArgs<Umbraco.Core.Models.IContent> e)
        {
            var allowDeleteAlias = "AllowDelete";

            foreach (var content in e.MoveInfoCollection)
            {
                // hasn't got the allow delete property
                if (!content.Entity.HasProperty(allowDeleteAlias)) continue;

                var allowDelete = content.Entity.GetValue<bool>(allowDeleteAlias);
                if (allowDelete) continue;                 

                _logger.Error(GetType(), $"Deletion of ({content.Entity.Name}) has been blocked.");
                e.Cancel = true;
                e.Messages.Add(new EventMessage("Deletion of this item has been blocked", "This item is important to the successful operation of this website.<br>If you would still like to delete this item, please uncheck the 'Disable Delete' field on the 'Delete' tab.", EventMessageType.Warning));
            }
        }

        public void Terminate()
        {
        }
    }
}
