using System.Collections.Generic;
using System.Reflection.Emit;
using HarmonyLib;
using PeakGeneralImprovements.Utilities;

namespace PeakGeneralImprovements.Patches
{
    internal static class CharacterClimbingPatch
    {
        [HarmonyPatch(typeof(CharacterClimbing), nameof(CharacterClimbing.CanClimb))]
        [HarmonyTranspiler]
        private static IEnumerable<CodeInstruction> CanClimb_Transpiler(IEnumerable<CodeInstruction> instructions)
        {
            CodeMatcher matcher = new CodeMatcher(instructions);

            if (Plugin.RopeVineChainBehavior.Value != Enums.eRopeVineChainOptions.Vanilla)
            {
                string invalidMessage = "Unexpected IL code when trying to transpile CharacterClimbing.CanClimb. Rope/vine/chain behavior will not be changed!";

                // Store vine climbing label
                Label? vineBranch = null;
                var vineMatcher = new CodeMatcher(instructions);
                vineMatcher.MatchForward(true,
                    new CodeMatch(i => i.LoadsField(typeof(CharacterData).GetField(nameof(CharacterData.isVineClimbing)))),
                    new CodeMatch(i => i.Branches(out vineBranch)));

                // Point first label to third, change AND to OR, and remove LDC and RET
                matcher.MatchForward(true,
                    new CodeMatch(i => i.LoadsField(typeof(CharacterData).GetField(nameof(CharacterData.sinceClimb)))),
                    new CodeMatch(OpCodes.Ldc_R4),
                    new CodeMatch(i => i.Branches(out _)));
                if (!matcher.IsValid) return instructions.ReturnWithMessage(invalidMessage);

                matcher.SetOperandAndAdvance(vineBranch);
                matcher.RemoveInstructions(2);

                // Point second label to third, change AND to OR, and remove LDC and RET
                matcher.MatchForward(true,
                    new CodeMatch(i => i.LoadsField(typeof(CharacterData).GetField(nameof(CharacterData.isRopeClimbing)))),
                    new CodeMatch(i => i.Branches(out _)));
                if (!matcher.IsValid) return instructions.ReturnWithMessage(invalidMessage);

                matcher.SetAndAdvance(OpCodes.Brtrue_S, vineBranch);
                matcher.RemoveInstructions(2);

                // Change the final AND to OR
                matcher.MatchForward(true,
                    new CodeMatch(i => i.LoadsField(typeof(CharacterData).GetField(nameof(CharacterData.isVineClimbing)))),
                    new CodeMatch(i => i.Branches(out _)));
                if (!matcher.IsValid) return instructions.ReturnWithMessage(invalidMessage);

                Plugin.MLS.LogDebug("Transpiling CharacterClimbing.CanClimb to allow direct climbing from a rope/vine/chain.");
                matcher.Opcode = OpCodes.Brtrue_S;
            }

            return matcher.InstructionEnumeration();
        }

        [HarmonyPatch(typeof(CharacterClimbing), nameof(CharacterClimbing.TryToStartWallClimb))]
        [HarmonyPostfix]
        private static void Test()
        {

        }
    }
}