using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Threading;
using Umbraco.UmbracoStudio.Models;
using Umbraco.VisualStudio;

namespace Umbraco.UmbracoStudio.ExternalApplication
{
    public class UmbracoApplicationContext : IDisposable
    {
        private const string AssemblyFileName = "Umbraco.VisualStudio.dll";
        private readonly string _projectAppDataFolder;
        private readonly string _projectBinFolder;
        private readonly string _projectFolder;
        private readonly string _configurationFile;
        private readonly string _connectionString;
        private readonly string _providerName;
        private ServiceBridge _bridge;
        private Dictionary<string, Func<Dictionary<int, Dictionary<string, string>>>> _methods;
        private Dictionary<string, Func<int, Dictionary<int, Dictionary<string, string>>>> _methodsById;
        private Dictionary<string, Func<int, bool>> _deleteMethods;
        private Dictionary<string, Func<int, bool>> _trashMethods;
        private Dictionary<string, Func<int, int, bool>> _moveMethods;
        private Dictionary<string, Func<int, IPropertiesModel>> _propertiesModels;

        internal UmbracoApplicationContext(string projectFolder, string connectionString, string providerName)
        {
            _projectFolder = projectFolder;
            _projectAppDataFolder = Path.Combine(projectFolder, "App_Data");
            _projectBinFolder = Path.Combine(projectFolder, "bin");
            _configurationFile = Path.Combine(projectFolder, "Web.config");
            _connectionString = connectionString.Replace("|DataDirectory|", _projectAppDataFolder);
            _providerName = providerName;
        }

        /// <summary>
        /// Singleton accessor
        /// </summary>
        public static UmbracoApplicationContext Current { get; internal set; }

        bool _isReady = false;
        private readonly ManualResetEventSlim _isReadyEvent = new ManualResetEventSlim(false);

        public bool IsReady
        {
            get
            {
                return _isReady;
            }
            internal set
            {
                AssertIsNotReady();
                _isReady = value;
                _isReadyEvent.Set();
            }
        }

        public void StartApplication()
        {
            //Ensure that the Umbraco.VisualStudio.dll is available in the solution's bin folder
            var mainAssemblyDestinationPath = Path.Combine(_projectBinFolder, AssemblyFileName);
            if (File.Exists(mainAssemblyDestinationPath) == false)
            {
                var mainAssemblySourcePath = GetAssemblyLocation(AssemblyFileName);
                File.Copy(mainAssemblySourcePath, mainAssemblyDestinationPath);
            }
            //Load the ServiceBridge in a sandboxed appdomain using the directory/context of the solution
            var assemblyLocation = Path.Combine(_projectBinFolder, "Umbraco.Core.dll");
            ServiceBridge loader = ServiceBridge.Sandbox(assemblyLocation, _configurationFile, CurrentDomain_AssemblyResolve);
            _bridge = loader;
            if (_bridge == null)
                return;

            _bridge.Configure(_connectionString, _providerName, _projectFolder);

            SetupMethodShortcuts();

            IsReady = true;
        }

        public Assembly CurrentDomain_AssemblyResolve(object sender, ResolveEventArgs args)
        {
            Trace.WriteLine("Resolving assembly using current location of the ServiceBridge assembly");
            return typeof(ServiceBridge).Assembly;
        }

        public ServiceBridge Bridge
        {
            get { return _bridge; }
        }

        /// <summary>
        /// Gets a list of root items by their Type
        /// </summary>
        /// <param name="nodeType"></param>
        /// <returns></returns>
        public Dictionary<int, Dictionary<string, string>> GetRootByType(string nodeType)
        {
            return _methods[nodeType].Invoke();
        }

        /// <summary>
        /// Gets a list of child items by their Type and parent id
        /// </summary>
        /// <param name="nodeType"></param>
        /// <param name="id"></param>
        /// <returns></returns>
        public Dictionary<int, Dictionary<string, string>> GetChildrenByType(string nodeType, int id)
        {
            return _methodsById[nodeType].Invoke(id);
        }

        /// <summary>
        /// Gets a model based on the IPropertiesModel interface by its NodeType
        /// and integer Id.
        /// </summary>
        /// <param name="nodeType"></param>
        /// <param name="id"></param>
        /// <returns></returns>
        public IPropertiesModel GetPropertiesModel(string nodeType, int id)
        {
            return _propertiesModels[nodeType].Invoke(id);
        }

        /// <summary>
        /// Gets a ContentModel by its Id.
        /// Used for showing the Properties Frame.
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public ContentModel GetContentModelById(int id)
        {
            var properties = _bridge.GetContentProperties(id);
            var model = new ContentModel
                            {
                                Id = id,
                                Type = "Content",
                                ContentType = properties["ContentTypeName"],
                                ContentTypeAlias = properties["ContentTypeAlias"],
                                Created = properties["CreateDate"],
                                Level = int.Parse(properties["Level"]),
                                Name = properties["Name"],
                                ParentId = int.Parse(properties["ParentId"]),
                                Path = properties["Path"],
                                Published = properties["Published"],
                                SortOrder = int.Parse(properties["SortOrder"]),
                                Template = properties["TemplateAlias"],
                                UniqueId = new Guid(properties["UniqueId"]),
                                Updated = properties["UpdateDate"],
                                Version = new Guid(properties["Version"])
                            };
            return model;
        }

        /// <summary>
        /// Gets a MediaModel by its Id.
        /// Used for showing the Properties Frame.
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public MediaModel GetMediaModelById(int id)
        {
            var properties = _bridge.GetMediaProperties(id);
            var model = new MediaModel
                            {
                                Id = id,
                                Type = "Media",
                                MediaType = properties["ContentTypeName"],
                                Created = properties["CreateDate"],
                                Level = int.Parse(properties["Level"]),
                                Name = properties["Name"],
                                ParentId = int.Parse(properties["ParentId"]),
                                Path = properties["Path"],
                                SortOrder = int.Parse(properties["SortOrder"]),
                                UniqueId = new Guid(properties["UniqueId"]),
                                Updated = properties["UpdateDate"]
                            };
            return model;
        }

        /// <summary>
        /// Gets a ContentTypeModel by its Id.
        /// Used for showing the Properties Frame.
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public ContentTypeModel GetContentTypeModelById(int id)
        {
            var properties = _bridge.GetContentTypeProperties(id);
            var model = new ContentTypeModel
                            {
                                Id = id,
                                Type = "ContentType",
                                Alias = properties["Alias"],
                                Created = properties["CreateDate"],
                                Level = int.Parse(properties["Level"]),
                                Name = properties["Name"],
                                ParentId = int.Parse(properties["ParentId"]),
                                Path = properties["Path"],
                                SortOrder = int.Parse(properties["SortOrder"]),
                                Template = properties["TemplateAlias"],
                                UniqueId = new Guid(properties["UniqueId"])
                            };
            return model;
        }

        /// <summary>
        /// Gets a MediaTypeModel by its Id.
        /// Used for showing the Properties Frame.
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public MediaTypeModel GetMediaTypeModelById(int id)
        {
            var properties = _bridge.GetMediaTypeProperties(id);
            var model = new MediaTypeModel
                            {
                                Id = id,
                                Type = "MediaType",
                                Alias = properties["Alias"],
                                Created = properties["CreateDate"],
                                Level = int.Parse(properties["Level"]),
                                Name = properties["Name"],
                                ParentId = int.Parse(properties["ParentId"]),
                                Path = properties["Path"],
                                SortOrder = int.Parse(properties["SortOrder"]),
                                UniqueId = new Guid(properties["UniqueId"])
                            };
            return model;
        }

        /// <summary>
        /// Gets a DataTypeModel by its Id.
        /// Used for showing the Properties Frame.
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public DataTypeModel GetDataTypeModelById(int id)
        {
            var properties = _bridge.GetDataTypeProperties(id);
            var model = new DataTypeModel
                            {
                                Id = id,
                                Type = "DataType",
                                Created = properties["CreateDate"],
                                Name = properties["Name"],
                                UniqueId = new Guid(properties["UniqueId"]),
                                ControlId = new Guid(properties["ControlId"]),
                                DatabaseType = properties["DatabaseType"]
                            };
            return model;
        }

        /// <summary>
        /// Invokes a Delete action for an item by its Id and Type
        /// </summary>
        /// <param name="treeType"></param>
        /// <param name="id"></param>
        /// <returns></returns>
        public bool Delete(string treeType, int id)
        {
            //TODO Check if tree type exists in dictionary
            return _deleteMethods[treeType].Invoke(id);
        }

        /// <summary>
        /// Invokes a Move action for an item by its Id and Type
        /// </summary>
        /// <param name="treeType"></param>
        /// <param name="id"></param>
        /// <param name="parentId"></param>
        /// <returns></returns>
        public bool Move(string treeType, int id, int parentId)
        {
            //TODO Check if tree type exists in dictionary
            return _moveMethods[treeType].Invoke(id, parentId);
        }

        /// <summary>
        /// Invokes a 'Move to Recycle Bin' action for an item by its Id and Type
        /// </summary>
        /// <param name="treeType"></param>
        /// <param name="id"></param>
        /// <returns></returns>
        public bool Trash(string treeType, int id)
        {
            //TODO Check if tree type exists in dictionary
            return _trashMethods[treeType].Invoke(id);
        }

        /// <summary>
        /// Invokes a 'Rename' action for an item by its Id and Type
        /// </summary>
        /// <param name="nodeType"></param>
        /// <param name="newName"></param>
        /// <param name="nodeId"></param>
        public void Rename(string nodeType, string newName, int nodeId)
        {
            if (nodeType.Equals("content"))
            {
                _bridge.RenameContent(nodeId, newName);
            }
            else if (nodeType.Equals("media"))
            {
                _bridge.RenameMedia(nodeId, newName);
            }
        }

        /// <summary>
        /// Invokes the xml export of an item by its Id and Type
        /// </summary>
        /// <param name="nodeType"></param>
        /// <param name="nodeId"></param>
        public void SerializeNode(string nodeType, int nodeId)
        {
            _bridge.ExportXml(nodeType, nodeId, _projectAppDataFolder);
        }

        public void DeserializeNode(string nodeType, int nodeId, string filePath)
        {
            _bridge.ImportXml(nodeType, nodeId, filePath);
        }

        public void Dispose()
        {
            _bridge.Dispose();
        }

        private void SetupMethodShortcuts()
        {
            _methods = new Dictionary<string, Func<Dictionary<int, Dictionary<string, string>>>>
                           {
                               {"content", _bridge.GetRootContent},
                               {"media", _bridge.GetRootMedia},
                               {"contentTypes", _bridge.GetRootContentTypes},
                               {"mediaTypes", _bridge.GetRootMediaTypes},
                               {"dataTypes", _bridge.GetRootDataTypes}
                           };

            _methodsById = new Dictionary<string, Func<int, Dictionary<int, Dictionary<string, string>>>>
                               {
                                   {"content", _bridge.GetChildContent},
                                   {"media", _bridge.GetChildMedia},
                                   {"contentTypes", _bridge.GetChildContentTypes},
                                   {"mediaTypes", _bridge.GetChildMediaTypes},
                                   {"dataTypes", _bridge.GetChildDataTypes}
                               };

            _propertiesModels = new Dictionary<string, Func<int, IPropertiesModel>>
                                    {
                                        {"content", GetContentModelById},
                                        {"media", GetMediaModelById},
                                        {"contentTypes", GetContentTypeModelById},
                                        {"mediaTypes", GetMediaTypeModelById},
                                        {"dataTypes", GetDataTypeModelById}
                                    };

            _deleteMethods = new Dictionary<string, Func<int, bool>>
                                 {
                                     {"content", _bridge.DeleteContent},
                                     {"media", _bridge.DeleteMedia}
                                 };

            _trashMethods = new Dictionary<string, Func<int, bool>>
                                {
                                    {"content", _bridge.MoveContentToRecycleBin},
                                    {"media", _bridge.MoveMediaToRecycleBin}
                                };

            _moveMethods = new Dictionary<string, Func<int, int, bool>>
                               {
                                   {"content", _bridge.MoveContent}
                               };
        }

        private string GetAssemblyLocation(string assemblyFileName)
        {
            var codeBase = Assembly.GetExecutingAssembly().CodeBase;
            var uri = new Uri(codeBase);
            var path = uri.LocalPath;
            var baseDirectory = Path.GetDirectoryName(path);
            return Path.Combine(baseDirectory, assemblyFileName);
        }

        private void AssertIsNotReady()
        {
            if (this.IsReady)
                throw new Exception("UmbracoApplicationContext has already been initialized.");
        }
    }
}