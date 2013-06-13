using System;
using EnvDTE;

namespace Umbraco.UmbracoStudio.VisualStudio
{
    public class ProjectEventArgs : EventArgs
    {
        public ProjectEventArgs(Project project)
        {
            Project = project;
        }

        public Project Project { get; private set; }
    }
}