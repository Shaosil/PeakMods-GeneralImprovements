using BepInEx;
using BepInEx.Configuration;
using BepInEx.Logging;
using HarmonyLib;
using PeakGeneralImprovements.Patches;

namespace PeakGeneralImprovements
{
    [BepInPlugin(Metadata.GUID, Metadata.PLUGIN_NAME, Metadata.VERSION)]
    public class Plugin : BaseUnityPlugin
    {
        public static ManualLogSource MLS { get; private set; }

        private const string CampfireSection = "Campfire";
        public static ConfigEntry<bool> CampfiresPreventHunger { get; private set; }

        private const string FogSection = "Fog";
        public static ConfigEntry<bool> DisableFogTimer { get; private set; }

        private const string GUISection = "GUI";
        public static ConfigEntry<bool> PlayFogRisesSoundEachTime { get; private set; }
        public static ConfigEntry<bool> SkipPretitleScreen { get; private set; }

        private void Awake()
        {
            MLS = Logger;

            BindConfigs();
            MLS.LogInfo("Configuration Initialized.");

            var harmony = new Harmony(Metadata.GUID);

            harmony.PatchAll(typeof(CampfirePatch));
            MLS.LogInfo("Campfire patched.");

            harmony.PatchAll(typeof(CharacterPatch));
            MLS.LogInfo("Character patched.");

            harmony.PatchAll(typeof(CharacterAfflictionsPatch));
            MLS.LogInfo("CharacterAfflictions patched.");

            harmony.PatchAll(typeof(CharacterItemsPatch));
            MLS.LogInfo("CharacterItems patched.");

            harmony.PatchAll(typeof(GUIManagerPatch));
            MLS.LogInfo("GUIManager patched.");

            harmony.PatchAll(typeof(MapHandlerPatch));
            MLS.LogInfo("MapHandler patched.");

            harmony.PatchAll(typeof(OrbFogHandlerPatch));
            MLS.LogInfo("OrbFogHandler patched.");

            harmony.PatchAll(typeof(PretitlePatch));
            MLS.LogInfo("Pretitle patched.");

            MLS.LogInfo($"{Metadata.PLUGIN_NAME} v{Metadata.VERSION} fully loaded.");
        }

        public void BindConfigs()
        {
            // Campfire
            CampfiresPreventHunger = Config.Bind(CampfireSection, nameof(CampfiresPreventHunger), true, "If set to true, player will not get hungry when near a campfire.");

            // Fog
            DisableFogTimer = Config.Bind(FogSection, nameof(FogSection), true, "If set to true, the fog will stop at all campfires until a player triggers its rising again by climbing higher.");

            // GUI
            PlayFogRisesSoundEachTime = Config.Bind(GUISection, nameof(PlayFogRisesSoundEachTime), true, "If set to true, the 'Fog Rises' sound effect will be played on all zones, not just the shore.");
            SkipPretitleScreen = Config.Bind(GUISection, nameof(SkipPretitleScreen), false, "If set to true, pre-title (intro) screen will be skipped on startup.");
        }
    }
}