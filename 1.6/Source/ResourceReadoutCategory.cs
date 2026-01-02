using RimWorld;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Verse;
using Verse.Sound;

namespace CustomResourceReadout
{
    public class ResourceReadoutCategory : IResourceReadoutItem, IExposable
    {
        private bool expanded;
        private bool uiExpanded;
        private string iconPath;
        private Texture2D icon;
        public List<IResourceReadoutItem> items = new List<IResourceReadoutItem>();

        public IEnumerable<ThingDef> ThingDefs => items.SelectMany(i => i.ThingDefs);

        public Texture2D Icon
        {
            get
            {
                if (icon == null)
                {
                    icon = ContentFinder<Texture2D>.Get(iconPath);
                }
                return icon;
            }
        }

        public ResourceReadoutCategory() { }

        public ResourceReadoutCategory(string iconPath) : this()
        {
            this.iconPath = iconPath;
        }

        public ResourceReadoutCategory(ThingCategoryDef category) : this(category.iconPath)
        {
            foreach (ThingCategoryDef childCategory in category.childCategories)
            {
                items.Add(new ResourceReadoutCategory(childCategory));
            }
            foreach (ThingDef def in category.childThingDefs)
            {
                items.Add(new ResourceReadoutLeaf(def));
            }
        }

        public float DoSettingsInterface(Rect rect)
        {
            rect.height = 24f;
            Rect expandRect = rect.LeftPartPixels(rect.height);
            if (Widgets.ButtonImage(expandRect.ContractedBy(2f), uiExpanded ? TexButton.Collapse : TexButton.Reveal))
            {
                uiExpanded = !uiExpanded;
                (uiExpanded ? SoundDefOf.TabOpen : SoundDefOf.TabClose).PlayOneShot(null);
            }
            Rect iconRect = expandRect;
            iconRect.x += iconRect.width;
            GUI.DrawTexture(iconRect.ContractedBy(1f), Icon);
            Rect labelRect = rect.RightPartPixels(rect.width - iconRect.xMax);
            using (new TextBlock(TextAnchor.MiddleLeft)) Widgets.Label(labelRect, "CustomResourceReadout_ItemsInCategory".Translate(items.Count));
            if (uiExpanded)
            {
                rect.xMin += 24f;
                rect.y += rect.height;
                rect.height = 0f;
                foreach (IResourceReadoutItem item in items)
                {
                    rect.y += item.DoSettingsInterface(rect);
                }
            }
            return rect.y + rect.height;
        }

        public void ExposeData()
        {
            Scribe_Values.Look(ref expanded, "expanded");
            Scribe_Values.Look(ref iconPath, "iconPath");
            Scribe_Collections.Look(ref items, "items", LookMode.Deep);
            if (Scribe.mode == LoadSaveMode.PostLoadInit && items == null)
            {
                items = new List<IResourceReadoutItem>();
            }
        }
    }
}
