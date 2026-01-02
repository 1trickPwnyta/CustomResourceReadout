using HarmonyLib;
using RimWorld;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Emit;
using UnityEngine;
using Verse;

namespace CustomResourceReadout
{
    [HarmonyPatch(typeof(PlaySettings))]
    [HarmonyPatch("DoMapControls")]
    public static class Patch_PlaySettings
    {
        public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
        {
            List<CodeInstruction> instructionsList = instructions.ToList();
            int index = instructionsList.FindIndex(i => i.LoadsConstant("CategorizedResourceReadoutToggleButton"));
            instructionsList[index + 5].opcode = OpCodes.Call;
            instructionsList[index + 5].operand = typeof(Patch_PlaySettings).Method(nameof(DoResourceReadoutButton));
            return instructionsList;
        }

        private static void DoResourceReadoutButton(WidgetRow row, ref bool _, Texture2D tex, string __, SoundDef mouseoverSound, string ___)
        {
            if (row.ButtonIcon(tex, tooltip: "CustomResourceReadout_ResourceReadoutButtonTip".Translate(CustomResourceReadoutSettings.CurrentModeLabel)))
            {
                Find.WindowStack.Add(new FloatMenu(new[]
                {
                    new FloatMenuOption("CustomResourceReadout_BasicUncategorized".Translate(), () =>
                    {
                        ChangeResourceReadout(ResourceReadoutModeType.Simple);
                    }),
                    new FloatMenuOption("CustomResourceReadout_BasicCategorized".Translate(), () =>
                    {
                        ChangeResourceReadout(ResourceReadoutModeType.Categorized);
                    })
                }.Concat(CustomResourceReadoutSettings.customModes.Select(m => new FloatMenuOption(m.name, () =>
                {
                    ChangeResourceReadout(ResourceReadoutModeType.Custom, m);
                }))).ToList()));
            } // TODO Need to ensure this gets written to the config file at some point
        }

        private static void ChangeResourceReadout(ResourceReadoutModeType type, ResourceReadoutMode mode = null)
        {
            CustomResourceReadoutSettings.modeType = type;
            CustomResourceReadoutSettings.currentMode = mode;
            ResourceCounter.ResetDefs();
            foreach (Map map in Find.Maps)
            {
                map.resourceCounter.UpdateResourceCounts();
            }
        }
    }
}
