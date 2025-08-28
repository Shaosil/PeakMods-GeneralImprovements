using System;
using System.Reflection;
using System.Reflection.Emit;
using HarmonyLib;
using static PeakGeneralImprovements.Enums;

namespace PeakGeneralImprovements.Patches.Shared
{
    internal static class RopesAndVines
    {
        public static void TranspileRopeAndVineUpdate(CodeMatcher matcher, Type classType)
        {
            if (Plugin.RopeVineChainBehavior.Value != eRopeVineChainOptions.Vanilla)
            {
                CodeInstruction[] thisCharInstructions = new[] { new CodeInstruction(OpCodes.Ldarg_0),
                   new CodeInstruction(OpCodes.Ldfld, classType.GetField("character", BindingFlags.Instance | BindingFlags.NonPublic)) };

                string patchText = Plugin.RopeVineChainBehavior.Value == eRopeVineChainOptions.AllowClimbing ? "allow climbing while on ropes/vines/chains."
                    : "automatically dismount and attempt to climb at the end of ropes/vines/chains.";
                Label? trueLabel = null;

                matcher.MatchForward(true,
                    // this.character.IsLocal &&
                    new CodeMatch(thisCharInstructions[0]),
                    new CodeMatch(thisCharInstructions[1]),
                    new CodeMatch(i => i.Calls(typeof(Character).GetMethod("get_IsLocal"))),
                    new CodeMatch(i => i.Branches(out _)),

                    // this.character.input.jumpWasPressed
                    new CodeMatch(thisCharInstructions[0]),
                    new CodeMatch(thisCharInstructions[1]),
                    new CodeMatch(i => i.LoadsField(typeof(Character).GetField(nameof(Character.input)))),
                    new CodeMatch(i => i.LoadsField(typeof(CharacterInput).GetField(nameof(CharacterInput.jumpWasPressed)))),
                    new CodeMatch(i => i.Branches(out trueLabel)),
                    new CodeMatch(i => true));

                if (matcher.IsValid)
                {
                    Plugin.MLS.LogDebug($"Transpiling {classType.Name}.Update to {patchText}");

                    // this.character.data.isClimbing
                    matcher.InsertAndAdvance(
                        thisCharInstructions[0],
                        thisCharInstructions[1],
                        new CodeInstruction(OpCodes.Ldfld, typeof(Character).GetField(nameof(Character.data))),
                        new CodeInstruction(OpCodes.Ldfld, typeof(CharacterData).GetField(nameof(CharacterData.isClimbing))),
                        new CodeInstruction(OpCodes.Brtrue_S, trueLabel)
                    );

                    // If auto dismounting is desired, also support that
                    if (Plugin.RopeVineChainBehavior.Value == eRopeVineChainOptions.AutoDismount)
                    {
                        // Create a label at the current position before inserting new code so we can jump to it
                        matcher.CreateLabel(out Label orLabel);

                        matcher.Insert(
                            // Load this.character on to stack and test against it
                            thisCharInstructions[0],
                            thisCharInstructions[1],
                            Transpilers.EmitDelegate<Func<Character, bool>>(c => ShouldAutoDismount(c, classType)),

                            // If false, branch to next check
                            new CodeInstruction(OpCodes.Brfalse_S, orLabel),

                            // Otherwise, we need to attempt a climb and then branch to the original true label
                            thisCharInstructions[0],
                            thisCharInstructions[1],
                            Transpilers.EmitDelegate<Action<Character>>(c => c.refs.climbing.TryToStartWallClimb(true, c.data.lookDirection, false)),
                            new CodeInstruction(OpCodes.Br_S, trueLabel)
                        );
                    }
                }
                else
                {
                    Plugin.MLS.LogWarning($"Unexpected IL code when trying to transpile {classType.Name}.Update. Rope/vine/chain behavior will not be changed!");
                }
            }
        }

        private static bool ShouldAutoDismount(Character character, Type classType)
        {
            // Should dismount if climb percent is at the end and y movement matches
            bool isRope = classType == typeof(CharacterRopeHandling);
            bool goingUp = character.input.movementInput.y > 0;
            float percent = isRope ? character.data.ropePercent : character.data.vinePercent;

            return (goingUp && percent >= 0.99f) || (!goingUp && percent <= 0.01f);
        }
    }
}