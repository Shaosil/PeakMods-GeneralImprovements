using BepInEx;
using BepInEx.Configuration;
using BepInEx.Logging;
using GeneralImprovements.Patches;
using HarmonyLib;

namespace GeneralImprovements
{
    [BepInPlugin(Metadata.GUID, Metadata.PLUGIN_NAME, Metadata.VERSION)]
    public class Plugin : BaseUnityPlugin
    {
        public static ManualLogSource MLS { get; private set; }

        private const string InventorySection = "Inventory";
        public static ConfigEntry<bool> Placeholder { get; private set; }

        private void Awake()
        {
            MLS = Logger;

            BindConfigs();
            MLS.LogInfo("Configuration Initialized.");

            var harmony = new Harmony(Metadata.GUID);

            harmony.PatchAll(typeof(CharacterItemsPatch));
            MLS.LogInfo("CharacterItems patched.");

            MLS.LogInfo($"{Metadata.PLUGIN_NAME} v{Metadata.VERSION} fully loaded.");
        }

        public void BindConfigs()
        {
            // Placeholder
            //Placeholder = Config.Bind(InventorySection, nameof(Placeholder), false, "If set to true, fixes something.");
        }
    }
}