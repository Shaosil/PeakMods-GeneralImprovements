using System;
using System.Collections.Generic;
using HarmonyLib;
using Photon.Pun;

namespace PeakGeneralImprovements.Patches
{
    internal static class LavaPatch
    {
        [HarmonyPatch(typeof(Lava), "TryCookItems")]
        [HarmonyTranspiler]
        private static IEnumerable<CodeInstruction> TryCookItems(IEnumerable<CodeInstruction> instructions)
        {
            CodeMatcher matcher = new CodeMatcher(instructions);

            Plugin.MLS.LogDebug("Transpiling Lava.TryCookItems to destroy incinerated items.");
            matcher.MatchForward(true, new CodeMatch(i => i.Calls(typeof(ItemCooking).GetMethod(nameof(ItemCooking.FinishCooking)))))
                .SetInstruction(Transpilers.EmitDelegate<Action<ItemCooking>>(i =>
                {
                    if (PhotonNetwork.IsMasterClient)
                    {
                        // If the item is already fully cooked, send out a destroy event. Otherwise, finish cooking as usual
                        int timesCooked = i.item.GetData<IntItemData>(DataEntryKey.CookedAmount).Value;
                        if (timesCooked <= 3)
                        {
                            i.FinishCooking();
                        }
                        else
                        {
                            Plugin.MLS.LogInfo($"{i.item.GetName()} has been incinerated in lava. Destroying.");
                            PhotonNetwork.Destroy(i.photonView);
                        }
                    }
                }));

            return matcher.InstructionEnumeration();
        }
    }
}