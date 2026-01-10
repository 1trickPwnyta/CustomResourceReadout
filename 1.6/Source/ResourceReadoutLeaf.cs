using RimWorld;
using System;
using System.Collections.Generic;
using UnityEngine;
using Verse;

namespace CustomResourceReadout
{
    public class ResourceReadoutLeaf : ResourceReadoutItem, IExposable
    {
        private ThingDef def;
        public ThingDef stuff;

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
            }
        }

        public override float GUIXOffset => 32f;

        protected override float DoSettingsInterfaceSub(Rect rect)
        {
            Rect iconRect = rect.LeftPartPixels(rect.height);
            Widgets.DefIcon(iconRect.ContractedBy(1f), def, stuff ?? GenStuff.DefaultStuffFor(def));
            Rect labelRect = rect.RightPartPixels(rect.width - iconRect.width);
            using (new TextBlock(TextAnchor.MiddleLeft)) Widgets.Label(labelRect, def.LabelCap + (def.MadeFromStuff ? $" ({stuff?.LabelCap ?? "CustomResourceReadout_Any".Translate()})" : ""));
            return rect.height;
        }

        protected override void ExposeDataSub()
        {
            Scribe_Defs.Look(ref def, "def");
            Scribe_Defs.Look(ref stuff, "stuff");
        }

        public override float OnGUI(Rect rect, ResourceReadout readout, Dictionary<ThingDef, int> amounts)
        {
            if (stuff == null)
            {
                if (amounts[def] > 0)
                {
                    rect.height = 24f;
                    DrawResource(rect, amounts[def]);
                }
            }
            else
            {
                Dictionary<Tuple<ThingDef, ThingDef>, int> countedAmountsStuff = Find.CurrentMap.resourceCounter.GetCountedAmountsStuff();
                Tuple<ThingDef, ThingDef> key = new Tuple<ThingDef, ThingDef>(def, stuff);
                int count = countedAmountsStuff.ContainsKey(key) ? countedAmountsStuff[key] : 0;
                if (count > 0)
                {
                    rect.height = 24f;
                    DrawResource(rect, count);
                }
            }

            return rect.height;
        }

        private void DrawResource(Rect rect, int count)
        {
            // TODO Get simple readout to match custom simple readout
            if (Mouse.IsOver(rect))
            {
                GUI.DrawTexture(rect, TexUI.HighlightTex);
                TooltipHandler.TipRegion(rect, new TipSignal(() => def.LabelCap + (stuff != null ? $" ({stuff.LabelCap})" : "") + $": {def.description.CapitalizeFirst()}", def.shortHash + stuff?.shortHash ?? 0));
            }

            Rect iconRect = rect;
            iconRect.width = iconRect.height = 28f;
            iconRect.y = rect.y + rect.height / 2f - iconRect.height / 2f;
            Widgets.DefIcon(iconRect, def, stuff);
            Rect labelRect = rect;
            labelRect.xMin = iconRect.xMax + 6f;
            Widgets.Label(labelRect, count.ToStringCached());
        }
    }
}
