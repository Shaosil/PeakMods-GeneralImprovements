using System.Collections.Generic;
using System.Reflection.Emit;
using HarmonyLib;
using PeakGeneralImprovements.Utilities;
using Photon.Pun;

namespace PeakGeneralImprovements.Patches
{
    internal static class OrbFogHandlerPatch
    {
        [HarmonyPatch(typeof(OrbFogHandler), nameof(TimeToMove))]
        [HarmonyPrefix]
        private static bool TimeToMove(ref bool __result)
        {
            // Disable fog activation timeout if needed
            if (Plugin.DisableFogTimer.Value && PhotonNetwork.IsMasterClient)
            {
                __result = false;
                return false;
            }

            return true;
        }

        [HarmonyPatch(typeof(OrbFogHandler), "PlayersHaveMovedOn")]
        [HarmonyTranspiler]
        private static IEnumerable<CodeInstruction> PlayersHaveMovedOn_Transpiler(IEnumerable<CodeInstruction> instructions)
        {
            CodeMatcher matcher = new CodeMatcher(instructions);

            if (Plugin.FixDeadPlayersPreventingFog.Value)
            {
                string invalidMessage = "Unexpected IL code when trying to transpile OrbFogHandler.PlayersHaveMovedOn. Dead players will still affect fog trigger!";
                Label? continueLable = null;

                // ...< this.currentStartForward
                matcher.MatchForward(false,
                    new CodeMatch(i => i.LoadsField(typeof(OrbFogHandler).GetField(nameof(OrbFogHandler.currentStartForward)))),
                    new CodeMatch(i => i.Branches(out continueLable)));

                if (matcher.IsInvalid) return instructions.ReturnWithMessage(invalidMessage);

                // Start of for loop
                matcher.MatchBack(true,
                    new CodeMatch(OpCodes.Ret),
                    new CodeMatch(OpCodes.Ldc_I4_0),
                    new CodeMatch(OpCodes.Stloc_0),
                    new CodeMatch(i => i.Branches(out continueLable)),
                    new CodeMatch(i => i.LoadsField(typeof(Character).GetField(nameof(Character.AllCharacters)))));

                if (matcher.IsInvalid) return instructions.ReturnWithMessage(invalidMessage);

                Plugin.MLS.LogDebug("Transpiling OrbFogHandler.PlayersHaveMovedOn to fix dead players affecting fog rising trigger.");

                // if (Character.AllCharacters[i].data.dead) continue;
                matcher.Advance(-1);
                matcher.InsertAndAdvance(
                    new CodeInstruction(OpCodes.Ldsfld, typeof(Character).GetField(nameof(Character.AllCharacters))),
                    new CodeInstruction(OpCodes.Ldloc_0),
                    new CodeInstruction(OpCodes.Callvirt, typeof(List<Character>).GetMethod("get_Item")),
                    new CodeInstruction(OpCodes.Ldfld, typeof(Character).GetField(nameof(Character.data))),
                    new CodeInstruction(OpCodes.Ldfld, typeof(CharacterData).GetField(nameof(CharacterData.dead))),
                    new CodeInstruction(OpCodes.Brtrue_S, continueLable)
                );
            }

            return matcher.InstructionEnumeration();
        }
    }
}