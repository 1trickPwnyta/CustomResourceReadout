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
            if (stuff != null)
            {
                this.stuff = stuff;
            }
        }

        public ThingDef Def
        {
            get => def;
            set
            {
                def = value;
                stuff = GenStuff.DefaultStuffFor(def);
            }
        }

        public override IEnumerable<ThingDef> ThingDefs => new[] { def };

        protected override float DoSettingsInterfaceSub(Rect rect)
        {
            Rect iconRect = rect.LeftPartPixels(rect.height);
            Widgets.DefIcon(iconRect.ContractedBy(1f), def, stuff);
            Rect labelRect = rect.RightPartPixels(rect.width - iconRect.width);
            using (new TextBlock(TextAnchor.MiddleLeft)) Widgets.Label(labelRect, def.LabelCap);
            return rect.height;
        }

        public void ExposeData()
        {
            Scribe_Defs.Look(ref def, "def");
            Scribe_Defs.Look(ref stuff, "stuff");
        }
    }
}
