using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using Verse;

namespace CustomResourceReadout
{
    public class ResourceReadoutMode : IExposable, ILoadReferenceable, IRenameable
    {
        public static ResourceReadoutMode FromDef(ResourceReadoutModeDef def) => new ResourceReadoutMode(def.label) { items = def.items };

        public string name;
        public List<ResourceReadoutItem> items = new List<ResourceReadoutItem>();

        public ResourceReadoutMode() { }

        public ResourceReadoutMode(string name) : this()
        {
            this.name = name;
        }

        public string RenamableLabel { get => name; set => name = value; }

        public string BaseLabel => name;

        public string InspectLabel => name;

        public void ExposeData()
        {
            Scribe_Values.Look(ref name, "name");
            Scribe_Collections.Look(ref items, "items", LookMode.Deep);
            if (Scribe.mode == LoadSaveMode.PostLoadInit && items == null)
            {
                items = new List<ResourceReadoutItem>();
            }
        }

        public string GetUniqueLoadID() => "ResourceReadoutMode_" + name;

        public ResourceReadoutModeDef ToDef() => new ResourceReadoutModeDef()
        {
            defName = Regex.Replace(name, "\\s+", "") + "_" + new DateTimeOffset(DateTime.UtcNow).ToUnixTimeMilliseconds(),
            label = name,
            items = items
        };
    }
}
