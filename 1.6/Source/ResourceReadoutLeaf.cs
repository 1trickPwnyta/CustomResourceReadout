using RimWorld;
using System.Collections.Generic;
using UnityEngine;
using Verse;

namespace CustomResourceReadout
{
    public class ResourceReadoutLeaf : ResourceReadoutItem, IExposable
    {
        private ThingDef def;
        public ThingDef stuff;

        public ResourceReadoutLeaf() { }

        public ResourceReadoutLeaf(ThingDef def, ThingDef stuff = null) : this()
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

        public override IEnumerable<ThingDef> ThingDefs => new[] { def };

        protected override IEnumerable<FloatMenuOption> FloatMenuOptions
        {
            get
            {
                if (def.MadeFromStuff)
                {
                    yield return new FloatMenuOption("CustomResourceReadout_ChangeStuff".Translate(), () =>
                    {
                        Find.WindowStack.Add(new Dialog_SelectStuff(parent?.items ?? CustomResourceReadoutSettings.editingMode.items, def));
                    });
                }
            }
        }

        protected override float DoSettingsInterfaceSub(Rect rect)
        {
            Rect iconRect = rect.LeftPartPixels(rect.height);
            Widgets.DefIcon(iconRect.ContractedBy(1f), def, stuff ?? GenStuff.DefaultStuffFor(def));
            Rect labelRect = rect.RightPartPixels(rect.width - iconRect.width);
            using (new TextBlock(TextAnchor.MiddleLeft)) Widgets.Label(labelRect, def.LabelCap + (def.MadeFromStuff ? $" ({stuff?.LabelCap ?? "CustomResourceReadout_Any".Translate()})" : ""));
            return rect.height;
        }

        public void ExposeData()
        {
            Scribe_Defs.Look(ref def, "def");
            Scribe_Defs.Look(ref stuff, "stuff");
        }
    }
}
