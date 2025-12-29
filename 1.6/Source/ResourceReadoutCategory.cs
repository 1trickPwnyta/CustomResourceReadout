using System;
using UnityEngine;
using Verse;

namespace CustomResourceReadout
{
    public class ResourceReadoutCategory : IResourceReadoutItem, IExposable
    {
        public Texture2D Icon => throw new NotImplementedException();

        public float DoSettingsInterface(Rect rect)
        {
            throw new NotImplementedException();
        }

        public void ExposeData()
        {
            throw new NotImplementedException();
        }
    }
}
