using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Security;
using System.Security.Permissions;
using System.Xml.Linq;
using Umbraco.Core.Models;
using Umbraco.Core.Models.EntityBase;
using Umbraco.Core.Services;
using Umbraco.Core.Standalone;

namespace Umbraco.VisualStudio
{
    public class ServiceBridge : MarshalByRefObject
    {
        private ServiceContext _serviceContext;
        private ServiceContextManager _manager;

        private Assembly _trustedAssembly;
        private readonly List<Assembly> _trustedAssemblies;

        //Create a SandBox to load Assemblies with "Full Trust"
        public static ServiceBridge Sandbox(string assemblyFilename, string configPath, ResolveEventHandler handler)
        {
            var trustedLoadGrantSet = new PermissionSet(PermissionState.Unrestricted);
            var trustedLoadSetup = new AppDomainSetup();
            trustedLoadSetup.ApplicationBase = Path.GetDirectoryName(assemblyFilename);
            trustedLoadSetup.PrivateBinPath = Path.GetDirectoryName(assemblyFilename);
            //trustedLoadSetup.ConfigurationFile = AppDomain.CurrentDomain.SetupInformation.ConfigurationFile;
            trustedLoadSetup.ConfigurationFile = configPath;

            AppDomain.CurrentDomain.AssemblyResolve += handler;
            AppDomain appDomain = AppDomain.CreateDomain("ServiceBridge SandBox", null, trustedLoadSetup, trustedLoadGrantSet);
            appDomain.SetData("DataDirectory", configPath.Replace("Web.config", "App_Data"));

            ServiceBridge loader = appDomain.CreateInstanceAndUnwrap(
                typeof(ServiceBridge).Assembly.GetName().FullName,
                typeof(ServiceBridge).FullName,
                false,
                BindingFlags.Default,
                null,
                new object[] { assemblyFilename },
                CultureInfo.InvariantCulture,
                null) as ServiceBridge;

            return loader;
        }

        public ServiceBridge()
        {
        }

        public ServiceBridge(string assemblyFilename)
        {
            _trustedAssembly = Assembly.LoadFile(assemblyFilename);
            
            var assemblyPath = assemblyFilename.Replace("Umbraco.Core.dll", "");
            _trustedAssemblies = new List<Assembly>();
            _trustedAssemblies.Add(Assembly.LoadFile(Path.Combine(assemblyPath, "businesslogic.dll")));
            _trustedAssemblies.Add(Assembly.LoadFile(Path.Combine(assemblyPath, "cms.dll")));
            _trustedAssemblies.Add(Assembly.LoadFile(Path.Combine(assemblyPath, "controls.dll")));
            _trustedAssemblies.Add(Assembly.LoadFile(Path.Combine(assemblyPath, "interfaces.dll")));
            _trustedAssemblies.Add(Assembly.LoadFile(Path.Combine(assemblyPath, "umbraco.DataLayer.dll")));
            _trustedAssemblies.Add(Assembly.LoadFile(Path.Combine(assemblyPath, "umbraco.dll")));
            _trustedAssemblies.Add(Assembly.LoadFile(Path.Combine(assemblyPath, "umbraco.editorControls.dll")));
            _trustedAssemblies.Add(Assembly.LoadFile(Path.Combine(assemblyPath, "umbraco.MacroEngines.dll")));
            _trustedAssemblies.Add(Assembly.LoadFile(Path.Combine(assemblyPath, "umbraco.macroRenderings.dll")));
            _trustedAssemblies.Add(Assembly.LoadFile(Path.Combine(assemblyPath, "umbraco.providers.dll")));
            _trustedAssemblies.Add(Assembly.LoadFile(Path.Combine(assemblyPath, "Umbraco.Web.UI.dll")));
            _trustedAssemblies.Add(Assembly.LoadFile(Path.Combine(assemblyPath, "umbraco.XmlSerializers.dll")));
        }

        public void Configure(string connectionString, string providerName, string baseDirectory)
        {
            Trace.WriteLine("Current AppDomain: " + AppDomain.CurrentDomain.FriendlyName);
            Trace.WriteLine("Current AppDomain: " + AppDomain.CurrentDomain.BaseDirectory);
            _manager = new ServiceContextManager(connectionString, providerName, baseDirectory);
            _serviceContext = _manager.Services;
        }

        public Dictionary<int, Dictionary<string, string>> GetRootContent()
        {
            var entities = _serviceContext.EntityService.GetRootEntities(UmbracoObjectTypes.Document);
            return ConvertEntitiesToDictionary(entities);
        }

        public Dictionary<int, Dictionary<string, string>> GetRootMedia()
        {
            var entities = _serviceContext.EntityService.GetRootEntities(UmbracoObjectTypes.Media);
            return ConvertEntitiesToDictionary(entities);
        }

        public Dictionary<int, Dictionary<string, string>> GetRootContentTypes()
        {
            var entities = _serviceContext.EntityService.GetRootEntities(UmbracoObjectTypes.DocumentType);
            return ConvertEntitiesToDictionary(entities);
        }

        public Dictionary<int, Dictionary<string, string>> GetRootMediaTypes()
        {
            var entities = _serviceContext.EntityService.GetRootEntities(UmbracoObjectTypes.MediaType);
            return ConvertEntitiesToDictionary(entities);
        }

        public Dictionary<int, Dictionary<string, string>> GetRootDataTypes()
        {
            var entities = _serviceContext.EntityService.GetRootEntities(UmbracoObjectTypes.DataType);
            return ConvertEntitiesToDictionary(entities);
        }

        public Dictionary<int, Dictionary<string, string>> GetChildContent(int parentId)
        {
            var entities = _serviceContext.EntityService.GetChildren(parentId, UmbracoObjectTypes.Document);
            return ConvertEntitiesToDictionary(entities);
        }

        public Dictionary<int, Dictionary<string, string>> GetChildMedia(int parentId)
        {
            var entities = _serviceContext.EntityService.GetChildren(parentId, UmbracoObjectTypes.Media);
            return ConvertEntitiesToDictionary(entities);
        }

        public Dictionary<int, Dictionary<string, string>> GetChildContentTypes(int parentId)
        {
            var entities = _serviceContext.EntityService.GetChildren(parentId, UmbracoObjectTypes.DocumentType);
            return ConvertEntitiesToDictionary(entities);
        }

        public Dictionary<int, Dictionary<string, string>> GetChildMediaTypes(int parentId)
        {
            var entities = _serviceContext.EntityService.GetChildren(parentId, UmbracoObjectTypes.MediaType);
            return ConvertEntitiesToDictionary(entities);
        }

        public Dictionary<int, Dictionary<string, string>> GetChildDataTypes(int parentId)
        {
            var entities = _serviceContext.EntityService.GetChildren(parentId, UmbracoObjectTypes.DataType);
            return ConvertEntitiesToDictionary(entities);
        }

        public Dictionary<string, string> GetContentProperties(int id)
        {
            var content = _serviceContext.ContentService.GetById(id);
            var dictionary = new Dictionary<string, string>
                                 {
                                     {"Id", content.Id.ToString(CultureInfo.InvariantCulture)},
                                     {"ParentId", content.ParentId.ToString(CultureInfo.InvariantCulture)},
                                     {"Name", content.Name},
                                     {"UniqueId", content.Key.ToString()},
                                     {"ContentTypeName", content.ContentType.Name},
                                     {"ContentTypeAlias", content.ContentType.Alias},
                                     {
                                         "CreateDate",
                                         content.CreateDate.ToString(CultureInfo.InvariantCulture)
                                     },
                                     {
                                         "UpdateDate",
                                         content.UpdateDate.ToString(CultureInfo.InvariantCulture)
                                     },
                                     {
                                         "Level",
                                         content.Level.ToString(CultureInfo.InvariantCulture)
                                     },
                                     {"Path", content.Path},
                                     {
                                         "SortOrder",
                                         content.SortOrder.ToString(CultureInfo.InvariantCulture)
                                     },
                                     {"Published", ReturnYesNo(content.Published)},
                                     {"Version", content.Version.ToString()},
                                     {"TemplateAlias", content.Template.Alias}
                                 };
            return dictionary;
        }

        public Dictionary<string, string> GetMediaProperties(int id)
        {
            var content = _serviceContext.MediaService.GetById(id);
            var dictionary = new Dictionary<string, string>
                                 {
                                     {"Id", content.Id.ToString(CultureInfo.InvariantCulture)},
                                     {"ParentId", content.ParentId.ToString(CultureInfo.InvariantCulture)},
                                     {"Name", content.Name},
                                     {"UniqueId", content.Key.ToString()},
                                     {"ContentTypeName", content.ContentType.Name},
                                     {"ContentTypeAlias", content.ContentType.Alias},
                                     {
                                         "CreateDate",
                                         content.CreateDate.ToString(CultureInfo.InvariantCulture)
                                     },
                                     {
                                         "UpdateDate",
                                         content.UpdateDate.ToString(CultureInfo.InvariantCulture)
                                     },
                                     {
                                         "Level",
                                         content.Level.ToString(CultureInfo.InvariantCulture)
                                     },
                                     {"Path", content.Path},
                                     {
                                         "SortOrder",
                                         content.SortOrder.ToString(CultureInfo.InvariantCulture)
                                     }
                                 };
            return dictionary;
        }

        public Dictionary<string, string> GetContentTypeProperties(int id)
        {
            var contentType = _serviceContext.ContentTypeService.GetContentType(id);
            var dictionary = new Dictionary<string, string>
                                 {
                                     {"Id", contentType.Id.ToString(CultureInfo.InvariantCulture)},
                                     {"ParentId", contentType.ParentId.ToString(CultureInfo.InvariantCulture)},
                                     {"Name", contentType.Name},
                                     {"Alias", contentType.Alias},
                                     {"UniqueId", contentType.Key.ToString()},
                                     {"CreateDate", contentType.CreateDate.ToString(CultureInfo.InvariantCulture)},
                                     {"Level", contentType.Level.ToString(CultureInfo.InvariantCulture)},
                                     {"Path", contentType.Path},
                                     {"SortOrder", contentType.SortOrder.ToString(CultureInfo.InvariantCulture)},
                                     {"TemplateAlias", contentType.DefaultTemplate.Alias}
                                 };
            return dictionary;
        }

        public Dictionary<string, string> GetMediaTypeProperties(int id)
        {
            var mediaType = _serviceContext.ContentTypeService.GetMediaType(id);
            var dictionary = new Dictionary<string, string>
                                 {
                                     {"Id", mediaType.Id.ToString(CultureInfo.InvariantCulture)},
                                     {"ParentId", mediaType.ParentId.ToString(CultureInfo.InvariantCulture)},
                                     {"Name", mediaType.Name},
                                     {"Alias", mediaType.Alias},
                                     {"UniqueId", mediaType.Key.ToString()},
                                     {"CreateDate", mediaType.CreateDate.ToString(CultureInfo.InvariantCulture)},
                                     {"Level", mediaType.Level.ToString(CultureInfo.InvariantCulture)},
                                     {"Path", mediaType.Path},
                                     {"SortOrder", mediaType.SortOrder.ToString(CultureInfo.InvariantCulture)}
                                 };
            return dictionary;
        }

        public Dictionary<string, string> GetDataTypeProperties(int id)
        {
            var dataType = _serviceContext.DataTypeService.GetDataTypeDefinitionById(id);
            var dictionary = new Dictionary<string, string>
                                 {
                                     {"Id", dataType.Id.ToString(CultureInfo.InvariantCulture)},
                                     {"Name", dataType.Name},
                                     {"UniqueId", dataType.Key.ToString()},
                                     {"CreateDate", dataType.CreateDate.ToString(CultureInfo.InvariantCulture)},
                                     {"ControlId", dataType.ControlId.ToString()},
                                     {"DatabaseType", dataType.DatabaseType.ToString()}
                                 };
            return dictionary;
        }

        public bool MoveContent(int id, int parentId)
        {
            var contentService = _serviceContext.ContentService;
            var content = contentService.GetById(id);
            if (content == null)
                return false;
            contentService.Move(content, parentId);
            return true;
        }

        public bool MoveContentToRecycleBin(int id)
        {
            var contentService = _serviceContext.ContentService;
            var content = contentService.GetById(id);
            if (content == null)
                return false;
            contentService.MoveToRecycleBin(content);
            return true;
        }

        public bool DeleteContent(int id)
        {
            var contentService = _serviceContext.ContentService;
            var content = contentService.GetById(id);
            if (content == null)
                return false;

            var xml = content.ToXml();

            contentService.Delete(content);
            return true;
        }

        public void RenameContent(int nodeId, string newName)
        {
            var contentService = _serviceContext.ContentService;
            var content = contentService.GetById(nodeId);
            if (content == null)
                return;

            content.Name = newName;
            contentService.Save(content);
        }

        public void SaveXml(string nodeType, int nodeId, string projectAppDataPath)
        {
            if (nodeType.Equals("contentTypes"))
            {
                var contentType = _serviceContext.ContentTypeService.GetContentType(nodeId);
                var xml = _serviceContext.PackagingService.Export(contentType);

                var fileName = string.Concat(nodeType.TrimEnd('s'), "-", nodeId);
                var filePath = Path.Combine(projectAppDataPath, fileName);
                xml.Save(filePath);
            }
        }

        private Dictionary<int, Dictionary<string, string>> ConvertEntitiesToDictionary(IEnumerable<IUmbracoEntity> entities)
        {
            var dictionary = new Dictionary<int, Dictionary<string, string>>();
            foreach (var entity in entities)
            {
                var innerDic = new Dictionary<string, string>
                                   {
                                       {"CreateDate", entity.CreateDate.ToString(CultureInfo.InvariantCulture)},
                                       {"CreatorId", entity.CreatorId.ToString(CultureInfo.InvariantCulture)},
                                       {"Key", entity.Key.ToString()},
                                       {"Level", entity.Level.ToString(CultureInfo.InvariantCulture)},
                                       {"Name", entity.Name},
                                       {"ParentId", entity.ParentId.ToString(CultureInfo.InvariantCulture)},
                                       {"Path", entity.Path},
                                       {"SortOrder", entity.SortOrder.ToString(CultureInfo.InvariantCulture)},
                                       {"Trashed", entity.Trashed.ToString()},
                                       {"UpdateDate", entity.UpdateDate.ToString(CultureInfo.InvariantCulture)}
                                   };

                dictionary.Add(entity.Id, innerDic);
            }
            return dictionary;
        }

        private string ReturnYesNo(bool isAyes)
        {
            return isAyes ? "Yes" : "No";
        }

        public void Dispose()
        {
            _manager.Dispose();
        }
    }
}