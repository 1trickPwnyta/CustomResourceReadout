using HarmonyLib;
using RimWorld;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Emit;
using UnityEngine;
using Verse;

namespace CustomResourceReadout
{
    [HarmonyPatch(typeof(ResourceReadout))]
    [HarmonyPatch(nameof(ResourceReadout.ResourceReadoutOnGUI))]
    public static class Patch_ResourceReadout_ResourceReadoutOnGUI
    {
        public static bool Prefix(ResourceReadout __instance, ref float ___lastDrawnHeight, ref Vector2 ___scrollPosition)
        {
            if (CustomResourceReadoutSettings.CustomOrPresetMode)
            {
                if (Event.current.type != EventType.Layout && Current.ProgramState == ProgramState.Playing && Find.MainTabsRoot.OpenTab != MainButtonDefOf.Menu)
                {
                    GenUI.DrawTextWinterShadow(new Rect(256f, 512f, -256f, -512f));
                    Text.Font = GameFont.Small;
                    Rect outRect = new Rect(2f, 7f, 124f, (UI.screenHeight - 7) - 200f);
                    Rect viewRect = new Rect(0f, 0f, outRect.width, ___lastDrawnHeight);
                    bool scroll = viewRect.height > outRect.height;
                    if (scroll)
                    {
                        Widgets.BeginScrollView(outRect, ref ___scrollPosition, viewRect, false);
                    }
                    else
                    {
                        ___scrollPosition = Vector2.zero;
                        Widgets.BeginGroup(outRect);
                    }

                    Widgets.BeginGroup(viewRect);
                    float y = 0f;
                    using (new TextBlock(TextAnchor.MiddleLeft))
                    {
                        Dictionary<ThingDef, int> amounts = Find.CurrentMap.resourceCounter.AllCountedAmounts;
                        foreach (ResourceReadoutItem item in CustomResourceReadoutSettings.CurrentMode.Items)
                        {
                            Rect resourceRect = new Rect(0f, y, viewRect.width, 0f);
                            y += item.OnGUI(resourceRect, amounts);
                        }
                    }
                    ___lastDrawnHeight = y;
                    Widgets.EndGroup();

                    if (scroll)
                    {
                        Widgets.EndScrollView();
                    }
                    else
                    {
                        Widgets.EndGroup();
                    }
                }

                return false;
            }

            return true;
        }
    }

    [HarmonyPatch(typeof(ResourceReadout))]
    [HarmonyPatch(nameof(ResourceReadout.DrawResourceSimple))]
    public static class Patch_ResourceReadout_DrawResourceSimple
    {
        public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
        {
            List<CodeInstruction> instructionsList = instructions.ToList();
            int index = instructionsList.FindIndex(i => i.LoadsConstant(34f));
            instructionsList.InsertRange(index + 1, new[]
            {
                new CodeInstruction(OpCodes.Ldarga_S, 1),
                new CodeInstruction(OpCodes.Call, typeof(Rect).PropertyGetter(nameof(Rect.x))),
                new CodeInstruction(OpCodes.Add)
            });
            foreach (CodeInstruction instruction in instructionsList)
            {
                if (instruction.Calls(typeof(ResourceCounter).Method(nameof(ResourceCounter.GetCount))))
                {
                    instruction.opcode = OpCodes.Call;
                    instruction.operand = typeof(Patch_ResourceReadout_DrawResourceSimple).Method(nameof(GetCount));
                }
                yield return instruction;
            }
        }

        private static int GetCount(ResourceCounter counter, ThingDef def)
        {
            if (CustomResourceReadoutSettings.CustomOrPresetMode)
            {
                return counter.AllCountedAmounts[def];
            }
            else
            {
                return counter.GetCount(def);
            }
        }
    }
}
