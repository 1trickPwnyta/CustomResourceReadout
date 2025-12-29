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
            if (row.ButtonIcon(tex, tooltip: "CustomResourceReadout_ResourceReadoutButtonTip".Translate()))
            {
                Find.WindowStack.Add(new FloatMenu(CustomResourceReadoutSettings.customModes.Select(m => new FloatMenuOption(m.name, () =>
                {
                    CustomResourceReadoutSettings.currentMode = m; // TODO Need to ensure this gets written to the config file at some point
                })).ToList()));
            }
        }
    }
}
