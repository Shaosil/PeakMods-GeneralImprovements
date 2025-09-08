using System.Collections.Generic;
using System.Linq;
using HarmonyLib;
using UnityEngine;
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

        internal static bool CharacterIsInRangeOfAnyCampfire(Character character)
        {
            return character && AllCampfires.Any(c => c && c.transform && Vector3.Distance(c.transform.position, character.Center) <= 30);
        }

        [HarmonyPatch(typeof(Campfire), nameof(Awake))]
        [HarmonyPostfix]
        private static void Awake(Campfire __instance)
        {
            AllCampfires.Add(__instance);
        }
    }
}