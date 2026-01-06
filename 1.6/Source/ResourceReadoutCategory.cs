using RimWorld;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Verse;
using Verse.Sound;

namespace CustomResourceReadout
{
    public class ResourceReadoutCategory : ResourceReadoutItem, IExposable
    {
        private bool expanded;
        private bool uiExpanded;
        private string iconPath;
        private Color iconColor;
        private Texture2D icon;
        public List<ResourceReadoutItem> items = new List<ResourceReadoutItem>();
        public ResourceReadoutItem deletedItem;

        public override IEnumerable<ThingDef> ThingDefs => items.SelectMany(i => i.ThingDefs);

        public Texture2D Icon
        {
            get
            {
                if (icon == null && iconPath != null)
                {
                    icon = ContentFinder<Texture2D>.Get(iconPath);
                }
                return icon;
            }
        }

        protected override IEnumerable<FloatMenuOption> FloatMenuOptions => new[]
        {
            new FloatMenuOption("CustomResourceReadout_AddResources".Translate(), () => Find.WindowStack.Add(new Dialog_SelectThingDefs(items))),
            new FloatMenuOption("CustomResourceReadout_AddCategories".Translate(), () => Find.WindowStack.Add(new Dialog_AddThingCategoryDefs(items))),
            new FloatMenuOption("CustomResourceReadout_AddEmptyCategory".Translate(), () => Find.WindowStack.Add(new Dialog_SelectIcon("CustomResourceReadout_AddEmptyCategory".Translate(), (p, c) =>
            {
                items.Add(new ResourceReadoutCategory(p, c));
            })))
            // TODO Allow changing the icon
            // TODO Allow searching icons
            // TODO Allow changing a leaf's stuff if other stuff is available for it
        };

        public override IEnumerable<ResourceReadoutItem> DraggableItems => uiExpanded ? base.DraggableItems.Concat(items.SelectMany(i => i.DraggableItems)) : base.DraggableItems;

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

        public bool Accepts(ResourceReadoutItem item)
        {
            if (item == this || parent?.Accepts(item) == false)
            {
                return false;
            }
            if (item is ResourceReadoutLeaf leaf && items.OfType<ResourceReadoutLeaf>().Any(l => l != leaf && l.Def == leaf.Def))
            {
                return false;
            }
            return true;
        }

        protected override void OnDragOver(Rect rect)
        {
            if (CustomResourceReadoutSettings.draggedItem != this && Mouse.IsOver(rect.MiddlePart(1f, 0.5f)))
            {
                Widgets.DrawStrongHighlight(rect);
                CustomResourceReadoutSettings.dropIntoCategory = this;
            }
        }

        protected override float DoSettingsInterfaceSub(Rect rect)
        {
            Rect expandRect = rect.LeftPartPixels(rect.height);
            if (Widgets.ButtonImage(expandRect.ContractedBy(2f), uiExpanded ? TexButton.Collapse : TexButton.Reveal))
            {
                uiExpanded = !uiExpanded;
                (uiExpanded ? SoundDefOf.TabOpen : SoundDefOf.TabClose).PlayOneShot(null);
            }
            Rect iconRect = expandRect;
            iconRect.x += iconRect.width;
            if (Icon != null)
            {
                GUI.color = iconColor;
                Widgets.DrawTextureFitted(iconRect.ContractedBy(1f), Icon, 1f);
                GUI.color = Color.white;
            }
            Rect labelRect = rect.RightPartPixels(rect.width - iconRect.width - expandRect.width);
            using (new TextBlock(TextAnchor.MiddleLeft)) Widgets.Label(labelRect, "CustomResourceReadout_ItemsInCategory".Translate(items.Count));

            float y = rect.y;
            if (uiExpanded)
            {
                rect.xMin += 24f;
                rect.y += rect.height;
                rect.height = 0f;
                foreach (ResourceReadoutItem item in items)
                {
                    rect.y += item.DoSettingsInterface(rect);
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
            if (Scribe.mode == LoadSaveMode.PostLoadInit)
            {
                if (items == null)
                {
                    items = new List<ResourceReadoutItem>();
                }
                foreach (ResourceReadoutItem item in items)
                {
                    item.parent = this;
                }
            }
        }
    }
}
