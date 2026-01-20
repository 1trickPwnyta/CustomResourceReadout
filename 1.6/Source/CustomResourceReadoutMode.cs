using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using UnityEngine;
using Verse;

namespace CustomResourceReadout
{
    public class CustomResourceReadoutMode : IResourceReadoutMode, IExposable, ILoadReferenceable, IRenameable
    {
        public static CustomResourceReadoutMode FromSimple()
        {
            CustomResourceReadoutMode mode = new CustomResourceReadoutMode();
            foreach (ThingDef def in DefDatabase<ThingDef>.AllDefsListForReading.Where(d => d.CountAsResource).OrderByDescending(d => d.resourceReadoutPriority))
            {
                mode.items.Add(new ResourceReadoutLeaf(def, null) { alwaysShow = def.resourceReadoutAlwaysShow });
            }
            return mode;
        }

        public static CustomResourceReadoutMode FromCategorized()
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

            CustomResourceReadoutMode mode = new CustomResourceReadoutMode();
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

        public static CustomResourceReadoutMode FromDef(ResourceReadoutModeDef def) => new CustomResourceReadoutMode(def.label) { items = def.Items };

        public string name;
        private List<ResourceReadoutItem> items = new List<ResourceReadoutItem>();

        public CustomResourceReadoutMode() { }

        public CustomResourceReadoutMode(string name) : this()
        {
            this.name = name;
        }

        public string RenamableLabel { get => name; set => name = value; }

        public string BaseLabel => name;

        public string InspectLabel => name;

        public List<ResourceReadoutItem> Items => items;

        public string Name => name;

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

        public CustomResourceReadoutMode Copy()
        {
            CustomResourceReadoutMode copy = new CustomResourceReadoutMode();
            foreach (ResourceReadoutItem item in items)
            {
                copy.items.Add(item.Copy());
            }
            return copy;
        }
    }
}
