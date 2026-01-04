using System.Collections.Generic;
using UnityEngine;
using Verse;

namespace CustomResourceReadout
{
    public interface IResourceReadoutItem
    {
        IEnumerable<ThingDef> ThingDefs { get; }

        Texture2D Icon { get; }

        float DoSettingsInterface(Rect rect, ResourceReadoutCategory parentCategory = null);
    }
}
