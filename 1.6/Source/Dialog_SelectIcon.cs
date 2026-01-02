using System;
using UnityEngine;
using Verse;

namespace CustomResourceReadout
{
    public class Dialog_SelectIcon : Window
    {
        private string title;
        private Action<string> callback;
        private float height;
        private Vector2 scrollPosition;

        public override Vector2 InitialSize => new Vector2(600f, 500f);

        public Dialog_SelectIcon(string title, Action<string> callback)
        {
            this.title = title;
            this.callback = callback;
            doCloseX = true;
            closeOnClickedOutside = true;
        }

        public override void DoWindowContents(Rect inRect)
        {
            using (new TextBlock(GameFont.Medium)) Widgets.Label(inRect, title);
            inRect.yMin += 40f;
            Widgets.Label(inRect, "CustomResourceReadout_SelectIcon".Translate());
            inRect.yMin += 30f;

            Rect outRect = inRect;
            outRect.yMax -= CloseButSize.y + 10f;
            Rect viewRect = new Rect(0f, 0f, inRect.width - 20f, height);
            Widgets.BeginScrollView(outRect, ref scrollPosition, viewRect);



            Widgets.EndScrollView();

            Rect buttonRect = inRect.BottomPartPixels(CloseButSize.y);
            buttonRect.xMin = buttonRect.width / 2 - 100f;
            buttonRect.width = 95f;
            if (Widgets.ButtonText(buttonRect, "Cancel".Translate()))
            {
                Close();
            }
            buttonRect.x += buttonRect.width + 10f;
            if (Widgets.ButtonText(buttonRect, "OK".Translate()))
            {
                callback();
            }
        }
    }
}
