using System;
using System.Collections.Generic;
using System.Reflection.Emit;
using HarmonyLib;
using PeakGeneralImprovements.Utilities;
using Photon.Pun;

namespace PeakGeneralImprovements.Patches
{
    internal static class CharacterSpawnerPatch
    {
        [HarmonyPatch(typeof(CharacterSpawner), "SpawnLocalPlayer", MethodType.Enumerator)]
        [HarmonyTranspiler]
        private static IEnumerable<CodeInstruction> SpawnLocalPlayer_Transpiler(IEnumerable<CodeInstruction> instructions)
        {
            CodeMatcher matcher = new CodeMatcher(instructions);

            if (Plugin.AirportElevatorSpawnBehavior.Value == Enums.eAirportElevatorOptions.UseAllRandomly)
            {
                matcher.MatchForward(false,
                    new CodeMatch(i => i.Calls(typeof(PhotonNetwork).GetMethod("get_LocalPlayer"))),
                    new CodeMatch(i => i.Calls(typeof(Photon.Realtime.Player).GetMethod("get_ActorNumber"))),
                    new CodeMatch(i => i.LoadsField(typeof(SpawnPoint).GetField(nameof(SpawnPoint.allSpawnPoints)))),
                    new CodeMatch(i => i.Calls(typeof(List<SpawnPoint>).GetMethod("get_Count"))),
                    new CodeMatch(OpCodes.Rem),
                    new CodeMatch(OpCodes.Stfld));

                if (matcher.IsValid)
                {
                    Plugin.MLS.LogDebug("Transpiling CharacterSpawner.SpawnLocalPlayer to allow players to spawn in all elevators randomly.");

                    // Remove 4 instructions and replace the 5th with a random call
                    matcher.RemoveInstructions(4);
                    matcher.SetInstruction(Transpilers.EmitDelegate<Func<int>>(() => UnityEngine.Random.Range(0, SpawnPoint.allSpawnPoints.Count)));
                }
                else
                {
                    return instructions.ReturnWithMessage("Unexpected IL code when trying to transpile CharacterSpawner.SpawnLocalPlayer. Players will spawn in all elevators sequentially instead of randomly!");
                }
            }

            return matcher.InstructionEnumeration();
        }
    }
}