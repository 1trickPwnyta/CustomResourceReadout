using System.Collections.Generic;

namespace CustomResourceReadout
{
    public interface IResourceReadoutMode
    {
        string Name { get; }

        List<ResourceReadoutItem> Items { get; }
    }
}
