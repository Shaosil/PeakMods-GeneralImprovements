using System;
using System.Collections.Generic;
using System.Reflection;
using System.Reflection.Emit;
using HarmonyLib;
using Photon.Pun;

namespace PeakGeneralImprovements.Patches
{
    internal static class CharacterAnimationsPatch
    {
        private static string _lastPlayedEmote = string.Empty;

        [HarmonyPatch(typeof(CharacterAnimations), nameof(CharacterAnimations.PlayEmote))]
        [HarmonyPostfix]
        private static void PlayEmote(Character ___character, string emoteName)
        {
            if (Plugin.EmoteLoopMode.Value != Enums.eEmoteLoopingOptions.None && ___character.IsLocal)
            {
                _lastPlayedEmote = emoteName;
            }
        }

        [HarmonyPatch(typeof(CharacterAnimations), "Update")]
        [HarmonyTranspiler]
        private static IEnumerable<CodeInstruction> Update_Transpiler(IEnumerable<CodeInstruction> instructions, ILGenerator generator)
        {
            CodeMatcher matcher = new CodeMatcher(instructions, generator);

            if (Plugin.EmoteLoopMode.Value != Enums.eEmoteLoopingOptions.None)
            {
                Func<IEnumerable<CodeInstruction>> returnWithWarning = () =>
                {
                    Plugin.MLS.LogWarning("Unexpected IL code when trying to transpile CharacterAnimations.Update. Emotes will not loop!");
                    return instructions;
                };
                FieldInfo sinceEmoteStartField = typeof(CharacterAnimations).GetField("sinceEmoteStart", BindingFlags.NonPublic | BindingFlags.Instance);

                // ... (sinceEmoteStart > 2f && (...
                matcher.MatchForward(false,
                    new CodeMatch(OpCodes.Ldarg_0),
                    new CodeMatch(i => i.LoadsField(sinceEmoteStartField)),
                    new CodeMatch(i => i.LoadsConstant(2f)),
                    new CodeMatch(i => i.Branches(out _)));

                if (matcher.IsInvalid) return returnWithWarning();

                // Simply delete the > 2f check and add a label to the next instruction
                matcher.RemoveInstructions(4).CreateLabel(out Label restOfIfLabel);

                // if (emoting &&...
                matcher.MatchBack(false,
                    new CodeMatch(OpCodes.Ldarg_0),
                    new CodeMatch(i => i.LoadsField(typeof(CharacterAnimations).GetField("emoting", BindingFlags.NonPublic | BindingFlags.Instance))),
                    new CodeMatch(i => i.Branches(out _)),
                    new CodeMatch(OpCodes.Ldarg_0));

                if (matcher.IsInvalid) return returnWithWarning();

                string desc = Plugin.EmoteLoopMode.Value == Enums.eEmoteLoopingOptions.NetworkedLooping ? "networked" : "local";
                Plugin.MLS.LogDebug($"Transpiling CharacterAnimations.Update to make emotes use {desc} looping until player moves.");

                // Throw in a should keep emoting check before the rest of the if statements
                matcher.InsertAndAdvance(new CodeInstruction(OpCodes.Ldarg_0));
                matcher.InsertAndAdvance(Transpilers.EmitDelegate<Func<CharacterAnimations, bool>>(ShouldKeepEmoting));
                matcher.InsertAndAdvance(new CodeInstruction(OpCodes.Brfalse_S, restOfIfLabel));
            }

            return matcher.InstructionEnumeration();
        }

        private static bool ShouldKeepEmoting(CharacterAnimations anim)
        {
            bool keepEmoting = anim.character.IsLocal && anim.sinceEmoteStart > 1.8f && !string.IsNullOrWhiteSpace(_lastPlayedEmote) && _lastPlayedEmote != "A_Scout_Emote_Flex";
            bool emoteIsFingerWag = _lastPlayedEmote == "A_Scout_Emote_Nono";

            if (keepEmoting && (Plugin.EmoteLoopMode.Value == Enums.eEmoteLoopingOptions.NetworkedLooping || emoteIsFingerWag))
            {
                if (Plugin.EmoteLoopMode.Value == Enums.eEmoteLoopingOptions.NetworkedLooping)
                {
                    // The finger wag is special and is the only one that doesn't loop, so include ourselves in that call
                    RpcTarget targets = emoteIsFingerWag ? RpcTarget.All : RpcTarget.Others;

                    // Send an emote ping to the necessary targets. It will look a little choppy to keep restarting it, but it's the only way to get it working on unmodded clients
                    anim.character.refs.view.RPC(nameof(CharacterAnimations.RPCA_PlayRemove), targets, new object[] { _lastPlayedEmote });
                }
                else if (emoteIsFingerWag)
                {
                    // Locally, just call the RPCA method manually to retrigger the animation
                    anim.RPCA_PlayRemove(_lastPlayedEmote);
                }

                // Reset variable for next check
                anim.sinceEmoteStart = 0f;
            }

            return keepEmoting;
        }
    }
}