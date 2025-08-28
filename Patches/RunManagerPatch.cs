using System;
using HarmonyLib;
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
    }
}