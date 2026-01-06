using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Verse;

namespace CustomResourceReadout
{
    public class Dialog_SelectStuff : Dialog_SelectDefs<ThingDef>
    {
        private ThingDef thingDef;
        private int index;

        public Dialog_SelectStuff(List<ResourceReadoutItem> items, ThingDef thingDef) : base(items)
        {
            this.thingDef = thingDef;
            index = items.FindIndex(i => i is ResourceReadoutLeaf l && l.Def == thingDef);
        }

        protected override bool AllowNull => true;

        protected override void OnSelectNull(List<ResourceReadoutItem> items, bool selected)
        {
            items.RemoveWhere(i => i is ResourceReadoutLeaf l && l.Def == thingDef && (selected || l.stuff == null));
        }

        protected override bool DefAllowed(ThingDef def) => thingDef.stuffCategories.Any(s => def.stuffProps?.categories?.Contains(s) == true);

        protected override void DoIcon(Rect rect, ThingDef def)
        {
            Widgets.DefIcon(rect, def);
        }

        protected override bool HasDef(List<ResourceReadoutItem> items, ThingDef def) => items.Any(i => i is ResourceReadoutLeaf l && l.Def == thingDef && l.stuff == def);

        protected override void AddDef(List<ResourceReadoutItem> items, ThingDef def) => items.Insert(Mathf.Min(index, items.Count), new ResourceReadoutLeaf(thingDef, def));

        protected override void RemoveDef(List<ResourceReadoutItem> items, ThingDef def) => items.RemoveWhere(i => i is ResourceReadoutLeaf l && l.Def == thingDef && l.stuff == def);
    }
}
