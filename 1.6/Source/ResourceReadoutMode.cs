using System.Collections.Generic;
using Verse;

namespace CustomResourceReadout
{
    public class ResourceReadoutMode : IExposable, ILoadReferenceable, IRenameable
    {
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
    }
}
