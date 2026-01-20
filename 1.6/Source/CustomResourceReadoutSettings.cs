using RimWorld;
using System;
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
        Custom,
        Preset
    }

    public class CustomResourceReadoutSettings : ModSettings
    {
        public static bool dirty;

        public static CustomResourceReadoutMode editingMode;
        public static ResourceReadoutItem deletedItem;
        private static Vector2 scrollPositionLeft, scrollPositionRight;
        private static float heightLeft, heightRight;
        private static int reorderableModeGroup;
        public static int reorderableItemGroup;
        public static ResourceReadoutItem draggedItem;
        public static ResourceReadoutCategory dropIntoCategory;

        private static ResourceReadoutModeType modeType;
        private static CustomResourceReadoutMode currentCustomMode;
        private static ResourceReadoutModeDef currentPreset;
        private static List<CustomResourceReadoutMode> customModes = new List<CustomResourceReadoutMode>();

        public static string CurrentModeLabel
        {
            get
            {
                switch (modeType)
                {
                    case ResourceReadoutModeType.Simple: return "CustomResourceReadout_BasicUncategorized".Translate();
                    case ResourceReadoutModeType.Categorized: return "CustomResourceReadout_BasicCategorized".Translate();
                    case ResourceReadoutModeType.Custom: return currentCustomMode.Name;
                    case ResourceReadoutModeType.Preset: return currentPreset.Name;
                    default: throw new System.Exception("Invalid modeType: " + modeType);
                }
            }
        }

        public static bool CustomOrPresetMode => modeType == ResourceReadoutModeType.Custom || modeType == ResourceReadoutModeType.Preset;

        public static IResourceReadoutMode CurrentMode => modeType == ResourceReadoutModeType.Custom ? currentCustomMode as IResourceReadoutMode : modeType == ResourceReadoutModeType.Preset ? currentPreset : null;

        public static IEnumerable<CustomResourceReadoutMode> CustomResourceReadoutModes => customModes;

        public static void ChangeResourceReadout(ResourceReadoutModeType type, CustomResourceReadoutMode mode = null, ResourceReadoutModeDef preset = null)
        {
            modeType = type;
            if (type == ResourceReadoutModeType.Simple)
            {
                Prefs.ResourceReadoutCategorized = false;
            }
            else if (type == ResourceReadoutModeType.Categorized)
            {
                Prefs.ResourceReadoutCategorized = true;
            }
            currentCustomMode = mode;
            currentPreset = preset;
            ResourceCounter.ResetDefs();
            foreach (Map map in Find.Maps)
            {
                map.resourceCounter.UpdateResourceCounts();
            }
            Utility.ClearCaches();
            dirty = true;
        }

        public static void DoSettingsWindowContents(Rect inRect)
        {
            DoLeftSide(inRect.LeftPart(0.35f));
            if (editingMode != null)
            {
                DoRightSide(inRect.RightPart(0.65f));
            }
            Utility.ClearCaches();
        }

        public static void AddCustomResourceReadoutMode(CustomResourceReadoutMode mode, string renameLabel = null, Action<bool> callback = null)
        {
            if (mode.name.NullOrEmpty())
            {
                mode.name = "CustomResourceReadout_EnterAUniqueName".Translate();
            }
            Find.WindowStack.Add(new Dialog_RenameResourceReadoutMode(mode, successful =>
            {
                if (successful)
                {
                    customModes.Add(mode);
                    editingMode = mode;
                }
                if (callback != null)
                {
                    callback(successful);
                }
            }, renameLabel));
        }

        private static void DoLeftSide(Rect left)
        {
            Widgets.Label(left, "CustomResourceReadout_CustomResourceReadoutModes".Translate());

            Rect outRect = left;
            outRect.yMin += 30f;
            outRect.yMax -= 80f;
            Rect viewRect = new Rect(0f, 0f, left.width - 20f, heightLeft);

            if (Event.current.type == EventType.Repaint)
            {
                reorderableModeGroup = ReorderableWidget.NewGroup((from, to) =>
                {
                    CustomResourceReadoutMode mode = customModes[from];
                    customModes.Insert(to, mode);
                    customModes.RemoveAt(from < to ? from : from + 1);
                }, ReorderableDirection.Vertical, outRect);
            }

            Widgets.BeginScrollView(outRect, ref scrollPositionLeft, viewRect);
            
            Rect modeRect = new Rect(viewRect.x, viewRect.y, viewRect.width, 30f);
            CustomResourceReadoutMode deletedMode = null;
            foreach (CustomResourceReadoutMode mode in customModes)
            {
                Rect innerRect = modeRect.ContractedBy(1f);
                if (editingMode == mode)
                {
                    Widgets.DrawHighlightSelected(innerRect);
                }

                if (!ReorderableWidget.Dragging)
                {
                    Widgets.DrawHighlightIfMouseover(innerRect);
                }
                if ((Event.current.type != EventType.MouseDown || Mouse.IsOver(innerRect)) && ReorderableWidget.Reorderable(reorderableModeGroup, modeRect))
                {
                    Widgets.DrawRectFast(innerRect, Color.black.WithAlpha(0.5f));
                }

                Rect buttonRect = innerRect.RightPartPixels(innerRect.height);
                if (Widgets.ButtonImage(buttonRect.ContractedBy(1f), TexButton.Delete, tooltip: "Delete".Translate()))
                {
                    SoundDefOf.Click.PlayOneShot(null);
                    deletedMode = mode;
                }
                buttonRect.x -= buttonRect.width;
                if (Widgets.ButtonImage(buttonRect.ContractedBy(1f), TexButton.Save, tooltip: "Export".Translate()))
                {
                    Find.WindowStack.Add(new Dialog_ExportResourceReadoutMode(mode));
                }
                buttonRect.x -= buttonRect.width;
                if (Widgets.ButtonImage(buttonRect.ContractedBy(1f), TexButton.Copy, tooltip: "Copy".Translate()))
                {
                    AddCustomResourceReadoutMode(mode.Copy(), "Copy".Translate());
                }
                buttonRect.x -= buttonRect.width;
                if (Widgets.ButtonImage(buttonRect.ContractedBy(1f), TexButton.Rename, tooltip: "Rename".Translate()))
                {
                    Find.WindowStack.Add(new Dialog_RenameResourceReadoutMode(mode));
                }

                Rect labelRect = innerRect;
                labelRect.xMax = buttonRect.xMin;
                using (new TextBlock(TextAnchor.MiddleLeft)) Widgets.Label(labelRect.ContractedBy(2f), mode.name);

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
                if (editingMode == deletedMode)
                {
                    editingMode = null;
                }
                if (currentCustomMode == deletedMode)
                {
                    currentCustomMode = null;
                    if (modeType == ResourceReadoutModeType.Custom)
                    {
                        ChangeResourceReadout(ResourceReadoutModeType.Simple);
                    }
                }
            }

            Widgets.EndScrollView();
            heightLeft = modeRect.y;

            Rect bottomRect = left.BottomPartPixels(80f);
            bottomRect.height = 40f;
            if (Widgets.ButtonText(bottomRect.ContractedBy(1f), "CustomResourceReadout_AddNewMode".Translate()))
            {
                Find.WindowStack.Add(new FloatMenu(new[]
                {
                    new FloatMenuOption("CustomResourceReadout_SimpleCopy".Translate(), () =>
                    {
                        AddCustomResourceReadoutMode(CustomResourceReadoutMode.FromSimple(), "CustomResourceReadout_NameNewMode".Translate());
                    }),
                    new FloatMenuOption("CustomResourceReadout_CategorizedCopy".Translate(), () =>
                    {
                        AddCustomResourceReadoutMode(CustomResourceReadoutMode.FromCategorized(), "CustomResourceReadout_NameNewMode".Translate());
                    }),
                    new FloatMenuOption("CustomResourceReadout_NewCustomResourceReadoutMode".Translate(), () =>
                    {
                        AddCustomResourceReadoutMode(new CustomResourceReadoutMode(), "CustomResourceReadout_NameNewMode".Translate());
                    })
                }.ToList()));
            }
            bottomRect.y += bottomRect.height;
            if (Widgets.ButtonText(bottomRect.ContractedBy(1f), "CustomResourceReadout_ImportMode".Translate()))
            {
                Find.WindowStack.Add(new Dialog_ImportResourceReadoutMode());
            }
        }

        private static void DoRightSide(Rect right)
        {
            Rect outRect = right;
            outRect.yMax -= 40f;
            Rect viewRect = new Rect(0f, 0f, right.width - 20f, heightRight);

            if (Event.current.type == EventType.Repaint)
            {
                reorderableItemGroup = ReorderableWidget.NewGroup((_, to) =>
                {
                    if (draggedItem != null && dropIntoCategory == null)
                    {
                        List<ResourceReadoutItem> draggableItems = editingMode.Items.SelectMany(i => i.DraggableItems).ToList();
                        bool accepted = true;
                        if (to < draggableItems.Count && draggableItems[to].parent?.CanAccept(draggedItem) == false)
                        {
                            accepted = false;
                        }
                        if (to >= draggableItems.Count || draggableItems[to].parent == null)
                        {
                            if (!editingMode.Items.CanAccept(draggedItem))
                            {
                                accepted = false;
                            }
                        }
                        if (!accepted)
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
                            editingMode.Items.Remove(draggedItem);
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
                                editingMode.Items.Insert(editingMode.Items.IndexOf(insertBeforeItem), draggedItem);
                            }
                            draggedItem.parent = insertBeforeItem.parent;
                        }
                        else
                        {
                            editingMode.Items.Add(draggedItem);
                            draggedItem.parent = null;
                        }
                    }
                }, ReorderableDirection.Vertical, outRect, dropIntoCategory != null ? 99999f : -1f);
            }

            dropIntoCategory = null;
            Widgets.BeginScrollView(outRect, ref scrollPositionRight, viewRect);

            Rect itemRect = new Rect(viewRect.x, viewRect.y, viewRect.width, 0f);
            foreach (ResourceReadoutItem item in editingMode.Items)
            {
                itemRect.y += item.DoSettingsInterface(itemRect);
            }
            if (deletedItem != null)
            {
                editingMode.Items.Remove(deletedItem);
                deletedItem = null;
            }

            Widgets.EndScrollView();
            heightRight = itemRect.y;

            if (Widgets.ButtonText(right.BottomPartPixels(40f).ContractedBy(1f), "CustomResourceReadout_AddNewItem".Translate()))
            {
                Find.WindowStack.Add(new FloatMenu(new[]
                {
                    new FloatMenuOption("CustomResourceReadout_AddResources".Translate(), () => Find.WindowStack.Add(new Dialog_SelectThingDefs(null))),
                    new FloatMenuOption("CustomResourceReadout_AddCategories".Translate(), () => Find.WindowStack.Add(new Dialog_AddThingCategoryDefs(null))),
                    new FloatMenuOption("CustomResourceReadout_AddEmptyCategory".Translate(), () => Find.WindowStack.Add(new Dialog_SelectIcon("CustomResourceReadout_AddEmptyCategory".Translate(), (p, c) =>
                    {
                        editingMode.Items.Add(new ResourceReadoutCategory(p, c, null));
                    })))
                }.ToList()));
            }
        }

        public static void OnDragEnd()
        {
            if (dropIntoCategory != null && draggedItem != null && editingMode != null)
            {
                if (dropIntoCategory.CanAccept(draggedItem))
                {
                    if (draggedItem.parent != null)
                    {
                        draggedItem.parent.items.Remove(draggedItem);
                    }
                    else
                    {
                        editingMode.Items.Remove(draggedItem);
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
            Scribe_References.Look(ref currentCustomMode, "currentMode");
            Scribe_Defs.Look(ref currentPreset, "currentPreset");
            Scribe_Collections.Look(ref customModes, "customModes", LookMode.Deep);
            if (Scribe.mode == LoadSaveMode.PostLoadInit)
            {
                if (customModes == null)
                {
                    customModes = new List<CustomResourceReadoutMode>();
                }
                editingMode = customModes.FirstOrDefault();
                if (modeType == ResourceReadoutModeType.Preset && currentPreset == null)
                {
                    ChangeResourceReadout(ResourceReadoutModeType.Simple);
                }
            }
        }
    }
}
