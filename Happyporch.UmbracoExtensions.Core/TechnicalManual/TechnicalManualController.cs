using HappyPorch.UmbracoExtensions.Core.PropertyEditors;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Web.Hosting;
using System.Web.Http;
using Umbraco.Core;
using Umbraco.Core.Models;
using Umbraco.Core.Models.PublishedContent;
using Umbraco.Web.PropertyEditors;
using Umbraco.Web.WebApi;

namespace HappyPorch.UmbracoExtensions.Core.TechnicalManual
{
    public class TechnicalManualController : UmbracoAuthorizedApiController
    {
        private readonly IVariationContextAccessor _variantContextAccessor;

        public TechnicalManualController(IVariationContextAccessor variationContextAccessor)
        {
            _variantContextAccessor = variationContextAccessor;
        }

        [HttpGet]
        public HttpResponseMessage Index()
        {
            EnsureEnglishVariantContext();

            var html = GenerateManualHtml();

            var response = new HttpResponseMessage();

            response.StatusCode = HttpStatusCode.OK;

            response.Content = new StringContent(html, Encoding.UTF8);
            response.Content.Headers.ContentType = new MediaTypeHeaderValue("text/html");

            return response;
        }

        /// <summary>
        /// Ensure that the correct English variant context is being used.
        /// </summary>
        private void EnsureEnglishVariantContext()
        {
            var englishLanguage = Services.LocalizationService.GetAllLanguages()?.FirstOrDefault(l => l.IsoCode.InvariantStartsWith("en"));

            if (englishLanguage != null)
            {
                _variantContextAccessor.VariationContext = new VariationContext(englishLanguage.IsoCode);
            }
        }

        private string GenerateManualHtml()
        {
            var html = new StringBuilder();

            html.AppendLine("<div class=\"technical-manual\">");
            html.AppendLine("<style>");
            html.AppendLine(".technical-manual .top-border { border-top: 2px solid #f3f3f5; }");
            html.AppendLine(".technical-manual li { margin-bottom: 5px; }");
            html.AppendLine("</style>");

            AddManualHeading(html);

            var bluePrints = Services.ContentService.GetBlueprintsForContentTypes().ToList();
            var documentTypes = GetDocumentTypes();
            var moduleTypes = GetModuleTypes(bluePrints);

            AddTableOfContents(html, documentTypes, moduleTypes);

            AddNotes(html);

            AddDocumentTypesAndTemplates(html, documentTypes, bluePrints, moduleTypes);

            AddModules(html, moduleTypes, documentTypes, bluePrints);

            html.AppendLine("</div>");

            return html.ToString();
        }

        /// <summary>
        /// Gets list of doc types that are used for pages and website settings.
        /// </summary>
        /// <returns></returns>
        private IList<IContentType> GetDocumentTypes()
        {
            return Services.ContentTypeService.GetAll()
                        .Where(c => c.Alias.EndsWith("Page") || c.Alias.EndsWith("websiteSettings"))
                        .OrderBy(c => c.Name)
                        .ToList();
        }

        /// <summary>
        /// Gets list of element doc types that are used across the website (based on blueprints set up).
        /// </summary>
        /// <param name="bluePrints"></param>
        /// <returns></returns>
        private IList<IContentType> GetModuleTypes(IList<IContent> bluePrints)
        {
            var elementDocTypeAliases = new List<string>();

            // get list of used element doc type IDs by looking at all Nested Content Elements properties in the available blueprints
            foreach (var bluePrint in bluePrints)
            {
                foreach (var elementProperty in bluePrint.Properties.Where(p => p.PropertyType.PropertyEditorAlias == NestedContentElementsPropertyEditor.EditorAlias))
                {
                    var value = elementProperty.GetValue();

                    if (value == null && bluePrint.EditedCultures?.Any() == true)
                    {
                        // try it with default culture
                        value = elementProperty.GetValue(bluePrint.EditedCultures.FirstOrDefault());
                    }

                    if (value == null)
                    {
                        continue;
                    }

                    var elements = JsonConvert.DeserializeObject<List<JObject>>(value.ToString());

                    foreach (var element in elements)
                    {
                        var elementDocTypeAlias = element["ncContentTypeAlias"]?.ToObject<string>();

                        if (!string.IsNullOrEmpty(elementDocTypeAlias) && !elementDocTypeAliases.Contains(elementDocTypeAlias))
                        {
                            elementDocTypeAliases.Add(elementDocTypeAlias);
                        }
                    }
                }
            }

            return Services.ContentTypeService.GetAll()
                        .Where(c => c.IsElement && elementDocTypeAliases.Contains(c.Alias))
                        .OrderBy(c => c.Name)
                        .ToList();
        }

        private void AddManualHeading(StringBuilder html)
        {
            AddUmbBox(html, "<h2 class=\"bold\">Website Technical Manual</h2><p>This document contains technical documentation of selected features.</p>");
        }

        private void AddTableOfContents(StringBuilder html, IList<IContentType> documentTypes, IList<IContentType> moduleTypes)
        {
            var toc = new StringBuilder();

            toc.AppendLine("<ul>");

            toc.AppendLine("<li>");
            toc.AppendLine("<button type=\"button\" class=\"btn-link -underline\" onClick=\"document.getElementById('notes').scrollIntoView()\">Notes</button>");
            toc.AppendLine("</li>");

            toc.AppendLine("<li>");
            toc.AppendLine("<button type=\"button\" class=\"btn-link -underline\" onClick=\"document.getElementById('document-types-templates').scrollIntoView()\">Document Types & Templates</button>");

            toc.AppendLine("<ul>");

            foreach (var documentType in documentTypes)
            {
                toc.AppendLine("<li>");
                toc.AppendLine($"<button type=\"button\" class=\"btn-link -underline\" onClick=\"document.getElementById('{documentType.Alias}').scrollIntoView()\">{documentType.Name}</button>");
                toc.AppendLine("</li>");
            }

            toc.AppendLine("</ul>");

            toc.AppendLine("</li>");

            toc.AppendLine("<li>");
            toc.AppendLine("<button type=\"button\" class=\"btn-link -underline\" onClick=\"document.getElementById('modules').scrollIntoView()\">Modules</button>");

            toc.AppendLine("<ul>");

            foreach (var module in moduleTypes)
            {
                toc.AppendLine("<li>");
                toc.AppendLine($"<button type=\"button\" class=\"btn-link -underline\" onClick=\"document.getElementById('{module.Alias}').scrollIntoView()\">{module.Name}</button>");
                toc.AppendLine("</li>");
            }

            toc.AppendLine("</ul>");

            toc.AppendLine("</li>");

            toc.AppendLine("</ul>");

            AddUmbPanelGroup(html, "Contents", "contents", toc.ToString());
        }

        private void AddNotes(StringBuilder html)
        {
            var notes = new StringBuilder();

            var notesPath = HostingEnvironment.MapPath("~/App_Plugins/UmbracoBase/technical-manual-notes.html");

            if (System.IO.File.Exists(notesPath))
            {
                notes.AppendLine(System.IO.File.ReadAllText(notesPath));
            }

            AddUmbPanelGroup(html, "Notes", "notes", notes.ToString());
        }

        private void AddDocumentTypesAndTemplates(StringBuilder html, IList<IContentType> documentTypes, IList<IContent> bluePrints, IList<IContentType> moduleTypes)
        {
            var docTypes = new StringBuilder();

            docTypes.AppendLine("</div>");

            var excludedTabs = new string[] { "Redirect", "SEO", "Visibility", "Markup", "Delete", "Auxiliary Folders" };

            foreach (var documentType in documentTypes)
            {
                docTypes.AppendLine("<div class=\"umb-panel-group__details-check-title top-border\">");

                docTypes.AppendLine($"<div class=\"umb-panel-group__details-check-name\" id=\"{documentType.Alias}\">");
                docTypes.AppendLine("<h3>");
                docTypes.AppendLine(documentType.Name);
                docTypes.AppendLine("</h3>");
                docTypes.AppendLine("</div>");

                // get URL of an example page
                var content = Umbraco.ContentAtXPath($"//{documentType.Alias}").FirstOrDefault(c => c.ContentType.Alias == documentType.Alias && c.TemplateId > 0);

                // https://our.umbraco.com/documentation/Reference/Language-Variation/

                docTypes.AppendLine($"<div class=\"umb-panel-group__details-check-description\">");
                docTypes.AppendLine($"{documentType.Description} {(content != null ? $"<a href=\"{content.Url}\" target=\"_blank\" class=\"btn-link -underline\">Example here</a>" : null)}");
                docTypes.AppendLine("</div>");

                docTypes.AppendLine($"<br />");

                // document type structure
                var allowedParentDocTypes = documentTypes.Where(d => d.AllowedContentTypes.Any(c => c.Alias == documentType.Alias)).Select(d => d.Name).OrderBy(d => d);
                var allowedChildDocTypes = documentTypes.Where(d => documentType.AllowedContentTypes.Any(c => c.Alias == d.Alias)).Select(d => d.Name).OrderBy(d => d);

                docTypes.AppendLine($"<div>");
                docTypes.AppendLine("<strong>Structure:</strong>");
                docTypes.AppendLine("<ul>");
                docTypes.AppendLine("<li>");
                docTypes.AppendLine($"<strong>Allowed parent doc types:</strong> {(allowedParentDocTypes.Any() ? string.Join(", ", allowedParentDocTypes) : "none")}.");
                docTypes.AppendLine("</li>");
                docTypes.AppendLine("<li>");
                docTypes.AppendLine($"<strong>Allowed child doc types:</strong> {(allowedChildDocTypes.Any() ? string.Join(", ", allowedChildDocTypes) : "none")}.");
                docTypes.AppendLine("</li>");
                docTypes.AppendLine("</ul>");
                docTypes.AppendLine("</div>");

                docTypes.AppendLine($"<br />");

                // properties grouped by tabs
                docTypes.AppendLine($"<div>");
                docTypes.AppendLine("<strong>Properties:</strong>");
                docTypes.AppendLine("<ul>");

                var tabs = documentType.PropertyGroups.Union(documentType.CompositionPropertyGroups).Where(p => !excludedTabs.Contains(p.Name)).OrderBy(p => p.SortOrder);

                foreach (var tab in tabs)
                {
                    docTypes.AppendLine("<li>");
                    docTypes.AppendLine($"<strong>{tab.Name}</strong> tab:");
                    docTypes.AppendLine("<ul>");

                    foreach (var property in tab.PropertyTypes)
                    {
                        AddDocumentTypeProperty(docTypes, property, documentType, documentTypes, moduleTypes, bluePrints);
                    }

                    docTypes.AppendLine("</ul>");
                    docTypes.AppendLine("</li>");
                }

                docTypes.AppendLine("</ul>");
                docTypes.AppendLine("</div>");

                docTypes.AppendLine("</div>");
            }

            // remove last closing div tag (inc. 2 EOL chars)
            docTypes.Remove(docTypes.Length - 8, 8);

            AddUmbPanelGroup(html, "Document Types & Templates", "document-types-templates", docTypes.ToString());
        }

        private void AddModules(StringBuilder html, IList<IContentType> moduleTypes, IList<IContentType> documentTypes, IList<IContent> bluePrints)
        {
            var modules = new StringBuilder();

            modules.AppendLine("</div>");

            foreach (var moduleType in moduleTypes)
            {
                modules.AppendLine("<div class=\"umb-panel-group__details-check-title top-border\">");

                modules.AppendLine($"<div class=\"umb-panel-group__details-check-name\" id=\"{moduleType.Alias}\">");
                modules.AppendLine("<h3>");
                modules.AppendLine(moduleType.Name);
                modules.AppendLine("</h3>");
                modules.AppendLine("</div>");

                // properties of the module
                modules.AppendLine($"<div>");
                modules.AppendLine("<strong>Properties:</strong>");
                modules.AppendLine("<ul>");

                var properties = moduleType.PropertyGroups.SelectMany(p => p.PropertyTypes).Union(moduleType.CompositionPropertyGroups.SelectMany(p => p.PropertyTypes)).OrderBy(p => p.SortOrder);

                foreach (var property in properties)
                {
                    AddDocumentTypeProperty(modules, property, moduleType, documentTypes, moduleTypes, bluePrints);
                }

                modules.AppendLine("</ul>");
                modules.AppendLine("</div>");

                modules.AppendLine("</div>");
            }

            // remove last closing div tag (inc. 2 EOL chars)
            modules.Remove(modules.Length - 8, 8);

            AddUmbPanelGroup(html, "Modules", "modules", modules.ToString());
        }

        private void AddDocumentTypeProperty(StringBuilder html, PropertyType property, IContentType documentType, IList<IContentType> documentTypes, IList<IContentType> moduleTypes, IList<IContent> bluePrints)
        {
            html.AppendLine("<li>");
            html.AppendLine($"<strong>{property.Name}:</strong> {property.Description}");

            if (property.PropertyEditorAlias == Constants.PropertyEditors.Aliases.NestedContent)
            {
                // list allowed element types
                var dataType = Services.DataTypeService.GetDataType(property.DataTypeId);

                if (dataType != null)
                {
                    var configuration = dataType.ConfigurationAs<NestedContentConfiguration>();

                    if (configuration.ContentTypes?.Any() == true)
                    {
                        html.AppendLine("<div>The elements can be of type:</div>");
                        html.AppendLine("<ul>");

                        var allowedElementTypes = Services.ContentTypeService.GetAll().Where(d => configuration.ContentTypes.Any(c => c.Alias == d.Alias)).OrderBy(d => d.Name);

                        foreach (var elementType in allowedElementTypes)
                        {
                            html.AppendLine("<li>");
                            html.AppendLine($"<strong>{elementType.Name}</strong> with the following properties:");

                            html.AppendLine("<ul>");

                            var allowedDocumentTypeProperties = elementType.PropertyGroups.SelectMany(p => p.PropertyTypes).Union(elementType.CompositionPropertyGroups.SelectMany(p => p.PropertyTypes)).OrderBy(p => p.SortOrder);

                            foreach (var allowedDocumentTypeProperty in allowedDocumentTypeProperties)
                            {
                                AddDocumentTypeProperty(html, allowedDocumentTypeProperty, elementType, documentTypes, moduleTypes, bluePrints);
                            }

                            html.AppendLine("</ul>");

                            html.AppendLine("</li>");
                        }

                        html.AppendLine("</ul>");
                    }
                }
            }

            if (property.PropertyEditorAlias == NestedContentElementsPropertyEditor.EditorAlias)
            {
                // list default element types from blueprint
                var bluePrint = bluePrints.FirstOrDefault(b => b.ContentType.Alias == documentType.Alias);

                if (bluePrint != null)
                {
                    var bluePrintProperty = bluePrint.Properties.FirstOrDefault(p => p.Alias == property.Alias);

                    if (bluePrintProperty != null)
                    {
                        var value = bluePrintProperty.GetValue();

                        if (value == null && bluePrint.EditedCultures?.Any() == true)
                        {
                            // try it with default culture
                            value = bluePrintProperty.GetValue(bluePrint.EditedCultures.FirstOrDefault());
                        }

                        if (value != null)
                        {
                            html.AppendLine("<ul>");

                            var elements = JsonConvert.DeserializeObject<List<JObject>>(value.ToString());

                            foreach (var element in elements)
                            {
                                var elementDocTypeAlias = element["ncContentTypeAlias"]?.ToObject<string>();

                                if (!string.IsNullOrEmpty(elementDocTypeAlias))
                                {
                                    var elementDocType = moduleTypes.FirstOrDefault(m => m.Alias == elementDocTypeAlias);

                                    if (elementDocType != null)
                                    {
                                        html.AppendLine("<li>");
                                        html.AppendLine($"<strong><button class=\"btn-link -underline\" onclick=\"document.getElementById('{elementDocType.Alias}').scrollIntoView()\">{elementDocType.Name}</button></strong>");
                                        html.AppendLine("</li>");
                                    }
                                }
                            }

                            html.AppendLine("</ul>");
                        }
                    }
                }
            }

            html.AppendLine("</li>");
        }

        private void AddUmbBox(StringBuilder html, string content)
        {
            html.AppendLine("<div class=\"umb-box\">");
            html.AppendLine("<div class=\"umb-box-content\">");

            html.AppendLine(content);

            html.AppendLine("</div>");
            html.AppendLine("</div>");
        }

        private void AddUmbPanelGroup(StringBuilder html, string title, string sectionId, string content)
        {
            html.AppendLine("<div class=\"umb-panel-group__details-group\">");

            html.AppendLine("<div class=\"umb-panel-group__details-group-title\">");
            html.AppendLine($"<div class=\"umb-panel-group__details-group-name\" id=\"{sectionId}\">");

            html.AppendLine(title);

            html.AppendLine("</div>");
            html.AppendLine("</div>");

            html.AppendLine("<div class=\"umb-panel-group__details-checks\">");
            html.AppendLine("<div class=\"umb-panel-group__details-check\">");
            html.AppendLine("<div class=\"umb-panel-group__details-check-title\">");

            html.AppendLine(content);

            html.AppendLine("</div>");
            html.AppendLine("</div>");
            html.AppendLine("</div>");

            html.AppendLine("</div>");

            html.AppendLine("<br />");
        }
    }
}
