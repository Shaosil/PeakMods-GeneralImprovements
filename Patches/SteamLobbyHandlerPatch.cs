using System.Collections.Generic;
using HarmonyLib;
using PeakGeneralImprovements.Patches.Shared;

namespace PeakGeneralImprovements.Patches
{
    internal static class SteamLobbyHandlerPatch
    {
        [HarmonyPatch(typeof(SteamLobbyHandler), "OnLobbyCreated")]
        [HarmonyTranspiler]
        private static IEnumerable<CodeInstruction> OnLobbyCreated_Transpiler(IEnumerable<CodeInstruction> instructions)
        {
            CodeMatcher matcher = new CodeMatcher(instructions);
            AirportLobbySkip.TranspileLoadingScreenType(matcher, "SteamLobbyHandler.OnLobbyCreated");
            AirportLobbySkip.TranspileStartGameButtons(matcher);
            return matcher.InstructionEnumeration();
        }
    }
}