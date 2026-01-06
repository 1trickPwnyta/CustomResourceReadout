using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Verse;
using Verse.Sound;

namespace CustomResourceReadout
{
    public class Dialog_SelectIcon : Window
    {
        private List<string> iconPathList;
        private Dictionary<string, Texture2D> icons;
        private List<Color> colors = new[] { Color.white, Color.black }.Concat(Enumerable.Range(0, 32).Select(i => Color.HSVToRGB(i / 32f, 0.7f, 1f))).ToList();

        private string title;
        private Action<string, Color> callback;
        private float height;
        private Vector2 scrollPosition;
        private string selectedIconPath;
        private Color selectedIconColor;
        private QuickSearchWidget search = new QuickSearchWidget();
        private bool focusedSearch;

        public override Vector2 InitialSize => new Vector2(600f, 500f);
        
        public Dialog_SelectIcon(string title, Action<string, Color> callback, string defaultIconPath = null, Color? defaultIconColor = null)
        {
            this.title = title;
            this.callback = callback;
            selectedIconPath = defaultIconPath;
            selectedIconColor = defaultIconColor ?? Color.white;
            doCloseX = true;
            closeOnClickedOutside = true;

            iconPathList = DefDatabase<ThingCategoryDef>.AllDefsListForReading.Select(d => d.iconPath)
                .Concat(DefDatabase<ThingDef>.AllDefsListForReading.Select(d => d.uiIconPath ?? (d.graphicData?.graphicClass == typeof(Graphic_Single) ? d.graphicData.texPath : null)))
                .Concat(DefDatabase<FactionDef>.AllDefsListForReading.Select(d => d.factionIconPath))
                .Concat(DefDatabase<IdeoIconDef>.AllDefsListForReading.Select(d => d.iconPath))
                .Concat(DefDatabase<StyleCategoryDef>.AllDefsListForReading.Select(d => d.iconPath))
                .Where(p => !p.NullOrEmpty()).Distinct().ToList();
            icons = new Dictionary<string, Texture2D>();
            foreach (string iconPath in iconPathList)
            {
                icons[iconPath] = ContentFinder<Texture2D>.Get(iconPath);
            }
        }

        public override void DoWindowContents(Rect inRect)
        {
            using (new TextBlock(GameFont.Medium)) Widgets.Label(inRect, title);
            inRect.yMin += 40f;
            Widgets.Label(inRect, "CustomResourceReadout_SelectIcon".Translate());
            Rect colorRect = new Rect(inRect.xMax - Text.LineHeight, inRect.y, Text.LineHeight, Text.LineHeight);
            using (new TextBlock(TextAnchor.UpperRight)) Widgets.Label(inRect.LeftPartPixels(inRect.width - colorRect.width), "CustomResourceReadout_SelectColor".Translate());
            Widgets.DrawRectFast(colorRect, selectedIconColor);
            if (Mouse.IsOver(colorRect))
            {
                Widgets.DrawBoxSolidWithOutline(colorRect, selectedIconColor, Color.white);
            }
            if (Widgets.ButtonInvisible(colorRect))
            {
                Find.WindowStack.Add(new Dialog_ChooseColor("CustomResourceReadout_SelectColor".Translate(), selectedIconColor, colors, c => selectedIconColor = c));
            }
            inRect.yMin += 30f;

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
                if (selectedIconPath != null)
                {
                    callback(selectedIconPath, selectedIconColor);
                    Close();
                }
                else
                {
                    Messages.Message("CustomResourceReadout_SelectIcon".Translate(), MessageTypeDefOf.RejectInput, false);
                }
            }
            inRect.yMax -= buttonRect.height + 10f;

            Rect searchRect = inRect.BottomPartPixels(30f);
            searchRect.width = 300f;
            search.OnGUI(searchRect.ContractedBy(2f));
            inRect.yMax -= searchRect.height + 10f;

            Rect outRect = inRect;
            Rect viewRect = new Rect(0f, 0f, inRect.width - 20f, height);
            Widgets.BeginScrollView(outRect, ref scrollPosition, viewRect);

            Rect iconRect = new Rect(viewRect.x, viewRect.y, viewRect.width / 12, viewRect.width / 12);
            foreach (string iconPath in iconPathList.Where(p => search.filter.Matches(p)))
            {
                if (iconRect.x >= viewRect.xMax)
                {
                    iconRect.x = viewRect.x;
                    iconRect.y += iconRect.height;
                }
                if (selectedIconPath == iconPath)
                {
                    Widgets.DrawBoxSolidWithOutline(iconRect, Widgets.MenuSectionBGFillColor, Widgets.HighlightStrongBgColor, 2);
                }

                GUI.color = selectedIconColor;
                Widgets.DrawTextureFitted(iconRect.ContractedBy(viewRect.width / 48), icons[iconPath], 1f);
                GUI.color = Color.white;

                Widgets.DrawHighlightIfMouseover(iconRect);
                if (Widgets.ButtonInvisible(iconRect))
                {
                    selectedIconPath = iconPath;
                    SoundDefOf.Click.PlayOneShot(null);
                }
                iconRect.x += iconRect.width;
            }
            height = iconRect.yMax;

            Widgets.EndScrollView();

            if (!focusedSearch)
            {
                search.Focus();
                focusedSearch = true;
            }
        }
    }
}
