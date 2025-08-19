using HarmonyLib;

namespace PeakGeneralImprovements.Patches
{
    internal static class OrbFogHandlerPatch
    {
        [HarmonyPatch(typeof(OrbFogHandler), nameof(TimeToMove))]
        [HarmonyPrefix]
        private static void TimeToMove(bool __result)
        {
            // Disable all fog activation timers if campfires should be safe zones
            if (Plugin.DisableFogTimer.Value)
            {
                __result = false;
            }
        }
    }
}