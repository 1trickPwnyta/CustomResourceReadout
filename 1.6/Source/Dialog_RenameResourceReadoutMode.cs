using System;
using Verse;

namespace CustomResourceReadout
{
    public class Dialog_RenameResourceReadoutMode : Dialog_Rename<ResourceReadoutMode>
    {
        private Action callback;

        public Dialog_RenameResourceReadoutMode(ResourceReadoutMode renaming, Action callback = null) : base(renaming)
        {
            this.callback = callback;
        }

        protected override int MaxNameLength => 64;

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
