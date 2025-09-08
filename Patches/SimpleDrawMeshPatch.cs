using System;
using System.Collections.Generic;
using System.Reflection;
using System.Reflection.Emit;
using HarmonyLib;
using UnityEngine;
using UnityEngine.Rendering;

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

            // Replace local character with observed character for culling distance
            matcher.MatchForward(true, new CodeMatch(i => i.LoadsField(typeof(Character).GetField(nameof(Character.localCharacter)))))
                .Repeat(m => m.SetAndAdvance(OpCodes.Call, typeof(Character).GetMethod($"get_{nameof(Character.observedCharacter)}")));

            // Turn receive shadows off for the DrawMeshInstanced call
            matcher.Advance(-1).SearchBack(m => m.operand is MethodBase method && method.Name == nameof(Graphics.DrawMeshInstanced));
            matcher.SetInstruction(Transpilers.EmitDelegate<Action<Mesh, int, Material, Matrix4x4[], int>>((mesh, i, mat, mtr, count) =>
            {
                Graphics.DrawMeshInstanced(mesh, i, mat, mtr, count, null, ShadowCastingMode.On, false);
            }));

            return matcher.InstructionEnumeration();
        }
    }
}