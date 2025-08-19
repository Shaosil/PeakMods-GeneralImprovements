using System.Collections.Generic;
using HarmonyLib;

namespace PeakGeneralImprovements.Patches
{
    internal static class MapHandlerPatch
    {
        [HarmonyPatch(typeof(MapHandler), nameof(Awake))]
        [HarmonyPostfix]
        private static void Awake()
        {
            CampfirePatch.AllCampfires = new HashSet<Campfire>();
        }
    }
}