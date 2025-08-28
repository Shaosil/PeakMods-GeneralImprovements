#if ENABLE_CHEATMODE
using System;
using HarmonyLib;
using Photon.Pun;
using UnityEngine;
#endif

namespace PeakGeneralImprovements.Patches
{
    internal static class CharacterPatch
    {
#if ENABLE_CHEATMODE
        private static bool _cheatsEnabled = false;
        private static Tuple<float, float, float, float, float> _originalValues;

        [HarmonyPatch(typeof(Character), nameof(Update))]
        [HarmonyPostfix]
        private static void Update(Character __instance)
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

                if (Input.GetKeyDown(KeyCode.F2))
                {
                    if (CampfirePatch.CurrentFarthest?.transform != null)
                    {
                        var offset = new Vector3(UnityEngine.Random.Range(2, 5), 0, UnityEngine.Random.Range(2, 5));
                        var newPos = CampfirePatch.CurrentFarthest.transform.position + offset;
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
#endif
    }
}