using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;
using BepInEx;
using BepInEx.Configuration;
using BepInEx.Logging;
using HarmonyLib;
using Steamworks;
using UnityEngine;
using static PeakGeneralImprovements.Enums;

namespace PeakGeneralImprovements
{
    [BepInPlugin(Metadata.GUID, Metadata.PLUGIN_NAME, Metadata.VERSION)]
    public class Plugin : BaseUnityPlugin
    {
        public static ManualLogSource MLS { get; private set; }

        private const string CampfireSection = "Campfire";
        public static ConfigEntry<bool> CampfiresPreventHunger { get; private set; }


        private const string ClimbingSection = "Climbing";
        public static ConfigEntry<eRopeVineChainOptions> RopeVineChainBehavior { get; private set; }


        private const string FogSection = "Fog";
        public static ConfigEntry<bool> DisableFogTimer { get; private set; }


        private const string GameplaySection = "Gameplay";
        public static ConfigEntry<bool> SpawnMissingPropsOnLateJoins { get; private set; }


        private const string GUISection = "GUI";
        public static ConfigEntry<bool> PlayFogRisesSoundEachTime { get; private set; }
        public static ConfigEntry<bool> SkipPretitleScreen { get; private set; }


        private const string InventorySection = "Inventory";
        public static ConfigEntry<bool> BringPassportToIsland { get; private set; }


        private const string MenuSection = "Menu";
        public static ConfigEntry<bool> SkipAirportLobby { get; private set; }
        public static ConfigEntry<string> SkipAirportUsesAscent { get; private set; }
        public static int SkipAirportUsesAscentNum;

        private void Awake()
        {
            MLS = Logger;

            BindConfigs();
            MigrateOldConfigValues();
            MLS.LogInfo("Configuration Initialized.");

            var harmony = new Harmony(Metadata.GUID);

            // Use reflection to patch all types that are defined in the project by namespace
            List<Type> allPatchables = Assembly.GetExecutingAssembly().GetTypes()
                .Where(t => t.Namespace == $"{GetType().Namespace}.Patches" && t.Name.EndsWith("Patch")).ToList();
            foreach (var patchable in allPatchables)
            {
                harmony.PatchAll(patchable);
                MLS.LogInfo($"{Regex.Replace(patchable.Name, "Patch$", string.Empty)} patched.");
            }

            MLS.LogInfo($"{Metadata.PLUGIN_NAME} v{Metadata.VERSION} fully loaded.");
        }

        public void BindConfigs()
        {
            // Campfire
            CampfiresPreventHunger = Config.Bind(CampfireSection, nameof(CampfiresPreventHunger), true, "If set to true, player will not get hungry when near a campfire.");

            // Climbing
            RopeVineChainBehavior = Config.Bind(ClimbingSection, nameof(RopeVineChainBehavior), eRopeVineChainOptions.AllowClimbing, $"Changes the behavior of rope/vine/chain climbing. {eRopeVineChainOptions.AllowClimbing} allows grabbing surfaces while still hanging on. {eRopeVineChainOptions.AutoDismount} will also automatically dismount at the ends and attempt to start climbing if possible.");

            // Fog
            DisableFogTimer = Config.Bind(FogSection, nameof(DisableFogTimer), true, "If set to true, the fog will stop at all campfires until a player triggers its rising again by climbing higher.");

            // Gameplay
            SpawnMissingPropsOnLateJoins = Config.Bind(GameplaySection, nameof(SpawnMissingPropsOnLateJoins), true, "[Host Only] If set to true, missing items (like upcoming marshmallows) will spawn when a player joins in the middle of a game.");

            // GUI
            PlayFogRisesSoundEachTime = Config.Bind(GUISection, nameof(PlayFogRisesSoundEachTime), true, "If set to true, the 'Fog Rises' sound effect will be played on all zones, not just the shore.");

            // Inventory
            BringPassportToIsland = Config.Bind(InventorySection, nameof(BringPassportToIsland), false, "If set to true, you will start on the island holding your passport. Useful when skipping airport.");

            // Menu
            SkipAirportLobby = Config.Bind(MenuSection, nameof(SkipAirportLobby), false, "If set to true, clicking Host Game or Play Offline will go directly to the island, bypassing the airport.");
            SkipAirportUsesAscent = Config.Bind(MenuSection, nameof(SkipAirportUsesAscent), string.Empty, $"If using {nameof(SkipAirportLobby)}, setting this will use the specified number as the ascent level. Leaving it blank will use the max ascent you've unlocked. Clamped from -1 to your max unlocked ascent.");
            SkipPretitleScreen = Config.Bind(MenuSection, nameof(SkipPretitleScreen), false, "If set to true, pre-title (intro) screen will be skipped on startup.");
        }

        /// <summary>
        /// To be called one time after the game loads in order to handle achievements or other things that need late binding
        /// </summary>
        public static void LateSanitize()
        {
            if (SkipAirportLobby.Value)
            {
                // Keep the ascent level to our maximum
                int maxAscent = 0;
                if (SteamManager.Initialized) SteamUserStats.GetStat("MaxAscent", out maxAscent);

                if (int.TryParse(SkipAirportUsesAscent.Value, out var wantedAscent))
                {
                    SkipAirportUsesAscentNum = Mathf.Clamp(maxAscent, -1, maxAscent);
                    MLS.LogDebug($"{nameof(SkipAirportUsesAscent)} specified {wantedAscent} (current max {maxAscent}). Using {SkipAirportUsesAscentNum} for quick start.");
                }
                else
                {
                    MLS.LogDebug($"{nameof(SkipAirportUsesAscent)} not specified. Using current max ascent ({maxAscent}) for quick start.");
                    SkipAirportUsesAscentNum = maxAscent;
                }
            }
        }

        private void MigrateOldConfigValues()
        {
            try
            {
                // Migrate and clear any orphans
                if (Config?.OrphanedEntries?.Any() ?? false)
                {
                    foreach (var orphan in Config.OrphanedEntries)
                    {
                        MigrateSpecificValue(orphan);
                    }

                    Config.OrphanedEntries.Clear();
                    Config.Save();
                }
            }
            catch (Exception ex)
            {
                MLS.LogError($"Error encountered while migrating old config values! This will not affect gameplay, but please verify your config file to ensure the settings are as you expect.\n\n{ex}");
            }
        }

        private void MigrateSpecificValue(KeyValuePair<ConfigDefinition, string> entry)
        {
            MLS.LogMessage($"Found unused config value: {entry.Key.Key}. Migrating and removing if possible...");

            switch (entry.Key.Key)
            {
                // Typo
                case "FogSection":
                    if (bool.TryParse(entry.Value, out bool disableFogTimer)) DisableFogTimer.Value = disableFogTimer;
                    break;

                // Entires that changed sections
                case "SkipPretitleScreen":
                    if (bool.TryParse(entry.Value, out bool skipPretitleScreen)) SkipPretitleScreen.Value = skipPretitleScreen;
                    break;

                default:
                    MLS.LogDebug("No matching migration");
                    break;
            }
        }
    }
}