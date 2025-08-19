using HarmonyLib;
using UnityEngine;

namespace PeakGeneralImprovements.Patches
{
    internal static class GUIManagerPatch
    {
        private static AudioSource fogSFX;
        private static AudioSource lavaSFX;

        [HarmonyPatch(typeof(GUIManager), nameof(Start))]
        [HarmonyPostfix]
        private static void Start(GUIManager __instance)
        {
            fogSFX = __instance.fogRises.GetComponentInChildren<AudioSource>();
            lavaSFX = __instance.lavaRises.GetComponentInChildren<AudioSource>();

            if (fogSFX != null) fogSFX.playOnAwake = false;
            if (lavaSFX != null) lavaSFX.playOnAwake = false;
        }

        [HarmonyPatch(typeof(GUIManager), nameof(GUIManager.TheFogRises))]
        [HarmonyPostfix]
        private static void TheFogRises()
        {
            if (Plugin.PlayFogRisesSoundEachTime.Value && fogSFX != null)
            {
                fogSFX.Play();
            }
        }

        [HarmonyPatch(typeof(GUIManager), nameof(GUIManager.TheLavaRises))]
        [HarmonyPostfix]
        private static void TheLavaRises()
        {
            if (Plugin.PlayFogRisesSoundEachTime.Value && lavaSFX != null)
            {
                lavaSFX.Play();
            }
        }
    }
}