using HarmonyLib;
using RimWorld;
using System;
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
            if (CustomResourceReadoutSettings.CustomOrPresetMode)
            {
                List<ThingDef> resources = typeof(ResourceCounter).Field("resources").GetValue(null) as List<ThingDef>;
                resources.AddRange(CustomResourceReadoutSettings.CurrentMode.Items.SelectMany(i => i.ThingDefs).Where(d => !resources.Contains(d)));
            }
        }
    }

    [HarmonyPatch(typeof(ResourceCounter))]
    [HarmonyPatch("ResetResourceCounts")]
    public static class Patch_ResourceCounter_ResetResourceCounts
    {
        public static void Postfix(ResourceCounter __instance)
        {
            if (CustomResourceReadoutSettings.CustomOrPresetMode)
            {
                foreach (ResourceReadoutItem item in CustomResourceReadoutSettings.CurrentMode.Items)
                {
                    item.ResetCount();
                }
                __instance.GetCountedAmountsStuff().Clear();
            }
        }
    }

    [HarmonyPatch(typeof(ResourceCounter))]
    [HarmonyPatch(nameof(ResourceCounter.UpdateResourceCounts))]
    public static class Patch_ResourceCounter_UpdateResourceCounts
    {
        public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
        {
            List<CodeInstruction> instructionsList = instructions.ToList();
            int index = instructionsList.FindIndex(i => i.LoadsField(typeof(Thing).Field(nameof(Thing.stackCount))));
            instructionsList.InsertRange(index + 3, new[]
            {
                new CodeInstruction(OpCodes.Ldarg_0),
                new CodeInstruction(OpCodes.Ldloc_3),
                new CodeInstruction(OpCodes.Ldloc_S, 4),
                new CodeInstruction(OpCodes.Call, typeof(Patch_ResourceCounter_UpdateResourceCounts).Method(nameof(UpdateResourceCountsStuff)))
            });
            return instructionsList;
        }

        private static void UpdateResourceCountsStuff(ResourceCounter counter, Thing thing, Dictionary<ThingDef, int> countedAmountsNoStuff)
        {
            if (CustomResourceReadoutSettings.CustomOrPresetMode)
            {
                if (thing.def.MadeFromStuff && CustomResourceReadoutSettings.CurrentMode.Items.Any(i => i.ThingDefsStuff.Any(t => t.Item1 == thing.def)))
                {
                    Dictionary<Tuple<ThingDef, ThingDef>, int> countedAmountsStuff = counter.GetCountedAmountsStuff();
                    Tuple<ThingDef, ThingDef> key = new Tuple<ThingDef, ThingDef>(thing.def, thing.Stuff);
                    if (!countedAmountsStuff.ContainsKey(key))
                    {
                        countedAmountsStuff[key] = 0;
                    }
                    countedAmountsStuff[key] += thing.stackCount;
                    countedAmountsNoStuff[thing.def] -= thing.stackCount;
                }
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
                    instruction.operand = typeof(Utility).Method(nameof(Utility.CountAsResource));
                }
                yield return instruction;
            }
        }
    }
}
