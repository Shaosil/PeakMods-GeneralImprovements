using System.Collections.Generic;
using System.Reflection.Emit;
using HarmonyLib;

namespace PeakGeneralImprovements.Patches
{
    internal static class RopeAnchorWithRopePatch
    {

        [HarmonyPatch(typeof(RopeAnchorWithRope), nameof(RopeAnchorWithRope.OnJoinedRoom))]
        [HarmonyTranspiler]
        private static IEnumerable<CodeInstruction> OnJoinedRoom_Transpiler(IEnumerable<CodeInstruction> instructions)
        {
            CodeMatcher matcher = new CodeMatcher(instructions);

            if (Plugin.FixAirportRope.Value)
            {
                Plugin.MLS.LogDebug("Transpiling RopeAnchorWithRope.OnJoinedRoom to fix climbing wall rope only working one time.");

                // this.SpawnRope();
                matcher.MatchForward(false,
                    new CodeMatch(i => i.IsLdarg(0)),
                    new CodeMatch(i => i.Calls(typeof(RopeAnchorWithRope).GetMethod(nameof(RopeAnchorWithRope.SpawnRope)))),
                    new CodeMatch(OpCodes.Pop));

                if (matcher.IsValid)
                {
                    matcher.RemoveInstructions(3);
                }
                else
                {
                    Plugin.MLS.LogWarning("Unexpected IL code - Could not transpile RopeAnchorWithRope.OnJoinedRoom to fix climbing wall rope only working one time!");
                }
            }

            return matcher.InstructionEnumeration();
        }
    }
}