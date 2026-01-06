using RimWorld;
using System.Collections.Generic;
using UnityEngine;
using Verse;

namespace CustomResourceReadout
{
    public class Dialog_SelectThingDefs : Dialog_SelectDefs<ThingDef>
    {
        public Dialog_SelectThingDefs(List<ResourceReadoutItem> items) : base(items)
        {
        }

        protected override bool DefAllowed(ThingDef def)
        {
            Texture2D icon = Widgets.GetIconFor(def);
            return icon != null && icon != BaseContent.BadTex && !def.IsCorpse && def.EverStorable(true);
        }

        protected override void DoIcon(Rect rect, ThingDef def)
        {
            Widgets.DefIcon(rect, def, GenStuff.DefaultStuffFor(def));
        }

        protected override bool HasDef(List<ResourceReadoutItem> items, ThingDef def) => items.Any(i => i is ResourceReadoutLeaf l && l.Def == def);

        protected override void AddDef(List<ResourceReadoutItem> items, ThingDef def) => items.Add(new ResourceReadoutLeaf(def));

        protected override void RemoveDef(List<ResourceReadoutItem> items, ThingDef def) => items.RemoveWhere(i => i is ResourceReadoutLeaf l && l.Def == def);
    }
}
