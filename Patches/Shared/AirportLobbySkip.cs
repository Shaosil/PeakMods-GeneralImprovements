using System;
using System.Reflection.Emit;
using HarmonyLib;
using Photon.Pun;
using Zorro.Core;

namespace PeakGeneralImprovements.Patches.Shared
{
    internal static class AirportLobbySkip
    {
        public static bool ShouldSkipAirport = false;

        internal static void TranspileLoadingScreenType(CodeMatcher matcher, string methodDesc)
        {
            if (Plugin.AllowAirportLobbySkip.Value)
            {
                matcher.MatchForward(false,
                    new CodeMatch(i => i.LoadsConstant(0)),
                    new CodeMatch(OpCodes.Ldnull),
                    new CodeMatch(i => i.LoadsConstant(1)),
                    new CodeMatch(OpCodes.Newarr));

                if (matcher.IsValid)
                {
                    Plugin.MLS.LogDebug($"Transpiling {methodDesc} to dynamically assign loading screen type.");
                    matcher.SetInstruction(Transpilers.EmitDelegate<Func<int>>(GetCurrentLoadingScreenType));
                }
                else
                {
                    Plugin.MLS.LogWarning($"Unexpected IL code when trying to transpile {methodDesc}.");
                }
            }
        }

        public static void TranspileStartGameButtons(CodeMatcher matcher, string methodDesc = null)
        {
            if (Plugin.AllowAirportLobbySkip.Value)
            {
                // LoadSceneProcess("Airport")
                matcher.MatchForward(false,
                    new CodeMatch(OpCodes.Ldstr, "Airport"),
                    new CodeMatch(i => i.LoadsConstant()),
                    new CodeMatch(i => i.LoadsConstant()),
                    new CodeMatch(i => i.LoadsConstant()),
                    new CodeMatch(i => i.Calls(typeof(LoadingScreenHandler).GetMethod("LoadSceneProcess"))));

                if (matcher.IsValid)
                {
                    if (!string.IsNullOrWhiteSpace(methodDesc)) Plugin.MLS.LogDebug($"Transpiling {methodDesc} to dynamically assign scene name.");
                    matcher.SetInstruction(Transpilers.EmitDelegate<Func<string>>(GetCurrentLoadingSceneName));
                }
                else
                {
                    Plugin.MLS.LogWarning($"Unexpected IL code when trying to transpile {methodDesc}. Airport lobby may not be skipped!");
                }
            }
        }

        private static int GetCurrentLoadingScreenType()
        {
            return (int)(ShouldSkipAirport ? LoadingScreen.LoadingScreenType.Plane : LoadingScreen.LoadingScreenType.Basic);
        }

        private static string GetCurrentLoadingSceneName()
        {
            if (!ShouldSkipAirport)
            {
                return "Airport";
            }    

            // Mostly copied from AirportCheckInKiosk.LoadIslandMaster
            NextLevelService service = GameHandler.GetService<NextLevelService>();
            MapBaker mapBaker = SingletonAsset<MapBaker>.Instance;
            string sceneToLoad = null;

            if (service.Data.IsSome) sceneToLoad = mapBaker.GetLevel(service.Data.Value.CurrentLevelIndex);
            else if (PhotonNetwork.OfflineMode) sceneToLoad = mapBaker.GetLevel(0);

            if (string.IsNullOrWhiteSpace(sceneToLoad)) sceneToLoad = "WilIsland";

            Plugin.MLS.LogInfo($"Skipping airport lobby, going directly to level '{sceneToLoad}' at ascent level {Plugin.SkipAirportUsesAscentNum}.");
            GameHandler.AddStatus<SceneSwitchingStatus>(new SceneSwitchingStatus());
            Ascents.currentAscent = Plugin.SkipAirportUsesAscentNum;

            return sceneToLoad;
        }
    }
}