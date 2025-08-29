using System.Collections.Generic;
using System.Reflection.Emit;
using HarmonyLib;

namespace PeakGeneralImprovements.Patches
{
    internal static class SimpleDrawMeshPatch
    {
        [HarmonyPatch(typeof(SimpleDrawMesh), nameof(SimpleDrawMesh.drawMeshes))]
        [HarmonyTranspiler]
        private static IEnumerable<CodeInstruction> drawMeshes_Transpiler(IEnumerable<CodeInstruction> instructions)
        {
            CodeMatcher matcher = new CodeMatcher(instructions);

            Plugin.MLS.LogDebug($"Transpiling {nameof(SimpleDrawMesh)}.{nameof(SimpleDrawMesh.drawMeshes)} to fix object culling for spectated players.");

            matcher.MatchForward(true, new CodeMatch(i => i.LoadsField(typeof(Character).GetField(nameof(Character.localCharacter)))))
                .Repeat(m => m.SetAndAdvance(OpCodes.Call, typeof(Character).GetMethod($"get_{nameof(Character.observedCharacter)}")));

            return matcher.InstructionEnumeration();
        }
    }
}