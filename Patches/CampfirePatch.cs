using System.Collections.Generic;
using HarmonyLib;
using Zorro.Core;

namespace PeakGeneralImprovements.Patches
{
    internal static class CampfirePatch
    {
        internal static HashSet<Campfire> AllCampfires = new HashSet<Campfire>();

        internal static Campfire CurrentFarthest
        {
            get
            {
                var mapHandler = Singleton<MapHandler>.Instance;
                int curSeg = ((int?)mapHandler?.GetCurrentSegment()) ?? -1;
                return mapHandler?.segments[curSeg].segmentCampfire?.GetComponentInChildren<Campfire>();
            }
        }

        [HarmonyPatch(typeof(Campfire), nameof(Awake))]
        [HarmonyPostfix]
        private static void Awake(Campfire __instance)
        {
            AllCampfires.Add(__instance);
        }
    }
}