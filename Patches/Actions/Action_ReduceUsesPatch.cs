using HarmonyLib;

namespace PeakGeneralImprovements.Patches
{
    internal static class Action_ReduceUsesPatch
    {
        [HarmonyPatch(typeof(Action_ReduceUses), nameof(Action_ReduceUses.ReduceUsesRPC))]
        [HarmonyPostfix]
        private static void ReduceUsesRPC(Action_ReduceUses __instance)
        {
            if (Plugin.ConsumableItemsGetLighter.Value && __instance.character.IsLocal)
            {
                __instance.character.refs.afflictions.UpdateWeight();
            }
        }
    }
}