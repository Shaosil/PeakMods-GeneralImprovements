using System.Collections.Generic;
using HarmonyLib;

namespace PeakGeneralImprovements.Patches
{
    internal static class MapHandlerPatch
    {
        [HarmonyPatch(typeof(MapHandler), "Awake")]
        [HarmonyPatch(typeof(MapHandler), nameof(MapHandler.OnDestroy))]
        [HarmonyPostfix]
        private static void AwakeAndDestroy()
        {
            // Reset AllCampfires variable on awake and destroy of map handler
            CampfirePatch.AllCampfires = new HashSet<Campfire>();
        }
    }
}