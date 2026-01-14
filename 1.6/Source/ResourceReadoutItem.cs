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
        public bool alwaysShow;
        public ResourceReadoutCategory parent;

        public abstract IEnumerable<ThingDef> ThingDefs { get; }

        public abstract IEnumerable<Tuple<ThingDef, ThingDef>> ThingDefsStuff { get; }

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

        public abstract float OnGUI(Rect rect, ResourceReadout readout, Dictionary<ThingDef, int> amounts);

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
