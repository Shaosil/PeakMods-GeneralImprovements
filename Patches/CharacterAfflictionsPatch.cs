using System;
using System.Collections.Generic;
using System.Reflection.Emit;
using HarmonyLib;
using PeakGeneralImprovements.Utilities;
using UnityEngine;

namespace PeakGeneralImprovements.Patches
{
    internal static class CharacterAfflictionsPatch
    {
        private static float _hungerSkipLogCountdown = 5;

        [HarmonyPatch(typeof(CharacterAfflictions), nameof(CharacterAfflictions.AddStatus))]
        [HarmonyPrefix]
        private static bool AddStatus(CharacterAfflictions __instance, CharacterAfflictions.STATUSTYPE statusType, float amount)
        {
            // If configured, being close to any campfire should prevent hunger
            if (Plugin.CampfiresPreventHunger.Value && __instance.character.IsLocal && statusType == CharacterAfflictions.STATUSTYPE.Hunger && amount > 0)
            {
                bool characterCloseToCampfire = CampfirePatch.CharacterIsInRangeOfAnyCampfire(__instance.character);
                if (characterCloseToCampfire)
                {
                    _hungerSkipLogCountdown -= Time.deltaTime;
                    if (_hungerSkipLogCountdown <= 0)
                    {
                        Plugin.MLS.LogDebug("Skipping hunger growth since player is near a campfire.");
                        _hungerSkipLogCountdown = 5;
                    }
                }

                return !characterCloseToCampfire;
            }

            return true;
        }

        [HarmonyPatch(typeof(CharacterAfflictions), "UpdateWeight")]
        [HarmonyTranspiler]
        private static IEnumerable<CodeInstruction> UpdateWeight_Transpiler(IEnumerable<CodeInstruction> instructions)
        {
            CodeMatcher matcher = new CodeMatcher(instructions);

            if (Plugin.ConsumableItemsGetLighter.Value)
            {
                // num += itemSlotVar.prefab.CarryWeight
                matcher.MatchForward(false,
                    new CodeMatch(i => i.IsLdloc()),
                    new CodeMatch(i => i.LoadsField(typeof(ItemSlot).GetField(nameof(ItemSlot.prefab)))),
                    new CodeMatch(i => i.Calls(typeof(Item).GetMethod("get_CarryWeight"))),
                    new CodeMatch(OpCodes.Add),
                    new CodeMatch(OpCodes.Stloc_0));

                if (matcher.IsValid)
                {
                    Plugin.MLS.LogDebug("Transpiling CharacterAfflictions.UpdateWeight to allow multi use consumable items to get lighter when used.");

                    matcher.Repeat(m =>
                    {
                        // Replace the CarryWeight call with our calculated percentage function, passing in the itemslot and our instance instead of the prefab
                        m.Advance(1);
                        m.SetInstructionAndAdvance(new CodeInstruction(OpCodes.Ldarg_0));
                        m.SetInstructionAndAdvance(Transpilers.EmitDelegate<Func<ItemSlot, CharacterAfflictions, int>>(CalculateModifiedCarryWeight));
                    });
                }
                else
                {
                    return instructions.ReturnWithMessage("Unexpected IL code when trying to transpile CharacterAfflictions.UpdateWeight. Multi use items will not get lighter!");
                }
            }

            return matcher.InstructionEnumeration();
        }

        private static int CalculateModifiedCarryWeight(ItemSlot itemSlot, CharacterAfflictions characterAfflictions)
        {
            RopeSpool spool = null;
            float percentLeft = 1f;
            int originalCarryWeight = itemSlot.prefab.CarryWeight;
            int modifiedCarryWeight = originalCarryWeight;

            // If this item defines a totalUses or is a rope of some type, change the weight
            if (characterAfflictions.character.IsLocal && (itemSlot.prefab.TryGetComponent(out spool) || itemSlot.prefab.totalUses > 0))
            {
                if (spool && itemSlot.data.TryGetDataEntry(DataEntryKey.Fuel, out FloatItemData currentFuel))
                {
                    percentLeft = currentFuel.Value / spool.ropeStartFuel;
                }
                else if (itemSlot.prefab.totalUses > 0 && itemSlot.data.TryGetDataEntry(DataEntryKey.ItemUses, out OptionableIntItemData uses) && uses.HasData)
                {
                    percentLeft = uses.Value / (float)itemSlot.prefab.totalUses;
                }

                if (percentLeft < 1f)
                {
                    modifiedCarryWeight = (int)Math.Round(originalCarryWeight * percentLeft);
                    Plugin.MLS.LogDebug($"Multi-use item weight calculation being overridden (for {characterAfflictions.character.characterName}'s {itemSlot.prefab.name}) - {Math.Round(percentLeft * 100, 2)}% ({modifiedCarryWeight}) of original weight ({originalCarryWeight}) being used.");
                }
            }

            return modifiedCarryWeight;
        }
    }
}