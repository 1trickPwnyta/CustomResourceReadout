using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Verse;
using static Verse.Widgets;

namespace CustomResourceReadout
{
    public class Dialog_SelectIconColor : Dialog_ColorPickerBase
    {
        private static List<Color> colors = new[] { Color.white, Color.black }.Concat(Enumerable.Range(0, 32).Select(i => Color.HSVToRGB(i / 32f, 0.7f, 0.75f))).ToList();
        private static ColorComponents colorComponents = ColorComponents.Red | ColorComponents.Blue | ColorComponents.Green | ColorComponents.Hue | ColorComponents.Sat;

        private Action<Color> callback;

        public Dialog_SelectIconColor(Color current, Action<Color> callback) : base(colorComponents, colorComponents)
        {
            color = oldColor = current;
            this.callback = callback;
        }

        public override Vector2 InitialSize => new Vector2(650f, 450f);

        protected override bool ShowDarklight => false;

        protected override Color DefaultColor => Color.white;

        protected override List<Color> PickableColors => colors;

        protected override float ForcedColorValue => 0.75f;

        protected override bool ShowColorTemperatureBar => false;

        protected override void SaveColor(Color color)
        {
            callback(color);
        }
    }
}
