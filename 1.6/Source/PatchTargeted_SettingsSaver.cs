using HarmonyLib;
using System.Collections.Generic;
using System.Reflection;
using Verse;
using Verse.Profile;

namespace CustomResourceReadout
{
    [HarmonyPatch]
    public static class PatchTargeted_SettingsSaver
    {
        public static IEnumerable<MethodBase> TargetMethods()
        {
            yield return typeof(MemoryUtility).Method(nameof(MemoryUtility.ClearAllMapsAndWorld));
            yield return typeof(Root).Method(nameof(Root.Shutdown));
        }

        public static void Prefix()
        {
            if (CustomResourceReadoutSettings.dirty)
            {
                CustomResourceReadoutMod.Settings.Write();
                CustomResourceReadoutSettings.dirty = false;
            }
        }
    }
}
