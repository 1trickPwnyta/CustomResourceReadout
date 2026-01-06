using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Verse;

namespace CustomResourceReadout
{
    public abstract class ResourceReadoutItem
    {
        public ResourceReadoutCategory parent;

        public abstract IEnumerable<ThingDef> ThingDefs { get; }

        public abstract Texture2D Icon { get; }

        protected virtual float SettingsInterfaceInteractionRectHeight => 24f;

        public virtual IEnumerable<ResourceReadoutItem> DraggableItems => new[] { this };

        protected virtual IEnumerable<FloatMenuOption> FloatMenuOptions => new FloatMenuOption[0];

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

            if ((Event.current.type != EventType.MouseDown || Mouse.IsOver(rect)) && ReorderableWidget.Reorderable(CustomResourceReadoutSettings.reorderableGroup, rect))
            {
                CustomResourceReadoutSettings.draggedItem = this;
                Widgets.DrawRectFast(rect, Color.black.WithAlpha(0.5f));
            }

            float height = DoSettingsInterfaceSub(rect);

            if (Event.current.button == 2 && Widgets.ButtonInvisible(rect))
            {
                Find.WindowStack.Add(new FloatMenu(FloatMenuOptions.Concat(new[]
                {
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
    }
}
