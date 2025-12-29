using UnityEngine;

namespace CustomResourceReadout
{
    public interface IResourceReadoutItem
    {
        Texture2D Icon { get; }

        float DoSettingsInterface(Rect rect);
    }
}
