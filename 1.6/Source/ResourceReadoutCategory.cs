using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Verse;
using Verse.Sound;

namespace CustomResourceReadout
{
    public class ResourceReadoutCategory : ResourceReadoutItem, IExposable
    {
        private static Texture2D background;

        private static Texture2D Background
        {
            get
            {
                if (background == null)
                {
                    background = SolidColorMaterials.NewSolidColorTexture(new Color(0.1f, 0.1f, 0.1f, 0.6f));
                }
                return background;
            }
        }

        private bool expanded;
        private bool uiExpanded;
        private string iconPath;
        private Color iconColor;
        private Texture2D icon;
        public List<ResourceReadoutItem> items = new List<ResourceReadoutItem>();
        public ResourceReadoutItem deletedItem;
        public string tip;

        public override IEnumerable<ThingDef> ThingDefs => items.SelectMany(i => i.ThingDefs);

        public override IEnumerable<Tuple<ThingDef, ThingDef>> ThingDefsStuff => items.SelectMany(i => i.ThingDefsStuff);

        public Texture2D Icon
        {
            get
            {
                if (icon == null && iconPath != null)
                {
                    try
                    {
                        icon = ContentFinder<Texture2D>.Get(iconPath);
                    }
                    catch
                    {
                        icon = BaseContent.BadTex;
                    }
                }
                return icon;
            }
        }

        protected override IEnumerable<FloatMenuOption> FloatMenuOptions
        {
            get
            {
                yield return new FloatMenuOption("CustomResourceReadout_AddResources".Translate(), () => Find.WindowStack.Add(new Dialog_SelectThingDefs(this)));
                yield return new FloatMenuOption("CustomResourceReadout_AddCategories".Translate(), () => Find.WindowStack.Add(new Dialog_AddThingCategoryDefs(this)));
                yield return new FloatMenuOption("CustomResourceReadout_AddEmptyCategory".Translate(), () => Find.WindowStack.Add(new Dialog_SelectIcon("CustomResourceReadout_AddEmptyCategory".Translate(), (p, c) =>
                {
                    items.Add(new ResourceReadoutCategory(p, c, this));
                })));
                yield return new FloatMenuOption("CustomResourceReadout_EditIcon".Translate(), () => Find.WindowStack.Add(new Dialog_SelectIcon("CustomResourceReadout_EditIcon".Translate(), (p, c) =>
                {
                    iconPath = p;
                    icon = null;
                    iconColor = c;
                }, iconPath, iconColor)));
            }
        }

        public override IEnumerable<ResourceReadoutItem> DraggableItems => uiExpanded ? base.DraggableItems.Concat(items.SelectMany(i => i.DraggableItems)) : base.DraggableItems;

        public override float GUIXOffset => 7f;

        private bool AlwaysShowThisOrDescendant => alwaysShow || items.Any(i => i.alwaysShow || i is ResourceReadoutCategory c && c.AlwaysShowThisOrDescendant);

        public override IEnumerable<ResourceReadoutItem> ThisAndAllDescendants
        {
            get
            {
                yield return this;
                foreach (ResourceReadoutItem child in items)
                {
                    foreach (ResourceReadoutItem item in child.ThisAndAllDescendants)
                    {
                        yield return item;
                    }
                }
            }
        }

        public ResourceReadoutCategory()
        {
        }

        public ResourceReadoutCategory(string iconPath, Color iconColor, ResourceReadoutCategory parent) : base(parent)
        {
            this.iconPath = iconPath;
            this.iconColor = iconColor;
        }

        public ResourceReadoutCategory(ThingCategoryDef category, ResourceReadoutCategory parent) : this(category.iconPath, Color.white, parent)
        {
            foreach (ThingCategoryDef childCategory in category.childCategories)
            {
                items.Add(new ResourceReadoutCategory(childCategory, this));
            }
            foreach (ThingDef def in category.childThingDefs)
            {
                items.Add(new ResourceReadoutLeaf(def, this));
            }
        }

        public bool CanAccept(ResourceReadoutItem item)
        {
            if (item == this || parent?.CanAccept(item) == false)
            {
                return false;
            }
            return items.CanAccept(item);
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
                if (alwaysShow)
                {
                    DoDot(new Vector2(iconRect.xMax - 8f, iconRect.yMax - 8f), alwaysShowColor);
                }
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

        public override float OnGUI(Rect rect, Dictionary<ThingDef, int> amounts)
        {
            int totalCount = Count(amounts);
            if (totalCount > 0 || AlwaysShowThisOrDescendant)
            {
                rect.height = 24f;
                Rect expandRect = rect.LeftPartPixels(18f);
                if (Widgets.ButtonImage(expandRect.MiddlePartPixels(expandRect.width, 18f), expanded ? TexButton.Collapse : TexButton.Reveal))
                {
                    expanded = !expanded;
                    (expanded ? SoundDefOf.TabOpen : SoundDefOf.TabClose).PlayOneShot(null);
                }

                Rect mainRect = rect;
                mainRect.xMin += expandRect.width;
                Rect position = mainRect;
                position.width = 80f;
                position.yMax -= 3f;
                position.yMin += 3f;
                GUI.DrawTexture(position, Background);
                if (Mouse.IsOver(mainRect))
                {
                    GUI.DrawTexture(mainRect, TexUI.HighlightTex);
                    if (!tip.NullOrEmpty())
                    {
                        TooltipHandler.TipRegion(mainRect, new TipSignal(tip, GetHashCode()));
                    }
                }

                Rect iconRect = mainRect;
                iconRect.width = iconRect.height = 28f;
                iconRect.y = mainRect.y + mainRect.height / 2f - iconRect.height / 2f;
                if (Icon != null)
                {
                    GUI.color = iconColor;
                    Widgets.DrawTextureFitted(iconRect, Icon, 1f);
                    GUI.color = Color.white;
                }

                Rect labelRect = mainRect;
                labelRect.xMin = iconRect.xMax + 6f;
                Widgets.Label(labelRect, totalCount.ToStringCached());

                float y = rect.y;
                if (expanded)
                {
                    rect.y += rect.height;
                    rect.height = 0f;
                    foreach (ResourceReadoutItem item in items)
                    {
                        Rect itemRect = rect;
                        itemRect.xMin += item.GUIXOffset;
                        rect.y += item.OnGUI(itemRect, amounts);
                    }
                }

                return rect.yMax - y;
            }

            return 0f;
        }

        protected override void ExposeDataSub()
        {
            Scribe_Values.Look(ref expanded, "expanded");
            Scribe_Values.Look(ref iconPath, "iconPath");
            Scribe_Values.Look(ref iconColor, "iconColor");
            Scribe_Values.Look(ref tip, "tip", tip);
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

        protected override ResourceReadoutItem CopySub()
        {
            ResourceReadoutCategory copy = new ResourceReadoutCategory(iconPath, iconColor, null);
            copy.tip = tip;
            foreach (ResourceReadoutItem item in items)
            {
                ResourceReadoutItem itemCopy = item.Copy();
                itemCopy.parent = copy;
                copy.items.Add(itemCopy);
            }
            return copy;
        }

        protected override int CountSub(Dictionary<ThingDef, int> amounts) => items.Sum(i => i.Count(amounts));

        protected override void ResetCountSub()
        {
            foreach (ResourceReadoutItem item in items)
            {
                item.ResetCount();
            }
        }
    }
}
