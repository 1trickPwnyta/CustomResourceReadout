using System;
using Verse;

namespace CustomResourceReadout
{
    public class Dialog_RenameResourceReadoutMode : Dialog_CustomRename<ResourceReadoutMode>
    {
        private Action callback;
        private string title;

        public Dialog_RenameResourceReadoutMode(ResourceReadoutMode renaming, Action callback = null, string title = null) : base(renaming)
        {
            this.callback = callback;
            this.title = title;
        }

        protected override int MaxNameLength => 64;

        public override TaggedString Title => title ?? base.Title;

        protected override AcceptanceReport NameIsValid(string name)
        {
            if (CustomResourceReadoutSettings.customModes.Any(m => m.name.EqualsIgnoreCase(name)))
            {
                return "CustomResourceReadout_NameAlreadyExists".Translate();
            }
            return base.NameIsValid(name);
        }

        protected override void OnRenamed(string name)
        {
            if (callback != null)
            {
                callback();
            }
        }
    }
}
