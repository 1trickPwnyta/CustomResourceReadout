using UnityEngine;
using Verse;

namespace CustomResourceReadout
{
    public class Dialog_CustomRename<T> : Dialog_Rename<T> where T : class, IRenameable
    {
        public virtual TaggedString Title => "Rename".Translate();

        public Dialog_CustomRename(T renaming) : base(renaming)
        {
        }

        public override void DoWindowContents(Rect inRect)
        {
            base.DoWindowContents(inRect);
            Rect titleRect = inRect;
            using (new TextBlock(GameFont.Medium))
            {
                titleRect.height = Text.LineHeight + 10f;
                Widgets.DrawRectFast(titleRect, Widgets.WindowBGFillColor);
                Widgets.Label(titleRect, Title);
            }
        }
    }
}
