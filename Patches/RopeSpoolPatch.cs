using HarmonyLib;

namespace PeakGeneralImprovements.Patches
{
    internal static class RopeSpoolPatch
    {
        [HarmonyPatch(typeof(RopeSpool), "set_RopeFuel")]
        [HarmonyPostfix]
        private static void set_RopeFuel(Item ___item)
        {
            if (Plugin.ConsumableItemsGetLighter.Value && ___item && ___item.holderCharacter && ___item.holderCharacter.IsLocal)
            {
                ___item.holderCharacter.refs.afflictions.UpdateWeight();
            }
        }
    }
}