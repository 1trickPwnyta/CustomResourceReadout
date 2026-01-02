using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Verse;

namespace CustomResourceReadout
{
    public class Dialog_AddThingCategoryDefs : Dialog_SelectDefs<ThingCategoryDef>
    {
        private List<IResourceReadoutItem> items;
        private HashSet<ThingCategoryDef> selectedDefs = new HashSet<ThingCategoryDef>();

        public Dialog_AddThingCategoryDefs(List<IResourceReadoutItem> items) : base(items)
        {
            this.items = items;
        }

        protected override bool DefAllowed(ThingCategoryDef def) => true;

        protected override Texture2D GetIcon(ThingCategoryDef def) => def.icon;

        protected override bool HasDef(List<IResourceReadoutItem> items, ThingCategoryDef def) => selectedDefs.Contains(def);

        protected override void AddDef(List<IResourceReadoutItem> items, ThingCategoryDef def) => selectedDefs.Add(def);

        protected override void RemoveDef(List<IResourceReadoutItem> items, ThingCategoryDef def) => selectedDefs.Remove(def);

        public override void PreClose()
        {
            items.AddRange(selectedDefs.Select(d => new ResourceReadoutCategory(d)));
        }
    }
}
