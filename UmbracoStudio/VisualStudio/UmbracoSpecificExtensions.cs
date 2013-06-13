using System;
using System.IO;
using System.Linq;
using System.Xml.Linq;
using EnvDTE;
using VSLangProj;

namespace Umbraco.UmbracoStudio.VisualStudio
{
    public static class UmbracoSpecificExtensions
    {
         public static bool IsUmbracoWebsite(this Project project)
         {
             References references;
             try
             {
                 references = project.Object.References;
             }
             catch (Microsoft.CSharp.RuntimeBinder.RuntimeBinderException)
             {
                 //References property doesn't exist, project does not have references
                 references = null;
             }

             if (references != null)
             {
                 return
                     references.Cast<Reference>()
                               .Any(reference =>
                                   reference.Name.Equals("Umbraco.Core", StringComparison.InvariantCultureIgnoreCase));
             }

             return false;
         }

        public static bool IsDatabaseConfigured(this Project project)
        {
            var fullPath = project.GetFullPath();
            var configFile = project.GetConfigurationFile();
            var configFilePath = Path.Combine(fullPath, configFile);

            if (File.Exists(configFilePath))
            {
                var configuration = XDocument.Load(configFilePath).Root;
                var connectionString = (from connectionStrings in configuration.Elements("connectionStrings")
                                        from connection in connectionStrings.Elements()
                                        where connection.Name.LocalName == "add" && (string) connection.Attribute("name") == "umbracoDbDSN"
                                        select connection).FirstOrDefault();

                if (connectionString != null && 
                    connectionString.Attribute("connectionString") != null &&
                    string.IsNullOrEmpty((string)connectionString.Attribute("connectionString")) == false &&
                    connectionString.Attribute("providerName") != null &&
                    string.IsNullOrEmpty((string)connectionString.Attribute("providerName")) == false)
                    return true;
            }

            return false;
        }

        public static string GetConnectionString(this Project project)
        {
            var fullPath = project.GetFullPath();
            var configFile = project.GetConfigurationFile();
            var configFilePath = Path.Combine(fullPath, configFile);

            if (File.Exists(configFilePath))
            {
                var configuration = XDocument.Load(configFilePath).Root;
                var connectionString = (from connectionStrings in configuration.Elements("connectionStrings")
                                        from connection in connectionStrings.Elements()
                                        where connection.Name.LocalName == "add" && (string)connection.Attribute("name") == "umbracoDbDSN"
                                        select connection).FirstOrDefault();

                if (connectionString != null &&
                    connectionString.Attribute("connectionString") != null &&
                    string.IsNullOrEmpty((string)connectionString.Attribute("connectionString")) == false)
                    return connectionString.Attribute("connectionString").Value;
            }

            return string.Empty;
        }

        public static string GetProviderName(this Project project)
        {
            var fullPath = project.GetFullPath();
            var configFile = project.GetConfigurationFile();
            var configFilePath = Path.Combine(fullPath, configFile);

            if (File.Exists(configFilePath))
            {
                var configuration = XDocument.Load(configFilePath).Root;
                var connectionString = (from connectionStrings in configuration.Elements("connectionStrings")
                                        from connection in connectionStrings.Elements()
                                        where connection.Name.LocalName == "add" && (string)connection.Attribute("name") == "umbracoDbDSN"
                                        select connection).FirstOrDefault();

                if (connectionString != null &&
                    connectionString.Attribute("providerName") != null &&
                    string.IsNullOrEmpty((string)connectionString.Attribute("providerName")) == false)
                    return connectionString.Attribute("providerName").Value;
            }
            return string.Empty;
        }
    }
}