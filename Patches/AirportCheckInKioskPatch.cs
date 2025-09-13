using System.Collections;
using HarmonyLib;
using PeakGeneralImprovements.Objects;
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
            if (Plugin.FixAirportRope.Value)
            {
                __instance.StartCoroutine(WaitToSpawnRope());
            }

            if (Plugin.AirportElevatorSpawnBehavior.Value != Enums.eAirportElevatorOptions.Vanilla)
            {
                SpawnPoint[] existingSpawnPoints = Object.FindObjectsByType<SpawnPoint>(FindObjectsSortMode.None);
                if (existingSpawnPoints.Length == 1)
                {
                    Plugin.MLS.LogInfo("Creating more spawn points in the other elevators.");

                    // When the airport loads, create the non networked spawn points so we can start making use of them right away
                    existingSpawnPoints[0].index = 1;
                    float[] xPositions = new float[] { -15.2f, -4.2f, 1.4f };
                    for (int i = 0; i < xPositions.Length; i++)
                    {
                        Vector3 existingPos = existingSpawnPoints[0].transform.position;
                        SpawnPoint newSpawn = Object.Instantiate(existingSpawnPoints[0], new Vector3(xPositions[i], existingPos.y, existingPos.z), existingSpawnPoints[0].transform.rotation, existingSpawnPoints[0].transform.parent);
                        newSpawn.index = i < 1 ? i : i + 1; // Skip past index 1 since the existing spawn point should use that
                    }

                    // Now add the updater so we can monitor player counts and activate elevator animations
                    AirportCheckInKioskUpdater updater = __instance.gameObject.AddComponent<AirportCheckInKioskUpdater>();
                }
                else
                {
                    Plugin.MLS.LogWarning("Multiple active spawnpoints already found in airport! Not creating more for the other elevators.");
                }
            }
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