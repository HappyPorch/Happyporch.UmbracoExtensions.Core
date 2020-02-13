using System;
using System.Collections.Generic;
using System.Linq;
using Umbraco.Core;
using Umbraco.Core.Composing;
using Umbraco.Core.Models;
using Umbraco.Web.Editors;
using Umbraco.Web.Models.ContentEditing;

namespace HappyPorch.UmbracoExtensions.Core.Components
{
    [RuntimeLevel(MinLevel = RuntimeLevel.Run)]
    public class SubscribeToEditorModelEventsComposer : ComponentComposer<SubscribeToEditorModelEvents>
    {
        //this automatically adds the component to the Components collection of the Umbraco composition
    }

    public class SubscribeToEditorModelEvents : IComponent
    {
        private string[] _reservedTabs = new string[] { "Markup", "Auxiliary Folders", "Delete" };

        public void Initialize()
        {
            EditorModelEventManager.SendingContentModel += EditorModelEventManager_SendingContentModel;
        }

        // terminate: runs once when Umbraco stops
        public void Terminate()
        {
        }

        private void EditorModelEventManager_SendingContentModel(System.Web.Http.Filters.HttpActionExecutedContext sender, EditorModelEventArgs<ContentItemDisplay> e)
        {
            // For the editor user, let's hide all the reserved tabs
            if (!e.UmbracoContext.Security.CurrentUser.IsAdmin())
            {
                e.Model.Variants.FirstOrDefault().Tabs = e.Model.Variants.FirstOrDefault().Tabs.Where(t => !_reservedTabs.Contains(t.Alias));
            }

            var bodyElements = e.Model.Variants.FirstOrDefault().Tabs.FirstOrDefault(f => f.Alias == "Content")?.Properties?.FirstOrDefault(f => f.Alias == "bodyElements");
            if (bodyElements != null)
            {
                //TODO: allow changing module names.
            }
        }
    }
}