using HarmonyLib;
using UnityEngine.SceneManagement;

namespace PeakGeneralImprovements.Patches
{
    internal static class PretitlePatch
    {
        [HarmonyPatch(typeof(Pretitle), nameof(Start))]
        [HarmonyPostfix]
        private static void Start()
        {
            if (Plugin.SkipPretitleScreen.Value)
            {
                Plugin.MLS.LogMessage("Skipping pre-title screen.");
                SceneManager.LoadScene("Title", LoadSceneMode.Single);
            }
        }
    }
}