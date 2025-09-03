using System;
using System.Collections.Generic;
using System.Reflection.Emit;
using HarmonyLib;
using UnityEngine;

namespace PeakGeneralImprovements.Patches
{
    internal static class HotSunPatch
    {
        private static ItemCooking _currentlyCookingItem = null;
        private static Dictionary<ItemCooking, float> _cookingItemsProgress = new Dictionary<ItemCooking, float>();

        [HarmonyPatch(typeof(HotSun), "Update")]
        [HarmonyTranspiler]
        private static IEnumerable<CodeInstruction> Update_Transpiler(IEnumerable<CodeInstruction> instructions, ILGenerator generator)
        {
            CodeMatcher matcher = new CodeMatcher(instructions, generator);

            if (Plugin.HotSunCooksShieldingItems.Value != Enums.eHotSunCookingOptions.None)
            {
                Label? retLabel = null;
                matcher.MatchForward(true,
                    new CodeMatch(i => i.Branches(out _)),          // If linecast didn't hit player, skip to ldloc below
                    new CodeMatch(i => i.LoadsConstant(1)),         // Set flag to true
                    new CodeMatch(i => i.IsStloc()),                // ^
                    new CodeMatch(i => i.IsLdloc()),                // Load flag
                    new CodeMatch(i => i.Branches(out retLabel)));  // If not flag (if linecast didn't work), branch to ret

                if (matcher.IsValid)
                {
                    Plugin.MLS.LogDebug($"Transpiling HotSun.Update to cook held{(Plugin.HotSunCooksShieldingItems.Value == Enums.eHotSunCookingOptions.OnlyFood ? " food " : " ")}items when shading player.");

                    // Squeeze the cooked items processing in here before the branch
                    matcher.InsertAndAdvance(Transpilers.EmitDelegate<Action>(ProcessCookingItems));

                    // Create label at true block and branch if true instead of return if false
                    matcher.CreateLabelAt(matcher.Pos + 1, out Label hitCharLabel);
                    matcher.SetAndAdvance(OpCodes.Brtrue_S, hitCharLabel);

                    // Load raycast hit info onto stack and pass it to our helper function, then branch to ret
                    matcher.InsertAndAdvance(new CodeInstruction(OpCodes.Ldloc_S, 1));
                    matcher.InsertAndAdvance(Transpilers.EmitDelegate<Action<RaycastHit>>(r => CheckItemCooking(r)));
                    matcher.InsertAndAdvance(new CodeInstruction(OpCodes.Br_S, retLabel));
                }
                else
                {
                    Plugin.MLS.LogWarning("Unexpected IL code when trying to transpile HotSun.Update. Items will NOT be cooked when used as shade!");
                }
            }

            return matcher.InstructionEnumeration();
        }

        private static void ProcessCookingItems()
        {
            // Remove and cancel the cooking of all items that are not the current one
            List<ItemCooking> itemsToRemove = new List<ItemCooking>();
            foreach (var item in _cookingItemsProgress)
            {
                if (item.Key != _currentlyCookingItem)
                {
                    itemsToRemove.Add(item.Key);
                }
            }

            // Clean up any tracked items that are no longer cooking
            foreach (var remove in itemsToRemove)
            {
                _cookingItemsProgress.Remove(remove);

                if (remove)
                {
                    remove.CancelCookingVisuals();
                    Plugin.MLS.LogDebug($"Item {remove.item.GetName()} cancelled cooking while blocking the sun.");
                }
            }

            if (_currentlyCookingItem)
            {
                // Increment progress (take 5 seconds to fully cook)
                _cookingItemsProgress[_currentlyCookingItem] += (0.2f * Time.deltaTime);
                if (_cookingItemsProgress[_currentlyCookingItem] >= 1)
                {
                    // Finish cooking and reset counter to keep going if it's not incenerated
                    _currentlyCookingItem.FinishCooking();

                    int cookedAmount = _currentlyCookingItem.item.GetData<IntItemData>(DataEntryKey.CookedAmount).Value;
                    if (cookedAmount < 4)
                    {
                        _currentlyCookingItem.StartCookingVisuals();
                        _cookingItemsProgress[_currentlyCookingItem] = 0;
                    }

                    string itemName = _currentlyCookingItem.item.GetName();
                    Plugin.MLS.LogDebug($"Item {itemName} finished cooking ({_currentlyCookingItem.item.GetItemName().Replace($" {itemName}", "")}) while blocking the sun.");
                }

                // Always clear the currently cooked item so it can be set afterwards
                _currentlyCookingItem = null;
            }
        }

        private static void CheckItemCooking(RaycastHit raycastHit)
        {
            // If the object we hit can be cooked and is currently held by the local player
            if (raycastHit.transform && raycastHit.transform.TryGetComponent(out ItemCooking itemCooking) && itemCooking.canBeCooked && Character.localCharacter.data.currentItem == itemCooking.item)
            {
                // Make sure it's not already incenerated
                int cookedAmount = itemCooking.item.GetData<IntItemData>(DataEntryKey.CookedAmount).Value;

                // Either everything cookable should be cooked, or only food (if it has the Action_RestoreHunger component)
                if (cookedAmount < 4 && Plugin.HotSunCooksShieldingItems.Value == Enums.eHotSunCookingOptions.AllCookables || itemCooking.TryGetComponent<Action_RestoreHunger>(out _))
                {
                    _currentlyCookingItem = itemCooking;

                    // If it's the first frame, it will be successfully added to the dictionary
                    if (_cookingItemsProgress.TryAdd(_currentlyCookingItem, 0))
                    {
                        _currentlyCookingItem.StartCookingVisuals();
                        Plugin.MLS.LogDebug($"Item {_currentlyCookingItem.item.GetName()} started cooking while blocking the sun.");
                    }
                }
            }
        }
    }
}