using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using HarmonyLib;
using PeakGeneralImprovements.Utilities;
using Photon.Pun;

namespace PeakGeneralImprovements.Patches
{
    internal static class LavaRisingPatch
    {
        [HarmonyPatch(typeof(LavaRising), "Update")]
        [HarmonyTranspiler]
        private static IEnumerable<CodeInstruction> Update_Transpiler(IEnumerable<CodeInstruction> instructions)
        {
            CodeMatcher matcher = new CodeMatcher(instructions);

            if (Plugin.KilnCampfireIsSafeZone.Value)
            {
                Label? falseLabel = null;

                // syncTime += Time.deltaTime; if (!this.started)...
                matcher.MatchForward(false,
                    new CodeMatch(OpCodes.Add),
                    new CodeMatch(i => i.StoresField(typeof(LavaRising).GetField("syncTime", BindingFlags.NonPublic | BindingFlags.Instance))),
                    new CodeMatch(OpCodes.Ldarg_0),
                    new CodeMatch(i => i.Calls(typeof(LavaRising).GetMethod("get_started"))),
                    new CodeMatch(i => i.Branches(out falseLabel)));

                if (matcher.IsInvalid) return instructions.ReturnWithMessage("Unexpected IL code when trying to transpile LavaRising.Update. Kiln campfire will NOT be a safe zone!");

                Plugin.MLS.LogDebug("Transpiling LavaRising.Update to allow the kiln campfire to act as a safe zone.");

                // If every live player is in range of a campfire, skip past the StartWaiting function call
                matcher.Advance(2);
                matcher.InsertAndAdvance(
                    new CodeInstruction(Transpilers.EmitDelegate<Func<bool>>(EveryoneAtCampfire)),
                    new CodeInstruction(OpCodes.Brtrue_S, falseLabel)
                );

            }

            return matcher.InstructionEnumeration();
        }

        private static bool EveryoneAtCampfire()
        {
            return PhotonNetwork.IsMasterClient && Character.AllCharacters.All(c => c && (c.data.dead || CampfirePatch.CharacterIsInRangeOfAnyCampfire(c)));
        }
    }
}