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
            if (row.ButtonIcon(tex, tooltip: "CustomResourceReadout_ResourceReadoutButtonTip".Translate() + CustomResourceReadoutSettings.CurrentModeLabel))
            {
                Find.WindowStack.Add(new FloatMenu(new[]
                {
                    new FloatMenuOption("CustomResourceReadout_BasicUncategorized".Translate(), () =>
                    {
                        CustomResourceReadoutSettings.ChangeResourceReadout(ResourceReadoutModeType.Simple);
                    }),
                    new FloatMenuOption("CustomResourceReadout_BasicCategorized".Translate(), () =>
                    {
                        CustomResourceReadoutSettings.ChangeResourceReadout(ResourceReadoutModeType.Categorized);
                    })
                }.Concat(CustomResourceReadoutSettings.CustomResourceReadoutModes.Select(m => new FloatMenuOption(m.name, () =>
                {
                    CustomResourceReadoutSettings.ChangeResourceReadout(ResourceReadoutModeType.Custom, mode: m);
                })))
                .ConcatIfNotNull(DefDatabase<ResourceReadoutModeDef>.AllDefsListForReading.Count > 0 ? new[]
                {
                    new FloatMenuOption("CustomResourceReadout_Presets".Translate(), () =>
                    {
                        Find.WindowStack.Add(new FloatMenu(DefDatabase<ResourceReadoutModeDef>.AllDefsListForReading.Select(d => new FloatMenuOption(d.label, () =>
                        {
                            CustomResourceReadoutSettings.ChangeResourceReadout(ResourceReadoutModeType.Preset, preset: d);
                        })).ToList()));
                    })
                } : null)
                .Append(new FloatMenuOption("CustomResourceReadout_EditCustomResourceReadoutModes".Translate(), () =>
                {
                    Find.WindowStack.Add(new Dialog_ModSettings(CustomResourceReadoutMod.Mod));
                }))
                .ToList()));
            }
        }
    }
}
