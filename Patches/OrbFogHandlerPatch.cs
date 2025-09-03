using HarmonyLib;

namespace PeakGeneralImprovements.Patches
{
    internal static class OrbFogHandlerPatch
    {
        [HarmonyPatch(typeof(OrbFogHandler), nameof(TimeToMove))]
        [HarmonyPrefix]
        private static bool TimeToMove(ref bool __result)
        {
            // Disable fog activation timeout if needed
            if (Plugin.DisableFogTimer.Value)
            {
                __result = false;
                return false;
            }

            return true;
        }
    }
}