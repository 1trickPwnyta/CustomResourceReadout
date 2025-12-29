using UnityEngine;
using Verse;

namespace CustomResourceReadout
{
    public class ResourceReadoutLeaf : IResourceReadoutItem, IExposable
    {
        private ThingDef def;
        private ThingDef stuff;
        private Texture2D icon;

        public ResourceReadoutLeaf() { }

        public ResourceReadoutLeaf(ThingDef def, ThingDef stuff = null) : this()
        {
            Def = def;
            Stuff = stuff;
        }

        public ThingDef Def
        {
            get => def;
            set
            {
                def = value;
                stuff = def.defaultStuff;
                icon = Widgets.GetIconFor(def, stuff);
            }
        }

        public ThingDef Stuff
        {
            get => stuff;
            set
            {
                stuff = value;
                icon = Widgets.GetIconFor(def, stuff);
            }
        }

        public Texture2D Icon => icon;

        public float DoSettingsInterface(Rect rect)
        {
            rect.height = 24f;
            Rect iconRect = rect.LeftPartPixels(rect.height);
            GUI.DrawTexture(iconRect.ContractedBy(1f), Icon);
            Rect labelRect = rect.RightPartPixels(rect.width - iconRect.width);
            using (new TextBlock(TextAnchor.MiddleLeft)) Widgets.Label(labelRect, def.LabelCap);
            return rect.height;
        }

        public void ExposeData()
        {
            Scribe_Defs.Look(ref def, "def");
            Scribe_Defs.Look(ref stuff, "stuff");
            if (Scribe.mode == LoadSaveMode.LoadingVars)
            {
                icon = Widgets.GetIconFor(def, stuff);
            }
        }
    }
}
