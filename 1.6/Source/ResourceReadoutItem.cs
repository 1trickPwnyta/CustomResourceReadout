using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Verse;

namespace CustomResourceReadout
{
    public abstract class ResourceReadoutItem : IExposable
    {
        protected static Color alwaysShowColor = Color.green;

        private int count;
        private bool counted;

        public bool alwaysShow;
        public ResourceReadoutCategory parent;

        public abstract IEnumerable<ThingDef> ThingDefs { get; }

        public abstract IEnumerable<Tuple<ThingDef, ThingDef>> ThingDefsStuff { get; }

        public abstract IEnumerable<ResourceReadoutItem> ThisAndAllDescendants { get; }

        protected virtual float SettingsInterfaceInteractionRectHeight => 24f;

        public virtual IEnumerable<ResourceReadoutItem> DraggableItems
        {
            get { yield return this; }
        }

        protected virtual IEnumerable<FloatMenuOption> FloatMenuOptions
        {
            get { yield break; }
        }

        public abstract float GUIXOffset { get; }

        public ResourceReadoutItem(ResourceReadoutCategory parent = null)
        {
            this.parent = parent;
        }

        protected abstract int CountSub(Dictionary<ThingDef, int> amounts);

        public int Count(Dictionary<ThingDef, int> amounts)
        {
            if (!counted)
            {
                count = CountSub(amounts);
                counted = true;
            }
            return count;
        }

        protected virtual void ResetCountSub() { }

        public void ResetCount()
        {
            counted = false;
            ResetCountSub();
        }

        protected abstract float DoSettingsInterfaceSub(Rect rect);

        protected virtual void OnDragOver(Rect rect) { }

        public float DoSettingsInterface(Rect rect)
        {
            rect.height = SettingsInterfaceInteractionRectHeight;

            if (!ReorderableWidget.Dragging)
            {
                Widgets.DrawHighlightIfMouseover(rect);
            }
            else if (Mouse.IsOver(rect))
            {
                OnDragOver(rect);
            }

            if ((Event.current.type != EventType.MouseDown || Mouse.IsOver(rect)) && ReorderableWidget.Reorderable(CustomResourceReadoutSettings.reorderableItemGroup, rect))
            {
                CustomResourceReadoutSettings.draggedItem = this;
                Widgets.DrawRectFast(rect, Color.black.WithAlpha(0.5f));
            }

            float height = DoSettingsInterfaceSub(rect);

            if (Event.current.button == 1 && Widgets.ButtonInvisible(rect))
            {
                Find.WindowStack.Add(new FloatMenu(FloatMenuOptions.Concat(new[]
                {
                    new FloatMenuOption("CustomResourceReadout_AlwaysShow".Translate(), () =>
                    {
                        alwaysShow = !alwaysShow;
                    }, alwaysShow ? Widgets.CheckboxOnTex : Widgets.CheckboxOffTex, Color.white, iconJustification: HorizontalJustification.Right, mouseoverGuiAction: r => TooltipHandler.TipRegionByKey(r, "CustomResourceReadout_AlwaysShowDesc")),
                    new FloatMenuOption("Delete".Translate(), () =>
                    {
                        if (parent != null)
                        {
                            parent.deletedItem = this;
                        }
                        else
                        {
                            CustomResourceReadoutSettings.deletedItem = this;
                        }
                    })
                }).ToList()));
            }

            return height;
        }

        public abstract float OnGUI(Rect rect, Dictionary<ThingDef, int> amounts);

        protected void DoDot(Vector2 position, Color color)
        {
            GUI.DrawTexture(new Rect(position.x, position.y, 6f, 6f), TexUI.DotHighlight, ScaleMode.ScaleToFit, true, 1f, color, 0f, 0f);
        }

        protected abstract ResourceReadoutItem CopySub();

        public ResourceReadoutItem Copy()
        {
            ResourceReadoutItem copy = CopySub();
            copy.alwaysShow = alwaysShow;
            return copy;
        }

        protected abstract void ExposeDataSub();

        public void ExposeData()
        {
            Scribe_Values.Look(ref alwaysShow, "alwaysShow");
            ExposeDataSub();
        }
    }
}
