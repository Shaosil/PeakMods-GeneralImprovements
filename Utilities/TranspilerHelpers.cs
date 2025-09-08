using System.Collections.Generic;
using HarmonyLib;

namespace PeakGeneralImprovements.Utilities
{
    internal static class TranspilerHelpers
    {
        public static IEnumerable<CodeInstruction> ReturnWithMessage(this IEnumerable<CodeInstruction> instructions, string message)
        {
            Plugin.MLS.LogWarning(message);
            return instructions;
        }
    }
}
