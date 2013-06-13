using System;
using System.ComponentModel;

namespace Umbraco.UmbracoStudio.Models
{
    [DefaultProperty("Type")]
    public class MediaModel : IPropertiesModel
    {
        [Category("Meta Data")]
        [ReadOnly(true)]
        public string Name { get; set; }

        [Category("Meta Data")]
        [ReadOnly(true)]
        public string Type { get; set; }

        [Category("Meta Data")]
        [ReadOnly(true)]
        [DisplayName("MediaType Name")]
        public string MediaType { get; set; }

        [Category("Meta Data")]
        [ReadOnly(true)]
        [DisplayName("MediaType Alias")]
        public string MediaTypeAlias { get; set; }

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
        [DisplayName("Created Date")]
        public string Created { get; set; }

        [Category("Properties")]
        [ReadOnly(true)]
        [DisplayName("Updated Date")]
        public string Updated { get; set; }
    }
}