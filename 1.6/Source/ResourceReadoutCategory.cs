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
        private Color iconColor;
        private Texture2D icon;
        public List<IResourceReadoutItem> items = new List<IResourceReadoutItem>();
        public IResourceReadoutItem deletedItem;

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

        public ResourceReadoutCategory(string iconPath, Color iconColor) : this()
        {
            this.iconPath = iconPath;
            this.iconColor = iconColor;
        }

        public ResourceReadoutCategory(ThingCategoryDef category) : this(category.iconPath, Color.white)
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

        public float DoSettingsInterface(Rect rect, ResourceReadoutCategory parentCategory = null)
        {
            rect.height = 24f;
            Widgets.DrawHighlightIfMouseover(rect);

            Rect expandRect = rect.LeftPartPixels(rect.height);
            if (Widgets.ButtonImage(expandRect.ContractedBy(2f), uiExpanded ? TexButton.Collapse : TexButton.Reveal))
            {
                uiExpanded = !uiExpanded;
                (uiExpanded ? SoundDefOf.TabOpen : SoundDefOf.TabClose).PlayOneShot(null);
            }
            Rect iconRect = expandRect;
            iconRect.x += iconRect.width;
            GUI.DrawTexture(iconRect.ContractedBy(1f), Icon, ScaleMode.ScaleToFit, true, 1f, iconColor, 0f, 0f);
            Rect labelRect = rect.RightPartPixels(rect.width - iconRect.width - expandRect.width);
            using (new TextBlock(TextAnchor.MiddleLeft)) Widgets.Label(labelRect, "CustomResourceReadout_ItemsInCategory".Translate(items.Count));
            
            if (Widgets.ButtonInvisible(rect))
            {
                Find.WindowStack.Add(new FloatMenu(new[]
                {
                    new FloatMenuOption("CustomResourceReadout_AddResources".Translate(), () => Find.WindowStack.Add(new Dialog_SelectThingDefs(items))),
                    new FloatMenuOption("CustomResourceReadout_AddCategories".Translate(), () => Find.WindowStack.Add(new Dialog_AddThingCategoryDefs(items))),
                    new FloatMenuOption("CustomResourceReadout_AddEmptyCategory".Translate(), () => Find.WindowStack.Add(new Dialog_SelectIcon("CustomResourceReadout_AddEmptyCategory".Translate(), (p, c) =>
                    {
                        items.Add(new ResourceReadoutCategory(p, c));
                    }))),
                    new FloatMenuOption("Delete".Translate(), () =>
                    {
                        if (parentCategory != null)
                        {
                            parentCategory.deletedItem = this;
                        }
                        else
                        {
                            CustomResourceReadoutSettings.deletedItem = this;
                        }
                    })
                }.ToList()));
            }

            float y = rect.y;
            if (uiExpanded)
            {
                rect.xMin += 24f;
                rect.y += rect.height;
                rect.height = 0f;
                foreach (IResourceReadoutItem item in items)
                {
                    rect.y += item.DoSettingsInterface(rect, this);
                }
                if (deletedItem != null)
                {
                    items.Remove(deletedItem);
                    deletedItem = null;
                }
            }

            return rect.yMax - y;
        }

        public void ExposeData()
        {
            Scribe_Values.Look(ref expanded, "expanded");
            Scribe_Values.Look(ref iconPath, "iconPath");
            Scribe_Values.Look(ref iconColor, "iconColor");
            Scribe_Collections.Look(ref items, "items", LookMode.Deep);
            if (Scribe.mode == LoadSaveMode.PostLoadInit && items == null)
            {
                items = new List<IResourceReadoutItem>();
            }
        }
    }
}
