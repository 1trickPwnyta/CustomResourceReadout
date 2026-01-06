using HarmonyLib;
using Verse;

namespace CustomResourceReadout
{
    [HarmonyPatch(typeof(ReorderableWidget))]
    [HarmonyPatch("StopDragging")]
    public static class Patch_ReorderableWidget
    {
        public static void Postfix()
        {
            CustomResourceReadoutSettings.OnDragEnd();
        }
    }
}
