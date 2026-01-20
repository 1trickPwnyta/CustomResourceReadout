using System.Collections.Generic;
using System.IO;
using UnityEngine;
using Verse;

namespace CustomResourceReadout
{
    public class ResourceReadoutModeDef : Def, IResourceReadoutMode, IExposable
    {
        public static string exportPath = Path.Combine(GenFilePaths.SaveDataFolderPath, "CustomResourceReadoutExport");
        public static string exportExt = ".xml";

        public static CustomResourceReadoutMode Import(FileInfo file)
        {
            ResourceReadoutModeDef def = null;
            Scribe.loader.InitLoading(file.FullName);
            try
            {
                Scribe_Deep.Look(ref def, "CustomResourceReadout.ResourceReadoutModeDef");
            }
            finally
            {
                Scribe.loader.FinalizeLoading();
            }
            return CustomResourceReadoutMode.FromDef(def);
        }

        public List<ResourceReadoutItem> items = new List<ResourceReadoutItem>();

        public List<ResourceReadoutItem> Items => items;

        public string Name => label;

        public void ExposeData()
        {
            Scribe_Values.Look(ref defName, "defName");
            Scribe_Values.Look(ref label, "label");
            Scribe_Collections.Look(ref items, "items", LookMode.Deep);
        }

        public void Export(string name)
        {
            Directory.CreateDirectory(exportPath);
            Scribe.saver.InitSaving(Path.Combine(exportPath, name + exportExt), "Defs");
            try
            {
                ResourceReadoutModeDef def = this;
                Scribe_Deep.Look(ref def, "CustomResourceReadout.ResourceReadoutModeDef");
            }
            finally
            {
                Scribe.saver.FinalizeSaving();
            }
            Application.OpenURL(exportPath);
        }
    }
}
