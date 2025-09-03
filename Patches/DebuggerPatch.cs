#if ENABLE_CHEATMODE

using System;
using HarmonyLib;
using Photon.Pun;
using Steamworks;
using UnityEngine;
using Zorro.Core;

namespace PeakGeneralImprovements.Patches
{
    internal static class DebuggerPatch
    {
        private static bool _cheatsEnabled = false;
        private static bool _preventAchievements = false;
        private static Tuple<float, float, float, float, float> _originalValues;

        [HarmonyPatch(typeof(MainMenuMainPage), "Start")]
        [HarmonyPostfix]
        private static void MainMenu_Start()
        {
            if (_cheatsEnabled || _preventAchievements)
            {
                _cheatsEnabled = false;
                _preventAchievements = false;
                Plugin.MLS.LogError("CHEAT MODE HAS BEEN DISABLED - ACHIEVEMENTS ARE REENABLED UNTIL NEXT CHEAT ACTIVATED.");
            }
        }

        [HarmonyPatch(typeof(MainMenuMainPage), "Update")]
        [HarmonyPostfix]
        private static void MainMenuMainPage_Update()
        {
            if (Input.GetKeyDown(KeyCode.F1))
            {
                NextLevelService service = GameHandler.GetService<NextLevelService>();
                NextLevelService.NextLevelData newData = new NextLevelService.NextLevelData { CurrentLevelIndex = service.Data.Value.CurrentLevelIndex + 1, SecondsLeftFromQueryTime = 60 };
                service.Data = Optionable<NextLevelService.NextLevelData>.Some(newData);
                Plugin.MLS.LogError($"NEXT LEVEL INDEX HAS BEEN INCREMENTED ({newData.CurrentLevelIndex})");

                _preventAchievements = true;
            }
        }

        [HarmonyPatch(typeof(Character), "Update")]
        [HarmonyPostfix]
        private static void Character_Update(Character __instance)
        {
            if (__instance.IsLocal && PhotonNetwork.IsMasterClient)
            {
                if (Input.GetKeyDown(KeyCode.F1))
                {
                    _cheatsEnabled = !_cheatsEnabled;
                    var climbing = __instance.GetComponent<CharacterClimbing>();
                    var movement = __instance.GetComponent<CharacterMovement>();

                    if (_cheatsEnabled)
                    {
                        _originalValues = new Tuple<float, float, float, float, float>
                        (
                            climbing.climbSpeed,
                            movement.movementForce,
                            movement.sprintMultiplier,
                            movement.jumpImpulse,
                            movement.maxAngle
                        );

                        _preventAchievements = true;
                    }

                    __instance.infiniteStam = _cheatsEnabled;
                    climbing.climbSpeed = _cheatsEnabled ? 50 : _originalValues.Item1;
                    movement.movementForce = _cheatsEnabled ? 50 : _originalValues.Item2;
                    movement.sprintMultiplier = _cheatsEnabled ? 5 : _originalValues.Item3;
                    movement.jumpImpulse = _cheatsEnabled ? 5000 : _originalValues.Item4;
                    movement.maxAngle = _cheatsEnabled ? 80 : _originalValues.Item5;
                    __instance.data.isInvincible = _cheatsEnabled;

                    Plugin.MLS.LogError($"CHEAT MODE NOW {(_cheatsEnabled ? "ENABLED" : "DISABLED")}!");
                }

                if (_cheatsEnabled && Input.GetKeyDown(KeyCode.F2))
                {
                    if (CampfirePatch.CurrentFarthest?.transform != null)
                    {
                        int[] offsets = new int[2];
                        for (int i = 0; i < 2; i++) offsets[i] = UnityEngine.Random.Range(2, 5) * (int)Mathf.Sign(UnityEngine.Random.Range(-1, 1));
                        var newPos = CampfirePatch.CurrentFarthest.transform.position + new Vector3(offsets[0], 2, offsets[1]);
                        __instance.photonView.RPC(nameof(Character.WarpPlayerRPC), RpcTarget.All, new object[] { newPos, true });

                        Plugin.MLS.LogError("TELEPORTED TO FARTHEST CAMPFIRE!");
                    }
                    else
                    {
                        Plugin.MLS.LogError("COULD NOT FIND FARTHEST CAMPFIRE!");
                    }
                }
            }
        }

        [HarmonyPatch(typeof(Character), nameof(Fall))]
        [HarmonyPrefix]
        private static bool Fall()
        {
            return !_cheatsEnabled;
        }

        [HarmonyPatch(typeof(SteamUserStats), nameof(SteamUserStats.SetAchievement))]
        [HarmonyPatch(typeof(SteamUserStats), nameof(SteamUserStats.SetStat), typeof(string), typeof(int))]
        [HarmonyPatch(typeof(SteamUserStats), nameof(SteamUserStats.SetStat), typeof(string), typeof(float))]
        [HarmonyPrefix]
        private static bool PreventAchievements()
        {
            return !_preventAchievements;
        }
    }
}

#endif