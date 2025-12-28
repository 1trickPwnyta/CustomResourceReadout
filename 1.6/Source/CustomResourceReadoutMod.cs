using HarmonyLib;
using Verse;

namespace CustomResourceReadout
{
    public class CustomResourceReadoutMod : Mod
    {
        public const string PACKAGE_ID = "customresourcereadout.1trickPwnyta";
        public const string PACKAGE_NAME = "Custom Resource Readout";

        public CustomResourceReadoutMod(ModContentPack content) : base(content)
        {
            var harmony = new Harmony(PACKAGE_ID);
            harmony.PatchAll();

            Log.Info("Ready.");
        }
    }
}
