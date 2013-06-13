using System;
using System.ComponentModel;

namespace Umbraco.UmbracoStudio.Models
{
    [DefaultProperty("Type")]
    public class DataTypeModel : IPropertiesModel
    {
        [Category("Meta Data")]
        [ReadOnly(true)]
        public string Name { get; set; }

        [Category("Meta Data")]
        [ReadOnly(true)]
        public string Type { get; set; }

        [Category("Properties")]
        [ReadOnly(true)]
        public int Id { get; set; }

        [Category("Properties")]
        [ReadOnly(true)]
        public Guid UniqueId { get; set; }

        [Category("Properties")]
        [ReadOnly(true)]
        public Guid ControlId { get; set; }

        [Category("Properties")]
        [ReadOnly(true)]
        public string DatabaseType { get; set; }

        [ReadOnly(true)]
        [DisplayName("Created Date")]
        public string Created { get; set; }
    }
}