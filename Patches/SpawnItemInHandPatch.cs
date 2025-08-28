using HarmonyLib;

namespace PeakGeneralImprovements.Patches
{
    internal static class SpawnItemInHandPatch
    {
        [HarmonyPatch(typeof(SpawnItemInHand), nameof(Start))]
        [HarmonyPostfix]
        private static void Start(SpawnItemInHand __instance)
        {
            if (Plugin.BringPassportToIsland.Value && (__instance?.item?.TryGetComponent<Action_Passport>(out _) ?? false))
            {
                Plugin.MLS.LogDebug("Overwriting Passport prefab to allow it to be dropped and thrown.");
                __instance.item.UIData.canDrop = true;
                __instance.item.UIData.canThrow = true;
            }
        }
    }
}