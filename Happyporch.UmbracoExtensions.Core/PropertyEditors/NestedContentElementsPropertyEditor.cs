using System;
using Umbraco.Core;
using Umbraco.Core.Logging;
using Umbraco.Core.PropertyEditors;
using Umbraco.Core.Services;
using Umbraco.Web.PropertyEditors;

namespace HappyPorch.UmbracoExtensions.Core.PropertyEditors
{
    [DataEditor(
        EditorAlias,
        "Nested Content - Elements",
        "~/App_Plugins/UmbracoBase/backoffice/views/nestedcontent.html",
        ValueType = ValueTypes.Json,
        Group = Constants.PropertyEditors.Groups.Lists,
        Icon = "icon-thumbnail-list")]
    public class NestedContentElementsPropertyEditor : NestedContentPropertyEditor
    {
        public const string EditorAlias = Constants.PropertyEditors.Aliases.NestedContent + "Elements";

        public NestedContentElementsPropertyEditor(ILogger logger, Lazy<PropertyEditorCollection> propertyEditors, IDataTypeService dataTypeService, IContentTypeService contentTypeService)
            : base(logger, propertyEditors, dataTypeService, contentTypeService)
        {
        }
    }
}
