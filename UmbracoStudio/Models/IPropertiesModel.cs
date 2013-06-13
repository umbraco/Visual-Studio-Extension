using System.ComponentModel;

namespace Umbraco.UmbracoStudio.Models
{
    public interface IPropertiesModel
    {
        [Category("Meta Data")]
        [ReadOnly(true)]
        string Name { get; set; }

        [Category("Meta Data")]
        [ReadOnly(true)]
        string Type { get; set; }
    }
}