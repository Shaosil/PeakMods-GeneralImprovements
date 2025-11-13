using System.Collections.Generic;
using HarmonyLib;
using PeakGeneralImprovements.Patches.Shared;

namespace PeakGeneralImprovements.Patches
{
    internal static class MainMenuPatch
    {
        [HarmonyPatch(typeof(MainMenu), "PlaySoloClicked")]
        [HarmonyTranspiler]
        private static IEnumerable<CodeInstruction> PlaySoloClicked_Transpiler(IEnumerable<CodeInstruction> instructions)
        {
            CodeMatcher matcher = new CodeMatcher(instructions);
            AirportLobbySkip.TranspileLoadingScreenType(matcher, "MainMenu.PlaySoloClicked");
            return matcher.InstructionEnumeration();
        }

        [HarmonyPatch(typeof(MainMenu), "StartOfflineModeRoutine", MethodType.Enumerator)]
        [HarmonyTranspiler]
        private static IEnumerable<CodeInstruction> StartOfflineModeRoutine_Transpiler(IEnumerable<CodeInstruction> instructions)
        {
            CodeMatcher matcher = new CodeMatcher(instructions);
            AirportLobbySkip.TranspileStartGameButtons(matcher, "MainMenu.StartOfflineModeRoutine");
            return matcher.InstructionEnumeration();
        }
    }
}