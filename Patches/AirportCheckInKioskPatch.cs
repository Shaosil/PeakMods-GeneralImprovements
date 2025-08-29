using System.Collections;
using HarmonyLib;
using Photon.Pun;
using UnityEngine;

namespace PeakGeneralImprovements.Patches
{
    internal static class AirportCheckInKioskPatch
    {
        [HarmonyPatch(typeof(AirportCheckInKiosk), nameof(Start))]
        [HarmonyPostfix]
        private static void Start(AirportCheckInKiosk __instance)
        {
            __instance.StartCoroutine(WaitToSpawnRope());
        }

        private static IEnumerator WaitToSpawnRope()
        {
            yield return new WaitUntil(() => PhotonNetwork.InRoom);

            // Force a rope spawn at the airport (it was removed from the RopeAnchorWithRope class)
            var ropeAnchors = Object.FindObjectsByType<RopeAnchorWithRope>(FindObjectsSortMode.None);
            foreach (var ropeAnchor in ropeAnchors)
            {
                ropeAnchor.SpawnRope();
            }
        }
    }
}