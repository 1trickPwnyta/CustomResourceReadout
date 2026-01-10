using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using UnityEngine;
using Verse;

namespace CustomResourceReadout
{
    public class ResourceReadoutMode : IExposable, ILoadReferenceable, IRenameable
    {
        public static ResourceReadoutMode FromSimple()
        {
            ResourceReadoutMode mode = new ResourceReadoutMode();
            foreach (ThingDef def in DefDatabase<ThingDef>.AllDefsListForReading.Where(d => d.CountAsResource).OrderByDescending(d => d.resourceReadoutPriority))
            {
                mode.items.Add(new ResourceReadoutLeaf(def, null));
            }
            return mode;
        }

        public static ResourceReadoutMode FromCategorized()
        {
            bool PopulateCategory(ThingCategoryDef def, ResourceReadoutCategory category)
            {
                foreach (ThingCategoryDef subDef in def.childCategories.Where(c => !c.resourceReadoutRoot))
                {
                    ResourceReadoutCategory subCategory = new ResourceReadoutCategory(subDef.iconPath, Color.white, category);
                    if (PopulateCategory(subDef, subCategory))
                    {
                        category.items.Add(subCategory);
                    }
                }
                foreach (ThingDef subDef in def.childThingDefs.Where(t => t.PlayerAcquirable && t.resourceReadoutPriority > ResourceCountPriority.Uncounted))
                {
                    category.items.Add(new ResourceReadoutLeaf(subDef, category));
                }
                return category.items.Any();
            }

            ResourceReadoutMode mode = new ResourceReadoutMode();
            foreach (ThingCategoryDef def in DefDatabase<ThingCategoryDef>.AllDefsListForReading.Where(d => d.resourceReadoutRoot))
            {
                ResourceReadoutCategory category = new ResourceReadoutCategory(def.iconPath, Color.white, null);
                if (PopulateCategory(def, category))
                {
                    mode.items.Add(category);
                }
            }
            return mode;
        }

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
