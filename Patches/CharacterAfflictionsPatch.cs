using HarmonyLib;
using UnityEngine;

namespace PeakGeneralImprovements.Patches
{
    internal static class CharacterAfflictionsPatch
    {
        private static float _hungerSkipLogCountdown = 5;

        [HarmonyPatch(typeof(CharacterAfflictions), nameof(CharacterAfflictions.AddStatus))]
        [HarmonyPrefix]
        private static bool AddStatus(CharacterAfflictions __instance, CharacterAfflictions.STATUSTYPE statusType, float amount)
        {
            // If configured, being close to any campfire should prevent hunger
            if (Plugin.CampfiresPreventHunger.Value && __instance.character.IsLocal && statusType == CharacterAfflictions.STATUSTYPE.Hunger && amount > 0)
            {
                bool characterCloseToCampfire = CampfirePatch.CharacterIsInRangeOfAnyCampfire(__instance.character);
                if (characterCloseToCampfire)
                {
                    _hungerSkipLogCountdown -= Time.deltaTime;
                    if (_hungerSkipLogCountdown <= 0)
                    {
                        Plugin.MLS.LogDebug("Skipping hunger growth since player is near a campfire.");
                        _hungerSkipLogCountdown = 5;
                    }
                }

                return !characterCloseToCampfire;
            }

            return true;
        }
    }
}