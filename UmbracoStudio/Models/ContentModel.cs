using System;
using System.ComponentModel;

namespace Umbraco.UmbracoStudio.Models
{
    [DefaultProperty("Type")]
    public class ContentModel : IPropertiesModel
    {
        [Category("Meta Data")]
        [ReadOnly(true)]
        public string Name { get; set; }

        [Category("Meta Data")]
        [ReadOnly(true)]
        public string Type { get; set; }

        [Category("Meta Data")]
        [ReadOnly(true)]
        [DisplayName("ContentType Name")]
        public string ContentType { get; set; }

        [Category("Meta Data")]
        [ReadOnly(true)]
        [DisplayName("ContentType Alias")]
        public string ContentTypeAlias { get; set; }

        [Category("Meta Data")]
        [ReadOnly(true)]
        public string Template { get; set; }

        [Category("Properties")]
        [ReadOnly(true)]
        public int Id { get; set; }

        [Category("Properties")]
        [ReadOnly(true)]
        public int ParentId { get; set; }

        [Category("Properties")]
        [ReadOnly(true)]
        public Guid UniqueId { get; set; }

        [Category("Properties")]
        [ReadOnly(true)]
        public int Level { get; set; }

        [Category("Properties")]
        [ReadOnly(true)]
        public int SortOrder { get; set; }

        [Category("Properties")]
        [ReadOnly(true)]
        public string Path { get; set; }

        [Category("Properties")]
        [ReadOnly(true)]
        public string Published { get; set; }

        [Category("Properties")]
        [ReadOnly(true)]
        public Guid Version { get; set; }

        [ReadOnly(true)]
        [DisplayName("Created Date")]
        public string Created { get; set; }

        [ReadOnly(true)]
        [DisplayName("Updated Date")]
        public string Updated { get; set; }
    }
}