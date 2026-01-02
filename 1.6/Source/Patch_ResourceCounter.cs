using HarmonyLib;
using RimWorld;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using Verse;

namespace CustomResourceReadout
{
    [HarmonyPatch(typeof(ResourceCounter))]
    [HarmonyPatch(nameof(ResourceCounter.ResetDefs))]
    public static class Patch_ResourceCounter_ResetDefs
    {
        public static void Postfix()
        {
            if (CustomResourceReadoutSettings.modeType == ResourceReadoutModeType.Custom)
            {
                List<ThingDef> resources = typeof(ResourceCounter).Field("resources").GetValue(null) as List<ThingDef>;
                resources.AddRange(CustomResourceReadoutSettings.currentMode.items.SelectMany(i => i.ThingDefs).Where(d => !resources.Contains(d)));
            }
        }
    }

    [HarmonyPatch]
    public static class Patch_ResourceCounter_CountAsResource
    {
        public static IEnumerable<MethodBase> TargetMethods()
        {
            yield return typeof(ResourceCounter).Method(nameof(ResourceCounter.UpdateResourceCounts));
            yield return typeof(ResourceCounter).Method(nameof(ResourceCounter.CheckUpdateResource));
        }

        public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
        {
            foreach (CodeInstruction instruction in instructions)
            {
                if (instruction.Calls(typeof(ThingDef).PropertyGetter("CountAsResource")))
                {
                    instruction.opcode = OpCodes.Call;
                    instruction.operand = typeof(Patch_ResourceCounter_CountAsResource).Method(nameof(CountAsResource));
                }
                yield return instruction;
            }
        }

        private static bool CountAsResource(ThingDef def)
        {
            if (CustomResourceReadoutSettings.modeType == ResourceReadoutModeType.Custom)
            {
                return CustomResourceReadoutSettings.currentMode.items.Any(i => i.ThingDefs.Contains(def)); // TODO cache this using a hashtable
            }
            else
            {
                return def.CountAsResource;
            }
        }
    }
}
