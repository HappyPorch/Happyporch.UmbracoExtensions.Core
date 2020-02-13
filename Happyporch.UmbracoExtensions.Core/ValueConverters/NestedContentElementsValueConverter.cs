using Umbraco.Core;
using Umbraco.Core.Logging;
using Umbraco.Core.Models.PublishedContent;
using Umbraco.Web.PropertyEditors.ValueConverters;
using Umbraco.Web.PublishedCache;
using HappyPorch.UmbracoExtensions.Core.PropertyEditors;

namespace HappyPorch.UmbracoExtensions.Core.ValueConverters
{
    public class NestedContentElementsValueConverter : NestedContentManyValueConverter
    {
        public NestedContentElementsValueConverter(IPublishedSnapshotAccessor publishedSnapshotAccessor, IPublishedModelFactory publishedModelFactory, IProfilingLogger proflog)
            : base(publishedSnapshotAccessor, publishedModelFactory, proflog)
        {
        }

        public override bool IsConverter(IPublishedPropertyType propertyType)
        {
            return propertyType.EditorAlias.InvariantEquals(NestedContentElementsPropertyEditor.EditorAlias);
        }
    }
}
