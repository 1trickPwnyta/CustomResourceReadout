using RimWorld;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Verse;
using Verse.Sound;

namespace CustomResourceReadout
{
    public abstract class Dialog_SelectDefs<T> : Window where T: Def
    {
        private List<IResourceReadoutItem> items;
        private Vector2 scrollPosition;
        private float height;

        public override Vector2 InitialSize => new Vector2(400f, 600f);

        public Dialog_SelectDefs(List<IResourceReadoutItem> items)
        {
            this.items = items;
            doCloseX = true;
            doCloseButton = true;
            closeOnClickedOutside = true;
        }

        protected abstract bool DefAllowed(T def);

        protected abstract Texture2D GetIcon(T def);

        protected abstract bool HasDef(List<IResourceReadoutItem> items, T def);

        protected abstract void AddDef(List<IResourceReadoutItem> items, T def);

        protected abstract void RemoveDef(List<IResourceReadoutItem> items, T def);

        public override void DoWindowContents(Rect inRect)
        {
            Rect outRect = inRect;
            outRect.height -= CloseButSize.y + 10f;
            Rect viewRect = new Rect(0f, 0f, outRect.width - 20f, height);
            Widgets.BeginScrollView(outRect, ref scrollPosition, viewRect);

            Rect defRect = new Rect(viewRect.x, viewRect.y, viewRect.width, 24f);
            bool shaded = false;
            foreach (T def in DefDatabase<T>.AllDefsListForReading.Where(d => DefAllowed(d)))
            {
                Texture2D icon = GetIcon(def);
                if (icon == null || icon == BaseContent.BadTex)
                {
                    continue;
                }

                if (shaded)
                {
                    Widgets.DrawRectFast(defRect, Widgets.MenuSectionBGFillColor);
                }

                Rect iconRect = defRect.LeftPartPixels(defRect.height);
                GUI.DrawTexture(iconRect.ContractedBy(1f), icon);

                Rect checkRect = defRect.RightPartPixels(defRect.height);
                bool included = HasDef(items, def);
                Widgets.CheckboxDraw(checkRect.x + 1f, checkRect.y + 1, included, false, defRect.height - 2f);

                Rect labelRect = defRect.MiddlePartPixels(defRect.width - iconRect.width - checkRect.width, defRect.height);
                using (new TextBlock(TextAnchor.MiddleLeft)) Widgets.Label(labelRect, def.LabelCap);

                Widgets.DrawHighlightIfMouseover(defRect);
                if (Widgets.ButtonInvisible(defRect))
                {
                    if (!included)
                    {
                        AddDef(items, def);
                        SoundDefOf.Tick_High.PlayOneShot(null);
                    }
                    else
                    {
                        RemoveDef(items, def);
                        SoundDefOf.Tick_Low.PlayOneShot(null);
                    }
                }

                defRect.y += defRect.height;
                shaded = !shaded;
            }
            height = defRect.y;

            Widgets.EndScrollView();
        }
    }
}
