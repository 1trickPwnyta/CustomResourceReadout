using RimWorld;
using System.IO;
using System.Xml.Linq;
using UnityEngine;
using Verse;
using Verse.Sound;

namespace CustomResourceReadout
{
    public class Dialog_ImportResourceReadoutMode : Window
    {
        private FileInfo selectedFile;
        private float height;
        private Vector2 scrollPosition;

        public override Vector2 InitialSize => new Vector2(400f, 600f);

        public Dialog_ImportResourceReadoutMode()
        {
            doCloseX = true;
            closeOnClickedOutside = true;
            Directory.CreateDirectory(ResourceReadoutModeDef.exportPath);
        }

        public override void DoWindowContents(Rect inRect)
        {
            Rect titleRect = inRect;
            titleRect.height = 40f;
            using (new TextBlock(GameFont.Medium)) Widgets.Label(titleRect, "CustomResourceReadout_ImportMode".Translate());

            Rect openFolderRect = titleRect;
            openFolderRect.xMin = openFolderRect.xMax - 150f;
            openFolderRect.y += openFolderRect.height;
            openFolderRect.height = 30f;
            if (Widgets.ButtonText(openFolderRect.ContractedBy(1f), "CustomResourceReadout_OpenFolder".Translate()))
            {
                Application.OpenURL(ResourceReadoutModeDef.exportPath);
            }

            Rect outRect = inRect;
            outRect.yMin = openFolderRect.yMax;
            outRect.yMax -= CloseButSize.y + 10f;
            Rect viewRect = new Rect(0f, 0f, outRect.width - 20f, height);
            Widgets.BeginScrollView(outRect, ref scrollPosition, viewRect);
            Rect fileRect = viewRect.TopPartPixels(30f);
            foreach (FileInfo file in new DirectoryInfo(ResourceReadoutModeDef.exportPath).EnumerateFiles())
            {
                if (file.Name == selectedFile?.Name)
                {
                    Widgets.DrawHighlightSelected(fileRect);
                }
                using (new TextBlock(TextAnchor.MiddleLeft)) Widgets.Label(fileRect.ContractedBy(2f), file.Name);
                Widgets.DrawHighlightIfMouseover(fileRect);
                if (Widgets.ButtonInvisible(fileRect))
                {
                    selectedFile = file;
                    SoundDefOf.Click.PlayOneShot(null);
                }
                fileRect.y += fileRect.height;
            }
            Widgets.EndScrollView();

            Rect buttonRect = inRect.BottomPartPixels(CloseButSize.y);
            buttonRect.width = 150f;
            buttonRect.x += inRect.width / 2f - buttonRect.width - 5f;
            if (Widgets.ButtonText(buttonRect, "Cancel".Translate()))
            {
                Close();
            }
            buttonRect.x += buttonRect.width + 10f;
            if (Widgets.ButtonText(buttonRect, "OK".Translate()))
            {
                if (selectedFile != null)
                {
                    ResourceReadoutMode mode = ResourceReadoutModeDef.Import(selectedFile);
                    if (CustomResourceReadoutSettings.customModes.Any(m => m.name.EqualsIgnoreCase(mode.name)))
                    {
                        Messages.Message("CustomResourceReadout_NameAlreadyExists".Translate(), MessageTypeDefOf.RejectInput, false);
                    }
                    else
                    {
                        CustomResourceReadoutSettings.customModes.Add(mode);
                        CustomResourceReadoutSettings.editingMode = mode;
                        Close();
                    }
                }
                else
                {
                    Messages.Message("CustomResourceReadout_SelectFile".Translate(), MessageTypeDefOf.RejectInput, false);
                }
            }
        }
    }
}
