using System.Linq;
using System.Text.RegularExpressions;
using System.Web.Http.Filters;
using Umbraco.Core;
using Umbraco.Core.Composing;
using Umbraco.Core.Models;
using Umbraco.Core.Strings;
using Umbraco.Web.Editors;
using Umbraco.Web.Models.ContentEditing;

namespace HappyPorch.UmbracoExtensions.Core.Components
{
    [RuntimeLevel(MinLevel = RuntimeLevel.Run)]
    public class SubscribeToEditorModelEventsComposer : ComponentComposer<SubscribeToEditorModelEventsComponent>
    {
        //this automatically adds the component to the Components collection of the Umbraco composition
    }

    public class SubscribeToEditorModelEventsComponent : IComponent
    {
        private string[] _reservedTabs = new string[] { "Markup", "Auxiliary Folders", "Delete" };

        private Regex _hideForPlaceHolderRegex = new Regex(@"\[HideFor.+?(?=\])\]", RegexOptions.Multiline);

        public void Initialize()
        {
            EditorModelEventManager.SendingContentModel += EditorModelEventManager_SendingContentModel;
        }

        // terminate: runs once when Umbraco stops
        public void Terminate()
        {
        }

        private void EditorModelEventManager_SendingContentModel(HttpActionExecutedContext sender, EditorModelEventArgs<ContentItemDisplay> e)
        {
            HideReservedTabs(e);
            HidePropertiesForUserGroups(e);
            CustomiseBodyElements(e);
        }

        /// <summary>
        /// For the editor user, let's hide all the reserved tabs.
        /// </summary>
        /// <param name="e"></param>
        private void HideReservedTabs(EditorModelEventArgs<ContentItemDisplay> e)
        {
            if (!e.UmbracoContext.Security.CurrentUser.IsAdmin())
            {
                foreach (var variant in e.Model.Variants)
                {
                    variant.Tabs = variant.Tabs.Where(t => !_reservedTabs.Contains(t.Alias));
                }
            }
        }

        /// <summary>
        /// Hide any content properties that don't apply to the current user group (using [HideForEditors] placeholder in property description).
        /// </summary>
        /// <param name="e"></param>
        private void HidePropertiesForUserGroups(EditorModelEventArgs<ContentItemDisplay> e)
        {
            var userHidForPlaceholders = e.UmbracoContext.Security.CurrentUser.Groups.Select(g => $"[HideFor{g.Name.ToCleanString(CleanStringType.PascalCase)}]");

            foreach (var variant in e.Model.Variants)
            {
                foreach (var tab in variant.Tabs)
                {
                    // exclude properties that should be hidden for this user
                    tab.Properties = tab.Properties?.Where(p => p.Description?.ContainsAny(userHidForPlaceholders) != true);

                    if (tab.Properties != null)
                    {
                        // remove the placeholders of properties that the current user is allowed to see
                        foreach (var property in tab.Properties)
                        {
                            property.Description = _hideForPlaceHolderRegex.Replace(property.Description, string.Empty)?.Trim();
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Customise the body elements (e.g. changing display names).
        /// </summary>
        /// <param name="e"></param>
        private void CustomiseBodyElements(EditorModelEventArgs<ContentItemDisplay> e)
        {
            var bodyElements = e.Model.Variants.Select(v => v.Tabs.FirstOrDefault(f => f.Alias == "Content")?.Properties?.FirstOrDefault(f => f.Alias == "bodyElements"));

            if (bodyElements != null)
            {
                //TODO: allow changing module names.
            }
        }
    }
}