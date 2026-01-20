using System.IO;
using Verse;

namespace CustomResourceReadout
{
    public class Dialog_ExportResourceReadoutMode : Dialog_Rename<CustomResourceReadoutMode>
    {
        private CustomResourceReadoutMode exporting;

        public Dialog_ExportResourceReadoutMode(CustomResourceReadoutMode exporting) : base(new CustomResourceReadoutMode(exporting.name))
        {
            this.exporting = exporting;
        }

        protected override int MaxNameLength => 64;

        protected override AcceptanceReport NameIsValid(string name)
        {
            if (name.IndexOfAny(Path.GetInvalidFileNameChars()) >= 0)
            {
                return "CustomResourceReadout_InvalidFilename".Translate();
            }
            try
            {
                Path.GetFullPath(Path.Combine(ResourceReadoutModeDef.exportPath, name + ResourceReadoutModeDef.exportExt));
            }
            catch
            {
                return "CustomResourceReadout_InvalidFilename".Translate();
            }
            return base.NameIsValid(name);
        }

        protected override void OnRenamed(string name)
        {
            exporting.ToDef().Export(name);
        }
    }
}
