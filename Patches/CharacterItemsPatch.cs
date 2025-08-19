using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using HarmonyLib;
using UnityEngine;

namespace PeakGeneralImprovements.Patches
{
    internal static class CharacterItemsPatch
    {
        [HarmonyPatch(typeof(CharacterItems), "DoDropping")]
        [HarmonyTranspiler]
        private static IEnumerable<CodeInstruction> DoDropping_Transpiler(IEnumerable<CodeInstruction> instructions)
        {
            var matcher = new CodeMatcher(instructions);

            // Find the single variable set for position (stloc.0)
            matcher.MatchForward(true, new CodeMatch[]
            {
                new CodeMatch(i => i.Calls(typeof(CharacterData).GetMethod("get_currentItem"))),
                new CodeMatch(i => i.Calls(typeof(Component).GetMethod("get_transform"))),
                new CodeMatch(i => i.Calls(typeof(Transform).GetMethod("get_position"))),
                new CodeMatch(i => i.opcode == OpCodes.Stloc_0)
            });

            if (matcher.IsValid)
            {
                Plugin.MLS.LogDebug("Transpiling CharacterItems.DoDropping to fix items being able to fall through terrain when placed.");

                matcher.InsertAndAdvance(
                    // Load current item onto stack also
                    new CodeInstruction(OpCodes.Ldarg_0),
                    new CodeInstruction(OpCodes.Ldfld, typeof(CharacterItems).GetField("character", BindingFlags.Instance | BindingFlags.NonPublic)),
                    new CodeInstruction(OpCodes.Ldfld, typeof(Character).GetField(nameof(Character.data))),
                    new CodeInstruction(OpCodes.Callvirt, typeof(CharacterData).GetMethod("get_currentItem")),

                    Transpilers.EmitDelegate<Func<Vector3, Item, Vector3>>((originalPos, curItem) =>
                    {
                        // Linecast from camera to end of item collider - if we hit terrain, set that as our drop position
                        Transform camTransform = UnityEngine.Object.FindAnyObjectByType<MainCamera>().transform;
                        float itemRadius = curItem.colliders.Max(c => c.bounds.extents.magnitude);
                        Vector3 targetPos = originalPos + (camTransform.forward * itemRadius);

                        if (Physics.Linecast(camTransform.position, targetPos, out RaycastHit hitInfo, LayerMask.GetMask("Terrain", "Map", "InvisWall")))
                        {
                            Plugin.MLS.LogDebug("Item dropped close to terrain! Using closest contact point instead.");
                            originalPos = hitInfo.point - (camTransform.forward * itemRadius);
                        }

                        return originalPos;
                    })
                );
            }
            else
            {
                Plugin.MLS.LogWarning("Unexpected IL code when trying to transpile CharacterItems.DoDropping. Item dropping will NOT be fixed!");
            }

            return matcher.InstructionEnumeration();
        }
    }
}