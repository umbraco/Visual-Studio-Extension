using System;
using System.ComponentModel;

namespace Umbraco.UmbracoStudio.Models
{
    [DefaultProperty("Type")]
    public class MediaTypeModel : IPropertiesModel
    {
        [Category("Meta Data")]
        [ReadOnly(true)]
        public string Name { get; set; }

        [Category("Meta Data")]
        [ReadOnly(true)]
        public string Alias { get; set; }

        [Category("Meta Data")]
        [ReadOnly(true)]
        public string Type { get; set; }

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

        [ReadOnly(true)]
        [DisplayName("Created Date")]
        public string Created { get; set; }
    }
}