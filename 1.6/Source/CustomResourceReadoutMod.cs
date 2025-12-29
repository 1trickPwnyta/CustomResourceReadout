using HarmonyLib;
using UnityEngine;
using Verse;

namespace CustomResourceReadout
{
    [StaticConstructorOnStartup]
    public static class CustomResourceReadoutInitializer
    {
        static CustomResourceReadoutInitializer()
        {
            CustomResourceReadoutMod.Settings = CustomResourceReadoutMod.Mod.GetSettings<CustomResourceReadoutSettings>();
        }
    }

    public class CustomResourceReadoutMod : Mod
    {
        public const string PACKAGE_ID = "customresourcereadout.1trickPwnyta";
        public const string PACKAGE_NAME = "Custom Resource Readout";

        public static CustomResourceReadoutMod Mod;
        public static CustomResourceReadoutSettings Settings;

        public CustomResourceReadoutMod(ModContentPack content) : base(content)
        {
            Mod = this;

            var harmony = new Harmony(PACKAGE_ID);
            harmony.PatchAll();

            Log.Info("Ready.");
        }

        public override string SettingsCategory() => PACKAGE_NAME;

        public override void DoSettingsWindowContents(Rect inRect)
        {
            base.DoSettingsWindowContents(inRect);
            CustomResourceReadoutSettings.DoSettingsWindowContents(inRect);
        }
    }
}
