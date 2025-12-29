using RimWorld;
using System.Collections.Generic;
using UnityEngine;
using Verse;
using Verse.Sound;

namespace CustomResourceReadout
{
    public class CustomResourceReadoutSettings : ModSettings
    {
        private static ResourceReadoutMode editingMode;

        public static ResourceReadoutMode currentMode;
        public static List<ResourceReadoutMode> customModes = new List<ResourceReadoutMode>();

        private static Vector2 scrollPositionLeft, scrollPositionRight;
        private static float heightLeft, heightRight;
        private static QuickSearchWidget search = new QuickSearchWidget();

        public static void DoSettingsWindowContents(Rect inRect)
        {
            DoLeftSide(inRect.LeftPart(0.35f));
            if (editingMode != null)
            {
                DoRightSide(inRect.RightPart(0.65f));
            }
        }

        private static void DoLeftSide(Rect left)
        {
            Widgets.Label(left, "CustomResourceReadout_CustomResourceReadoutModes".Translate());

            Rect outRect = left;
            outRect.yMin += 30f;
            outRect.yMax -= 40f;
            Rect viewRect = new Rect(0f, 0f, left.width - 20f, heightLeft);
            Widgets.BeginScrollView(outRect, ref scrollPositionLeft, viewRect);

            Rect modeRect = new Rect(viewRect.x, viewRect.y, viewRect.width, 30f);
            ResourceReadoutMode deletedMode = null;
            foreach (ResourceReadoutMode mode in customModes)
            {
                Rect innerRect = modeRect.ContractedBy(1f);
                Widgets.DrawRectFast(innerRect, Widgets.MenuSectionBGFillColor);
                using (new TextBlock(TextAnchor.MiddleLeft)) Widgets.Label(innerRect.ContractedBy(2f), mode.name);
                Widgets.DrawHighlightIfMouseover(innerRect);
                Rect buttonRect = innerRect.RightPartPixels(innerRect.height);
                if (Widgets.ButtonImage(buttonRect.ContractedBy(1f), TexButton.Delete))
                {
                    SoundDefOf.Click.PlayOneShot(null);
                    deletedMode = mode;
                }
                buttonRect.x -= buttonRect.width;
                if (Widgets.ButtonImage(buttonRect.ContractedBy(1f), TexButton.Rename))
                {
                    Find.WindowStack.Add(new Dialog_RenameResourceReadoutMode(mode));
                }
                if (Widgets.ButtonInvisible(innerRect))
                {
                    editingMode = mode;
                }
                modeRect.y += modeRect.height;
            }
            if (deletedMode != null)
            {
                customModes.Remove(deletedMode);
            }

            Widgets.EndScrollView();
            heightLeft = modeRect.y;

            if (Widgets.ButtonText(left.BottomPartPixels(40f).ContractedBy(1f), "CustomResourceReadout_AddNewMode".Translate()))
            {
                ResourceReadoutMode mode = new ResourceReadoutMode("CustomResourceReadout_EnterAUniqueName".Translate());
                Find.WindowStack.Add(new Dialog_RenameResourceReadoutMode(mode, () => customModes.Add(mode)));
            }
        }

        private static void DoRightSide(Rect right)
        {
            Rect outRect = right;
            outRect.yMax -= 40f;
            Rect viewRect = new Rect(0f, 0f, right.width - 20f, heightRight);
            Widgets.BeginScrollView(outRect, ref scrollPositionRight, viewRect);

            Rect itemRect = new Rect(viewRect.x, viewRect.y, viewRect.width, 0f);
            foreach (IResourceReadoutItem item in editingMode.items)
            {
                itemRect.y += item.DoSettingsInterface(itemRect);
            }

            Widgets.EndScrollView();
            heightRight = itemRect.y;

            if (Widgets.ButtonText(right.BottomPartPixels(40f).ContractedBy(1f), "CustomResourceReadout_AddNewItem".Translate()))
            {
                editingMode.items.Add(new ResourceReadoutLeaf(ThingDefOf.Boomshroom));
            }
        }

        public override void ExposeData()
        {
            Scribe_References.Look(ref currentMode, "currentMode");
            Scribe_Collections.Look(ref customModes, "customModes", LookMode.Deep);
            if (Scribe.mode == LoadSaveMode.PostLoadInit && customModes == null)
            {
                customModes = new List<ResourceReadoutMode>();
            }
        }
    }
}
