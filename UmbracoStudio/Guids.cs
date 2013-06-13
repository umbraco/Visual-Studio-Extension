// Guids.cs
// MUST match guids.h
using System;

namespace Umbraco.UmbracoStudio
{
    static class GuidList
    {
        public const string guidUmbracoStudioPkgString = "44c37488-2d70-4227-8d04-746ad0a477fc";
        public const string guidUmbracoStudioCmdSetString = "3d1654fc-5dbc-4306-a23c-19f9612f1fab";
        public const string guidToolWindowPersistanceString = "65f6549a-5a58-4ae4-9b48-4b6382898b95";

        public static readonly Guid guidSEPlusCmdSet = new Guid("6b3f3d9e-9613-4d31-a10a-a1c050e83b8a");
        public static readonly Guid guidUmbracoStudioCmdSet = new Guid(guidUmbracoStudioCmdSetString);
    };
}