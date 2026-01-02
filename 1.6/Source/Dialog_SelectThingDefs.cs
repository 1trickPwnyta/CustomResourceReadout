using System.Collections.Generic;
using UnityEngine;
using Verse;

namespace CustomResourceReadout
{
    public class Dialog_SelectThingDefs : Dialog_SelectDefs<ThingDef>
    {
        public Dialog_SelectThingDefs(List<IResourceReadoutItem> items) : base(items)
        {
        }

        protected override bool DefAllowed(ThingDef def) => def.EverStorable(true);

        protected override Texture2D GetIcon(ThingDef def) => Widgets.GetIconFor(def);

        protected override bool HasDef(List<IResourceReadoutItem> items, ThingDef def) => items.Any(i => i is ResourceReadoutLeaf l && l.Def == def);

        protected override void AddDef(List<IResourceReadoutItem> items, ThingDef def) => items.Add(new ResourceReadoutLeaf(def));

        protected override void RemoveDef(List<IResourceReadoutItem> items, ThingDef def) => items.RemoveWhere(i => i is ResourceReadoutLeaf l && l.Def == def);
    }
}
