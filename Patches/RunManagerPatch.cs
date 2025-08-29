using System;
using System.Collections.Generic;
using System.Linq;
using HarmonyLib;
using Photon.Pun;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace PeakGeneralImprovements.Patches
{
    internal static class RunManagerPatch
    {

        [HarmonyPatch(typeof(RunManager), nameof(RunManager.StartRun))]
        [HarmonyPostfix]
        private static void StartRun()
        {
            if (Plugin.BringPassportToIsland.Value && SceneManager.GetActiveScene().name != "Airport")
            {
                // Let SpawnItemInHand do all the work
                SpawnItemInHand spawner = Character.localCharacter.gameObject.AddComponent<SpawnItemInHand>();
                spawner.item = Resources.Load<GameObject>("0_Items/Passport").GetComponent<Item>();
            }

            // Reset run based variables
            CharacterItemsPatch.OurPassportGuid = Guid.Empty;
        }

        [HarmonyPatch(typeof(RunManager), nameof(RunManager.OnPlayerEnteredRoom))]
        [HarmonyPostfix]
        private static void OnPlayerEnteredRoom()
        {
            if (Plugin.SpawnMissingPropsOnLateJoins.Value && PhotonNetwork.IsMasterClient)
            {
                // Look for all spawners that have a requirement of the (now) current number of players, and try to spawn them.
                List<SingleItemSpawner> activeSpawners = UnityEngine.Object.FindObjectsByType<SingleItemSpawner>(FindObjectsSortMode.None)
                    .Where(s => s.playersInRoomRequirement == PhotonNetwork.PlayerList.Length && s.prefab).ToList();

                foreach (var spawner in activeSpawners)
                {
                    Plugin.MLS.LogInfo($"Trying to spawn missing {spawner.prefab.name} when new player joined.");
                    spawner.TrySpawnItems();
                }
            }
        }
    }
}