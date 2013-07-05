using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Xml.Linq;
using Umbraco.Core;
using Umbraco.Core.Strings;
using Umbraco.Core.Configuration;
using Umbraco.Core.Models;
using Umbraco.Core.Services;

namespace Umbraco.VisualStudio
{
    /// <summary>
    /// Temporary class to enable serialization and deserialization of nodes.
    /// Will only be used untill the PackagingService in Umbraco is stable/complete.
    /// </summary>
    internal class Packaging
    {
        private readonly ServiceContext _serviceContext;

        public Packaging(ServiceContext serviceContext)
        {
            _serviceContext = serviceContext;
        }

        #region Generic export methods

        internal void ExportToFile(string absoluteFilePath, string nodeType, int id)
        {
            XElement xml = null;

            if (nodeType.Equals("content", StringComparison.InvariantCultureIgnoreCase))
            {
                var content = _serviceContext.ContentService.GetById(id);
                xml = Export(content);
            }

            if (nodeType.Equals("media", StringComparison.InvariantCultureIgnoreCase))
            {
                var media = _serviceContext.MediaService.GetById(id);
                xml = Export(media);
            }

            if (nodeType.Equals("contenttype", StringComparison.InvariantCultureIgnoreCase))
            {
                var contentType = _serviceContext.ContentTypeService.GetContentType(id);
                xml = Export(contentType);
            }

            if (nodeType.Equals("mediatype", StringComparison.InvariantCultureIgnoreCase))
            {
                var mediaType = _serviceContext.ContentTypeService.GetMediaType(id);
                xml = Export(mediaType);
            }

            if (nodeType.Equals("datatype", StringComparison.InvariantCultureIgnoreCase))
            {
                var dataType = _serviceContext.DataTypeService.GetDataTypeDefinitionById(id);
                xml = Export(dataType);
            }

            if (xml != null)
                xml.Save(absoluteFilePath);
        }

        #endregion

        #region Content

        /// <summary>
        /// Exports an <see cref="IContent"/> item to xml as an <see cref="XElement"/>
        /// </summary>
        /// <param name="content">Content to export</param>
        /// <param name="deep">Optional parameter indicating whether to include descendents</param>
        /// <returns><see cref="XElement"/> containing the xml representation of the Content object</returns>
        internal XElement Export(IContent content, bool deep = false)
        {
            //nodeName should match Casing.SafeAliasWithForcingCheck(content.ContentType.Alias);
            var nodeName = UmbracoSettings.UseLegacyXmlSchema ? "node" : content.ContentType.Alias.ToSafeAliasWithForcingCheck();

            var xml = Export(content, nodeName);
            xml.Add(new XAttribute("nodeType", content.ContentType.Id));
            xml.Add(new XAttribute("creatorName", content.GetCreatorProfile().Name));
            xml.Add(new XAttribute("writerName", content.GetWriterProfile().Name));
            xml.Add(new XAttribute("writerID", content.WriterId));
            xml.Add(new XAttribute("template", content.Template == null ? "0" : content.Template.Id.ToString(CultureInfo.InvariantCulture)));
            xml.Add(new XAttribute("nodeTypeAlias", content.ContentType.Alias));

            if (deep)
            {
                var descendants = content.Descendants().ToArray();
                var currentChildren = descendants.Where(x => x.ParentId == content.Id);
                AddChildXml(descendants, currentChildren, xml);
            }

            var documents = new XElement("DocumentSet", xml);
            return documents;
        }

        /// <summary>
        /// Part of the export of IContent and IMedia which is shared
        /// </summary>
        /// <param name="contentBase">Base Content or Media to export</param>
        /// <param name="nodeName">Name of the node</param>
        /// <returns><see cref="XElement"/></returns>
        private XElement Export(IContentBase contentBase, string nodeName)
        {
            //NOTE: that one will take care of umbracoUrlName
            var url = contentBase.GetUrlSegment();

            var xml = new XElement(nodeName,
                                   new XAttribute("id", contentBase.Id),
                                   new XAttribute("parentID", contentBase.Level > 1 ? contentBase.ParentId : -1),
                                   new XAttribute("level", contentBase.Level),
                                   new XAttribute("creatorID", contentBase.CreatorId),
                                   new XAttribute("sortOrder", contentBase.SortOrder),
                                   new XAttribute("createDate", contentBase.CreateDate.ToString("s")),
                                   new XAttribute("updateDate", contentBase.UpdateDate.ToString("s")),
                                   new XAttribute("nodeName", contentBase.Name),
                                   new XAttribute("urlName", url),
                                   new XAttribute("path", contentBase.Path),
                                   new XAttribute("isDoc", ""));

            foreach (var property in contentBase.Properties.Where(p => p != null))
                xml.Add(property.ToXml());

            return xml;
        }

        /// <summary>
        /// Used by Content Export to recursively add children
        /// </summary>
        /// <param name="originalDescendants"></param>
        /// <param name="currentChildren"></param>
        /// <param name="currentXml"></param>
        private void AddChildXml(IContent[] originalDescendants, IEnumerable<IContent> currentChildren, XElement currentXml)
        {
            foreach (var child in currentChildren)
            {
                //add the child's xml
                var childXml = Export(child);
                currentXml.Add(childXml);
                //copy local (out of closure)
                var c = child;
                //get this item's children                
                var children = originalDescendants.Where(x => x.ParentId == c.Id);
                //recurse and add it's children to the child xml element
                AddChildXml(originalDescendants, children, childXml);
            }
        }

        #endregion

        #region ContentTypes

        /// <summary>
        /// Exports an <see cref="IContentType"/> to xml as an <see cref="XElement"/>
        /// </summary>
        /// <param name="contentType">ContentType to export</param>
        /// <returns><see cref="XElement"/> containing the xml representation of the ContentType item.</returns>
        public XElement Export(IContentType contentType)
        {
            var info = new XElement("Info",
                                    new XElement("Name", contentType.Name),
                                    new XElement("Alias", contentType.Alias),
                                    new XElement("Icon", contentType.Icon),
                                    new XElement("Thumbnail", contentType.Thumbnail),
                                    new XElement("Description", contentType.Description),
                                    new XElement("AllowAtRoot", contentType.AllowedAsRoot.ToString()));

            var masterContentType = contentType.CompositionAliases().FirstOrDefault();
            if (masterContentType != null)
                info.Add(new XElement("Master", masterContentType));

            var allowedTemplates = new XElement("AllowedTemplates");
            foreach (var template in contentType.AllowedTemplates)
            {
                allowedTemplates.Add(new XElement("Template", template.Alias));
            }
            info.Add(allowedTemplates);
            if (contentType.DefaultTemplate != null && contentType.DefaultTemplate.Id != 0)
                info.Add(new XElement("DefaultTemplate", contentType.DefaultTemplate.Alias));
            else
                info.Add(new XElement("DefaultTemplate", ""));

            var structure = new XElement("Structure");
            foreach (var allowedType in contentType.AllowedContentTypes)
            {
                structure.Add(new XElement("DocumentType", allowedType.Alias));
            }

            var genericProperties = new XElement("GenericProperties");
            foreach (var propertyType in contentType.PropertyTypes)
            {
                var definition = _serviceContext.DataTypeService.GetDataTypeDefinitionById(propertyType.DataTypeDefinitionId);
                var propertyGroupId = propertyType.PropertyGroupId == null ? default(int) : propertyType.PropertyGroupId.Value;
                var propertyGroup = contentType.PropertyGroups.FirstOrDefault(x => x.Id == propertyGroupId);
                var genericProperty = new XElement("GenericProperty",
                                                   new XElement("Name", propertyType.Name),
                                                   new XElement("Alias", propertyType.Alias),
                                                   new XElement("Type", propertyType.DataTypeId.ToString()),
                                                   new XElement("Definition", definition.Key),
                                                   new XElement("Tab", propertyGroup == null ? "" : propertyGroup.Name),
                                                   new XElement("Mandatory", propertyType.Mandatory.ToString()),
                                                   new XElement("Validation", propertyType.ValidationRegExp),
                                                   new XElement("Description", new XCData(propertyType.Description)));
                genericProperties.Add(genericProperty);
            }

            var tabs = new XElement("Tabs");
            foreach (var propertyGroup in contentType.PropertyGroups)
            {
                var tab = new XElement("Tab",
                                       new XElement("Id", propertyGroup.Id.ToString(CultureInfo.InvariantCulture)),
                                       new XElement("Caption", propertyGroup.Name));
                tabs.Add(tab);
            }

            var xml = new XElement("DocumentType",
                                   info,
                                   structure,
                                   genericProperties,
                                   tabs);
            return xml;
        }

        #endregion

        #region DataTypes

        internal XElement Export(IDataTypeDefinition dataTypeDefinition)
        {
            var prevalues = new XElement("PreValues");

            var prevalueList = ((DataTypeService)_serviceContext.DataTypeService).GetDetailedPreValuesByDataTypeId(dataTypeDefinition.Id);
            foreach (var tuple in prevalueList)
            {
                var prevalue = new XElement("PreValue");
                prevalue.Add(new XAttribute("Id", tuple.Item1));
                prevalue.Add(new XAttribute("Value", tuple.Item4));
                prevalue.Add(new XAttribute("Alias", tuple.Item2));
                prevalue.Add(new XAttribute("SortOrder", tuple.Item3));
                prevalues.Add(prevalue);
            }

            var xml = new XElement("DataType", prevalues);
            xml.Add(new XAttribute("Name", dataTypeDefinition.Name));
            xml.Add(new XAttribute("Id", dataTypeDefinition.Id));
            xml.Add(new XAttribute("Definition", dataTypeDefinition.Key));
            xml.Add(new XAttribute("DatabaseType", dataTypeDefinition.DatabaseType.ToString()));

            return xml;
        }

        #endregion

        #region Media

        /// <summary>
        /// Exports an <see cref="IMedia"/> item to xml as an <see cref="XElement"/>
        /// </summary>
        /// <param name="media">Media to export</param>
        /// <param name="deep">Optional parameter indicating whether to include descendents</param>
        /// <returns><see cref="XElement"/> containing the xml representation of the Media object</returns>
        internal XElement Export(IMedia media, bool deep = false)
        {
            //nodeName should match Casing.SafeAliasWithForcingCheck(content.ContentType.Alias);
            var nodeName = UmbracoSettings.UseLegacyXmlSchema ? "node" : media.ContentType.Alias.ToSafeAliasWithForcingCheck();

            var xml = Export(media, nodeName);
            xml.Add(new XAttribute("nodeType", media.ContentType.Id));
            xml.Add(new XAttribute("writerName", media.GetCreatorProfile().Name));
            xml.Add(new XAttribute("writerID", media.CreatorId));
            xml.Add(new XAttribute("version", media.Version));
            xml.Add(new XAttribute("template", 0));
            xml.Add(new XAttribute("nodeTypeAlias", media.ContentType.Alias));

            if (deep)
            {
                var descendants = media.Descendants().ToArray();
                var currentChildren = descendants.Where(x => x.ParentId == media.Id);
                AddChildXml(descendants, currentChildren, xml);
            }

            var medias = new XElement("MediaSet", xml);
            return medias;
        }

        /// <summary>
        /// Used by Media Export to recursively add children
        /// </summary>
        /// <param name="originalDescendants"></param>
        /// <param name="currentChildren"></param>
        /// <param name="currentXml"></param>
        private void AddChildXml(IMedia[] originalDescendants, IEnumerable<IMedia> currentChildren, XElement currentXml)
        {
            foreach (var child in currentChildren)
            {
                //add the child's xml
                var childXml = Export(child);
                currentXml.Add(childXml);
                //copy local (out of closure)
                var c = child;
                //get this item's children                
                var children = originalDescendants.Where(x => x.ParentId == c.Id);
                //recurse and add it's children to the child xml element
                AddChildXml(originalDescendants, children, childXml);
            }
        }

        #endregion

        #region MediaTypes

        /// <summary>
        /// Exports an <see cref="IMediaType"/> to xml as an <see cref="XElement"/>
        /// </summary>
        /// <param name="mediaType">MediaType to export</param>
        /// <returns><see cref="XElement"/> containing the xml representation of the MediaType item.</returns>
        internal XElement Export(IMediaType mediaType)
        {
            var info = new XElement("Info",
                                    new XElement("Name", mediaType.Name),
                                    new XElement("Alias", mediaType.Alias),
                                    new XElement("Icon", mediaType.Icon),
                                    new XElement("Thumbnail", mediaType.Thumbnail),
                                    new XElement("Description", mediaType.Description),
                                    new XElement("AllowAtRoot", mediaType.AllowedAsRoot.ToString()));

            var masterContentType = mediaType.CompositionAliases().FirstOrDefault();
            if (masterContentType != null)
                info.Add(new XElement("Master", masterContentType));

            var structure = new XElement("Structure");
            foreach (var allowedType in mediaType.AllowedContentTypes)
            {
                structure.Add(new XElement("MediaType", allowedType.Alias));
            }

            var genericProperties = new XElement("GenericProperties");
            foreach (var propertyType in mediaType.PropertyTypes)
            {
                var definition = _serviceContext.DataTypeService.GetDataTypeDefinitionById(propertyType.DataTypeDefinitionId);
                var propertyGroup = mediaType.PropertyGroups.FirstOrDefault(x => x.Id == propertyType.PropertyGroupId.Value);
                var genericProperty = new XElement("GenericProperty",
                                                   new XElement("Name", propertyType.Name),
                                                   new XElement("Alias", propertyType.Alias),
                                                   new XElement("Type", propertyType.DataTypeId.ToString()),
                                                   new XElement("Definition", definition.Key),
                                                   new XElement("Tab", propertyGroup == null ? "" : propertyGroup.Name),
                                                   new XElement("Mandatory", propertyType.Mandatory.ToString()),
                                                   new XElement("Validation", propertyType.ValidationRegExp),
                                                   new XElement("Description", new XCData(propertyType.Description)));
                genericProperties.Add(genericProperty);
            }

            var tabs = new XElement("Tabs");
            foreach (var propertyGroup in mediaType.PropertyGroups)
            {
                var tab = new XElement("Tab",
                                       new XElement("Id", propertyGroup.Id.ToString(CultureInfo.InvariantCulture)),
                                       new XElement("Caption", propertyGroup.Name));
                tabs.Add(tab);
            }

            var xml = new XElement("MediaType",
                                   info,
                                   structure,
                                   genericProperties,
                                   tabs);
            return xml;
        }

        #endregion
    }
}