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
        private List<ResourceReadoutItem> items;
        private Vector2 scrollPosition;
        private float height;
        private QuickSearchWidget search = new QuickSearchWidget();
        private bool focusedSearch;

        public override Vector2 InitialSize => new Vector2(400f, 600f);

        public Dialog_SelectDefs(List<ResourceReadoutItem> items)
        {
            this.items = items;
            doCloseX = true;
            doCloseButton = true;
            closeOnClickedOutside = true;
        }

        protected abstract bool DefAllowed(T def);

        protected abstract void DoIcon(Rect rect, T def);

        protected abstract bool HasDef(List<ResourceReadoutItem> items, T def);

        protected abstract void AddDef(List<ResourceReadoutItem> items, T def);

        protected abstract void RemoveDef(List<ResourceReadoutItem> items, T def);

        public override void DoWindowContents(Rect inRect)
        {
            Rect searchRect = inRect;
            searchRect.height = 30f;
            searchRect.width -= 20f;
            search.OnGUI(searchRect.ContractedBy(2f));
            Rect outRect = inRect;
            outRect.yMin = searchRect.yMax + 10f;
            outRect.yMax -= CloseButSize.y + 10f;
            Rect viewRect = new Rect(0f, 0f, outRect.width - 20f, height);
            Widgets.BeginScrollView(outRect, ref scrollPosition, viewRect);

            Rect defRect = new Rect(viewRect.x, viewRect.y, viewRect.width, 24f);
            bool shaded = false;
            foreach (T def in DefDatabase<T>.AllDefsListForReading.Where(d => DefAllowed(d) && search.filter.Matches(d.defName + d.label)))
            {
                if (shaded)
                {
                    Widgets.DrawRectFast(defRect, Widgets.MenuSectionBGFillColor);
                }

                Rect iconRect = defRect.LeftPartPixels(defRect.height);
                DoIcon(iconRect.ContractedBy(1f), def);

                Rect checkRect = defRect.RightPartPixels(defRect.height);
                bool included = HasDef(items, def);
                Widgets.CheckboxDraw(checkRect.x + 1f, checkRect.y + 1, included, false, defRect.height - 2f);

                Rect labelRect = defRect.MiddlePartPixels(defRect.width - iconRect.width - checkRect.width, defRect.height);
                using (new TextBlock(TextAnchor.MiddleLeft)) Widgets.Label(labelRect, def.LabelCap);

                Widgets.DrawHighlightIfMouseover(defRect);
                bool wasIncluded = included;
                Widgets.ToggleInvisibleDraggable(defRect, ref included, true, true);
                if (included != wasIncluded)
                {
                    if (included)
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

            if (!focusedSearch)
            {
                search.Focus();
                focusedSearch = true;
            }
        }
    }
}
