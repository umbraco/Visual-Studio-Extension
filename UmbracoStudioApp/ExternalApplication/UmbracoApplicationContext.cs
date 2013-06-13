using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Reflection;
using System.Security;
using System.Security.Permissions;
using System.Security.Policy;
using Umbraco.VisualStudio;

namespace UmbracoStudioApp.ExternalApplication
{
    public class UmbracoApplicationContext : IDisposable
    {
        private const string AppDomainName = "UmbracoApplicationDomain";
        private const string AssemblyFileName = "Umbraco.VisualStudio.dll";
        private readonly string _projectAppDataFolder;
        private readonly string _projectFolder;
        private readonly string _projectBinFolder;
        private readonly string _configurationFile;
        private string _connectionString;
        private string _providerName;
        private ServiceBridge _bridge;
        private Dictionary<string, Func<Dictionary<int, Dictionary<string, string>>>> _methods;
        private Dictionary<string, Func<int, Dictionary<int, Dictionary<string, string>>>> _methodsById;

        internal UmbracoApplicationContext(string projectFolder)
        {
            _projectFolder = projectFolder;
            _projectAppDataFolder = Path.Combine(projectFolder, "App_Data");
            _projectBinFolder = Path.Combine(projectFolder, "bin");
            _configurationFile = Path.Combine(projectFolder, "Web.config");
        }

        /// <summary>
        /// Singleton accessor
        /// </summary>
        public static UmbracoApplicationContext Current { get; internal set; }

        bool _isReady = false;
        readonly System.Threading.ManualResetEventSlim _isReadyEvent = new System.Threading.ManualResetEventSlim(false);

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
            //var domainSetup = new AppDomainSetup()
            //                      {
            //                          ApplicationBase = _projectBinFolder,
            //                          ConfigurationFile = _configurationFile,
            //                          ApplicationName =
            //                              AppDomain.CurrentDomain.SetupInformation.ApplicationName,
            //                          LoaderOptimization = LoaderOptimization.MultiDomainHost
            //                      };
            //var baseEvidence = AppDomain.CurrentDomain.Evidence;
            //var evidence = new Evidence(baseEvidence);
            //var childDomain = AppDomain.CreateDomain(AppDomainName, evidence, domainSetup);
            //childDomain.SetData("DataDirectory", _projectAppDataFolder);

            //var assemblyLocation = Path.Combine(_projectBinFolder, AssemblyFileName); //GetAssemblyLocation(AssemblyFileName);
            //var proxy = childDomain.CreateInstanceFromAndUnwrap(assemblyLocation, typeof(ServiceBridge).FullName);
            //_bridge = proxy as IServiceBridge;

            //Evidence adevidence = AppDomain.CurrentDomain.Evidence;
            //var newAppDomain = AppDomain.CreateDomain(AppDomainName, adevidence, _projectBinFolder, _projectBinFolder, true);
            //newAppDomain.SetData("DataDirectory", _projectAppDataFolder);
            //var assemblyLocation = Path.Combine(_projectBinFolder, AssemblyFileName);
            ////var unwrapped = newAppDomain.CreateInstanceFromAndUnwrap(assemblyLocation, typeof(ServiceBridge).FullName);
            //var unwrapped = newAppDomain.CreateInstanceFromAndUnwrap(assemblyLocation, typeof(ServiceBridge).FullName, false, BindingFlags.Default, null, null, CultureInfo.InvariantCulture, null);
            //_bridge = unwrapped as ServiceBridge;

            var assemblyLocation = /*GetAssemblyLocation(AssemblyFileName);*/ Path.Combine(_projectBinFolder, "Umbraco.Core.dll");
            ServiceBridge loader = ServiceBridge.Sandbox(assemblyLocation, _configurationFile, CurrentDomain_AssemblyResolve);
            _bridge = loader;
            if (_bridge == null)
                return;

            _connectionString = @"Data Source=C:\Temp\Playground\Umb610TestSiteVsPlugin\Umb610TestSiteVsPlugin\App_Data\Umbraco.sdf";
            _providerName = "System.Data.SqlServerCe.4.0";
            _bridge.Configure(_connectionString, _providerName, _projectFolder);

            _methods = new Dictionary<string, Func<Dictionary<int, Dictionary<string, string>>>>
                           {
                               {"content", _bridge.GetRootContent},
                               {"media", _bridge.GetRootMedia},
                               {"contentTypes", _bridge.GetRootContentTypes},
                               {"mediaTypes", _bridge.GetRootMediaTypes}
                           };

            _methodsById = new Dictionary<string, Func<int, Dictionary<int, Dictionary<string, string>>>>
                               {
                                   {"content", _bridge.GetChildContent},
                                   {"media", _bridge.GetChildMedia},
                                   {"contentTypes", _bridge.GetChildContentTypes},
                                   {"mediaTypes", _bridge.GetChildMediaTypes}
                               };

            IsReady = true;
        }

        public ServiceBridge Bridge
        {
            get { return _bridge; }
        }

        public Assembly CurrentDomain_AssemblyResolve(object sender, ResolveEventArgs args)
        {
            Trace.WriteLine("Resolving assembly using current location of the ServiceBridge assembly");
            return typeof(ServiceBridge).Assembly;
        }

        public string GetAssemblyLocation(string assemblyFileName)
        {
            var codeBase = Assembly.GetExecutingAssembly().CodeBase;
            var uri = new Uri(codeBase);
            var path = uri.LocalPath;
            var baseDirectory = Path.GetDirectoryName(path);
            return Path.Combine(baseDirectory, assemblyFileName);
        }

        public Dictionary<int, Dictionary<string, string>> GetRootByType(string treeType)
        {
            return _methods[treeType].Invoke();
        }

        public Dictionary<int, Dictionary<string, string>> GetChildrenByType(string treeType, int id)
        {
            return _methodsById[treeType].Invoke(id);
        }

        public void Dispose()
        {
            _bridge.Dispose();
        }

        private void AssertIsNotReady()
        {
            if (this.IsReady)
                throw new Exception("UmbracoApplicationContext has already been initialized.");
        }
    }
}