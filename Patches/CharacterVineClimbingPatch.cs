using System.Collections.Generic;
using System.Reflection.Emit;
using HarmonyLib;
using PeakGeneralImprovements.Patches.Shared;

namespace PeakGeneralImprovements.Patches
{
    internal static class CharacterVineClimbingPatch
    {
        [HarmonyPatch(typeof(CharacterVineClimbing), nameof(Update))]
        [HarmonyTranspiler]
        private static IEnumerable<CodeInstruction> Update(IEnumerable<CodeInstruction> instructions, ILGenerator generator)
        {
            CodeMatcher matcher = new CodeMatcher(instructions, generator);
            RopesAndVines.TranspileRopeAndVineUpdate(matcher, typeof(CharacterVineClimbing));
            return matcher.InstructionEnumeration();
        }
    }
}