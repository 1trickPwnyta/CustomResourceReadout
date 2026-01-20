using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Verse;

namespace CustomResourceReadout
{
    public class ResourceReadoutLeaf : ResourceReadoutItem, IExposable
    {
        private ThingDef def;
        public ThingDef stuff;
        public bool countAll;

        public ResourceReadoutLeaf()
        {
        }

        public ResourceReadoutLeaf(ThingDef def, ResourceReadoutCategory parent, ThingDef stuff = null) : base(parent)
        {
            Def = def;
            this.stuff = stuff;
        }

        public ThingDef Def
        {
            get => def;
            set
            {
                def = value;
                stuff = null;
            }
        }

        public override IEnumerable<ThingDef> ThingDefs
        {
            get { yield return def; }
        }

        public override IEnumerable<Tuple<ThingDef, ThingDef>> ThingDefsStuff
        {
            get { if (stuff != null) yield return new Tuple<ThingDef, ThingDef>(def, stuff); }
        }

        protected override IEnumerable<FloatMenuOption> FloatMenuOptions
        {
            get
            {
                if (def.MadeFromStuff)
                {
                    yield return new FloatMenuOption("CustomResourceReadout_ChangeStuff".Translate(), () =>
                    {
                        Find.WindowStack.Add(new Dialog_SelectStuff(parent, def));
                    });
                }
                yield return new FloatMenuOption("CustomResourceReadout_CountAll".Translate(), () =>
                {
                    countAll = !countAll;
                }, countAll ? Widgets.CheckboxOnTex : Widgets.CheckboxOffTex, Color.white, mouseoverGuiAction: r => TooltipHandler.TipRegionByKey(r, "CustomResourceReadout_CountAllDesc"), iconJustification: HorizontalJustification.Right);
            }
        }

        public override float GUIXOffset => 32f;

        public override IEnumerable<ResourceReadoutItem> ThisAndAllDescendants
        {
            get { yield return this; }
        }

        protected override float DoSettingsInterfaceSub(Rect rect)
        {
            Rect iconRect = rect.LeftPartPixels(rect.height);
            Widgets.DefIcon(iconRect.ContractedBy(1f), def, stuff ?? GenStuff.DefaultStuffFor(def));
            Vector2 dotPosition = new Vector2(iconRect.xMax - 8f, iconRect.yMax - 8f);
            if (alwaysShow)
            {
                DoDot(dotPosition, alwaysShowColor);
                dotPosition.x -= 6f;
            }
            if (countAll)
            {
                DoDot(dotPosition, Color.cyan);
                dotPosition.x -= 6f;
            }
            Rect labelRect = rect.RightPartPixels(rect.width - iconRect.width);
            using (new TextBlock(TextAnchor.MiddleLeft)) Widgets.Label(labelRect, def.LabelCap + (def.MadeFromStuff ? $" ({stuff?.LabelCap ?? "CustomResourceReadout_Any".Translate()})" : ""));
            return rect.height;
        }

        protected override void ExposeDataSub()
        {
            Scribe_Defs.Look(ref def, "def");
            Scribe_Defs.Look(ref stuff, "stuff");
            Scribe_Values.Look(ref countAll, "countAll");
        }

        public override float OnGUI(Rect rect, Dictionary<ThingDef, int> amounts)
        {
            int count = Count(amounts);
            if (count > 0 || alwaysShow)
            {
                rect.height = 24f;
                DrawResource(rect, count);
            }

            return rect.height;
        }

        private void DrawResource(Rect rect, int count)
        {
            if (Mouse.IsOver(rect))
            {
                GUI.DrawTexture(rect, TexUI.HighlightTex);
                TooltipHandler.TipRegion(rect, new TipSignal(() => def.LabelCap + (stuff != null ? $" ({stuff.LabelCap})" : "") + $": {def.description.CapitalizeFirst()}", def.shortHash + stuff?.shortHash ?? 0));
            }

            Rect iconRect = rect;
            iconRect.width = iconRect.height = 28f;
            iconRect.y = rect.y + rect.height / 2f - iconRect.height / 2f;
            Widgets.ThingIcon(iconRect, def, stuff);
            Rect labelRect = rect;
            labelRect.xMin = iconRect.xMax + 6f;
            Widgets.Label(labelRect, count.ToStringCached());
        }

        protected override ResourceReadoutItem CopySub()
        {
            ResourceReadoutLeaf copy = new ResourceReadoutLeaf(def, null, stuff);
            copy.countAll = countAll;
            return copy;
        }

        protected override int CountSub(Dictionary<ThingDef, int> amounts)
        {
            int count;
            if (stuff == null)
            {
                count = amounts.ContainsKey(def) ? amounts[def] : 0;
                if (countAll || true)
                {
                    count = Find.CurrentMap.listerThings.ThingsOfDef(def).Sum(t => t.stackCount);
                }
            }
            else
            {
                Dictionary<Tuple<ThingDef, ThingDef>, int> countedAmountsStuff = Find.CurrentMap.resourceCounter.GetCountedAmountsStuff();
                Tuple<ThingDef, ThingDef> key = new Tuple<ThingDef, ThingDef>(def, stuff);
                count = countedAmountsStuff.ContainsKey(key) ? countedAmountsStuff[key] : 0;
                if (countAll || true)
                {
                    count = Find.CurrentMap.listerThings.ThingsOfDef(def).Where(t => t.Stuff == stuff).Sum(t => t.stackCount);
                }
            }
            return count;
        }
    }
}
