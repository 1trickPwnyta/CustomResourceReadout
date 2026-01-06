using RimWorld;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Verse;
using Verse.Sound;

namespace CustomResourceReadout
{
    public enum ResourceReadoutModeType
    {
        Simple,
        Categorized,
        Custom
    }

    public class CustomResourceReadoutSettings : ModSettings
    {
        private static ResourceReadoutMode editingMode;
        public static ResourceReadoutItem deletedItem;
        private static Vector2 scrollPositionLeft, scrollPositionRight;
        private static float heightLeft, heightRight;
        public static int reorderableGroup;
        public static ResourceReadoutItem draggedItem;
        public static ResourceReadoutCategory dropIntoCategory;

        public static ResourceReadoutModeType modeType;
        public static ResourceReadoutMode currentMode;
        public static List<ResourceReadoutMode> customModes = new List<ResourceReadoutMode>();

        public static string CurrentModeLabel
        {
            get
            {
                switch (modeType)
                {
                    case ResourceReadoutModeType.Simple: return "CustomResourceReadout_BasicUncategorized".Translate();
                    case ResourceReadoutModeType.Categorized: return "CustomResourceReadout_BasicCategorized".Translate();
                    default: return currentMode.name;
                }
            }
        }

        public static void DoSettingsWindowContents(Rect inRect)
        {
            DoLeftSide(inRect.LeftPart(0.35f));
            if (editingMode != null)
            {
                DoRightSide(inRect.RightPart(0.65f));
            }
        }

        private static void DoLeftSide(Rect left)
        {
            Widgets.Label(left, "CustomResourceReadout_CustomResourceReadoutModes".Translate());

            Rect outRect = left;
            outRect.yMin += 30f;
            outRect.yMax -= 40f;
            Rect viewRect = new Rect(0f, 0f, left.width - 20f, heightLeft);
            Widgets.BeginScrollView(outRect, ref scrollPositionLeft, viewRect);

            Rect modeRect = new Rect(viewRect.x, viewRect.y, viewRect.width, 30f);
            ResourceReadoutMode deletedMode = null;
            foreach (ResourceReadoutMode mode in customModes)
            {
                Rect innerRect = modeRect.ContractedBy(1f);
                Widgets.DrawRectFast(innerRect, Widgets.MenuSectionBGFillColor);
                using (new TextBlock(TextAnchor.MiddleLeft)) Widgets.Label(innerRect.ContractedBy(2f), mode.name);
                Widgets.DrawHighlightIfMouseover(innerRect);
                Rect buttonRect = innerRect.RightPartPixels(innerRect.height);
                if (Widgets.ButtonImage(buttonRect.ContractedBy(1f), TexButton.Delete))
                {
                    SoundDefOf.Click.PlayOneShot(null);
                    deletedMode = mode;
                }
                buttonRect.x -= buttonRect.width;
                if (Widgets.ButtonImage(buttonRect.ContractedBy(1f), TexButton.Rename))
                {
                    Find.WindowStack.Add(new Dialog_RenameResourceReadoutMode(mode));
                }
                if (Widgets.ButtonInvisible(innerRect))
                {
                    editingMode = mode;
                    SoundDefOf.Click.PlayOneShot(null);
                }
                modeRect.y += modeRect.height;
            }
            if (deletedMode != null)
            {
                customModes.Remove(deletedMode);
            }

            Widgets.EndScrollView();
            heightLeft = modeRect.y;

            if (Widgets.ButtonText(left.BottomPartPixels(40f).ContractedBy(1f), "CustomResourceReadout_AddNewMode".Translate()))
            {
                ResourceReadoutMode mode = new ResourceReadoutMode("CustomResourceReadout_EnterAUniqueName".Translate());
                Find.WindowStack.Add(new Dialog_RenameResourceReadoutMode(mode, () => customModes.Add(mode)));
            }
        }

        private static void DoRightSide(Rect right)
        {
            Rect outRect = right;
            outRect.yMax -= 40f;
            Rect viewRect = new Rect(0f, 0f, right.width - 20f, heightRight);

            if (Event.current.type == EventType.Repaint)
            {
                reorderableGroup = ReorderableWidget.NewGroup((_, to) =>
                {
                    if (draggedItem != null && dropIntoCategory == null)
                    {
                        List<ResourceReadoutItem> draggableItems = editingMode.items.SelectMany(i => i.DraggableItems).ToList();
                        if (to < draggableItems.Count && draggableItems[to].parent?.Accepts(draggedItem) == false)
                        {
                            SoundDefOf.ClickReject.PlayOneShot(null);
                            return;
                        }

                        if (draggedItem.parent != null)
                        {
                            draggedItem.parent.items.Remove(draggedItem);
                        }
                        else
                        {
                            editingMode.items.Remove(draggedItem);
                        }

                        if (to < draggableItems.Count)
                        {
                            ResourceReadoutItem insertBeforeItem = draggableItems[to];
                            if (insertBeforeItem.parent != null)
                            {
                                insertBeforeItem.parent.items.Insert(insertBeforeItem.parent.items.IndexOf(insertBeforeItem), draggedItem);
                            }
                            else
                            {
                                editingMode.items.Insert(editingMode.items.IndexOf(insertBeforeItem), draggedItem);
                            }
                            draggedItem.parent = insertBeforeItem.parent;
                        }
                        else
                        {
                            editingMode.items.Add(draggedItem);
                            draggedItem.parent = null;
                        }
                    }
                }, ReorderableDirection.Vertical, outRect, dropIntoCategory != null ? 99999f : -1f);
            }

            dropIntoCategory = null;
            Widgets.BeginScrollView(outRect, ref scrollPositionRight, viewRect);

            Rect itemRect = new Rect(viewRect.x, viewRect.y, viewRect.width, 0f);
            foreach (ResourceReadoutItem item in editingMode.items)
            {
                itemRect.y += item.DoSettingsInterface(itemRect);
            }
            if (deletedItem != null)
            {
                editingMode.items.Remove(deletedItem);
                deletedItem = null;
            }

            Widgets.EndScrollView();
            heightRight = itemRect.y;

            if (Widgets.ButtonText(right.BottomPartPixels(40f).ContractedBy(1f), "CustomResourceReadout_AddNewItem".Translate()))
            {
                Find.WindowStack.Add(new FloatMenu(new[]
                {
                    new FloatMenuOption("CustomResourceReadout_AddResources".Translate(), () => Find.WindowStack.Add(new Dialog_SelectThingDefs(editingMode.items))),
                    new FloatMenuOption("CustomResourceReadout_AddCategories".Translate(), () => Find.WindowStack.Add(new Dialog_AddThingCategoryDefs(editingMode.items))),
                    new FloatMenuOption("CustomResourceReadout_AddEmptyCategory".Translate(), () => Find.WindowStack.Add(new Dialog_SelectIcon("CustomResourceReadout_AddEmptyCategory".Translate(), (p, c) =>
                    {
                        editingMode.items.Add(new ResourceReadoutCategory(p, c));
                    })))
                }.ToList()));
            }
        }

        public static void OnDragEnd()
        {
            if (dropIntoCategory != null && draggedItem != null && editingMode != null)
            {
                if (dropIntoCategory.Accepts(draggedItem))
                {
                    if (draggedItem.parent != null)
                    {
                        draggedItem.parent.items.Remove(draggedItem);
                    }
                    else
                    {
                        editingMode.items.Remove(draggedItem);
                    }
                    dropIntoCategory.items.Add(draggedItem);
                    draggedItem.parent = dropIntoCategory;
                }
                else
                {
                    SoundDefOf.ClickReject.PlayOneShot(null);
                }
            }
        }

        public override void ExposeData()
        {
            Scribe_Values.Look(ref modeType, "modeType");
            Scribe_References.Look(ref currentMode, "currentMode");
            Scribe_Collections.Look(ref customModes, "customModes", LookMode.Deep);
            if (Scribe.mode == LoadSaveMode.PostLoadInit)
            {
                if (customModes == null)
                {
                    customModes = new List<ResourceReadoutMode>();
                }
                editingMode = customModes.FirstOrDefault();
            }
        }
    }
}
