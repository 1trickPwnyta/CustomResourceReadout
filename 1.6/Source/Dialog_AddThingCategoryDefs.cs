using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Verse;

namespace CustomResourceReadout
{
    public class Dialog_AddThingCategoryDefs : Dialog_SelectDefs<ThingCategoryDef>
    {
        private HashSet<ThingCategoryDef> selectedDefs = new HashSet<ThingCategoryDef>();

        public Dialog_AddThingCategoryDefs(ResourceReadoutCategory parent) : base(parent)
        {
        }

        protected override bool DefAllowed(ThingCategoryDef def) => def.icon != null && def.icon != BaseContent.BadTex;

        protected override void DoIcon(Rect rect, ThingCategoryDef def)
        {
            GUI.DrawTexture(rect, def.icon);
        }

        protected override bool HasDef(List<ResourceReadoutItem> items, ThingCategoryDef def) => selectedDefs.Contains(def);

        protected override void AddDef(List<ResourceReadoutItem> items, ThingCategoryDef def) => selectedDefs.Add(def);

        protected override void RemoveDef(List<ResourceReadoutItem> items, ThingCategoryDef def) => selectedDefs.Remove(def);

        public override void PreClose()
        {
            items.AddRange(selectedDefs.Select(d => new ResourceReadoutCategory(d, parent) { tip = d.LabelCap}));
        }
    }
}
