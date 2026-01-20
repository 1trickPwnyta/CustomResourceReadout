using System;
using System.Linq;
using Verse;

namespace CustomResourceReadout
{
    public class Dialog_RenameResourceReadoutMode : Dialog_CustomRename<CustomResourceReadoutMode>
    {
        private Action<bool> callback;
        private string title;
        private bool successful = false;

        public Dialog_RenameResourceReadoutMode(CustomResourceReadoutMode renaming, Action<bool> callback = null, string title = null) : base(renaming)
        {
            this.callback = callback;
            this.title = title;
        }

        protected override int MaxNameLength => 64;

        public override TaggedString Title => title ?? base.Title;

        protected override AcceptanceReport NameIsValid(string name)
        {
            if (CustomResourceReadoutSettings.CustomResourceReadoutModes.Any(m => m.name.EqualsIgnoreCase(name)))
            {
                return "CustomResourceReadout_NameAlreadyExists".Translate();
            }
            return base.NameIsValid(name);
        }

        protected override void OnRenamed(string name)
        {
            successful = true;
        }

        public override void PostClose()
        {
            if (callback != null)
            {
                callback(successful);
            }
        }
    }
}
