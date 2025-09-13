using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Photon.Pun;
using UnityEngine;

namespace PeakGeneralImprovements.Objects
{
    internal class AirportCheckInKioskUpdater : MonoBehaviour
    {
        private HashSet<int> _spawnedPlayerIDs = new HashSet<int>();
        private Dictionary<SpawnPoint, Animator> _spawnPointElevatorAnimators = new Dictionary<SpawnPoint, Animator>();

        private void Start()
        {
            if (Plugin.AirportElevatorDoorsAlwaysAnimate.Value)
            {
                // Associate each spawn point with the elevator closest to it
                var allElevators = FindObjectsByType<GameObject>(FindObjectsSortMode.None)
                    .Where(g => g.TryGetComponent(out Animator animator) && animator.runtimeAnimatorController?.name == "Elevator")
                    .Select(g => g.GetComponent<Animator>())
                    .ToList();

                foreach (var sp in SpawnPoint.allSpawnPoints)
                {
                    _spawnPointElevatorAnimators[sp] = allElevators.OrderBy(e => Vector3.Distance(e.transform.position, sp.transform.position)).FirstOrDefault();
                }
            }
        }

        private void Update()
        {
            int[] connectedPlayerIDs = PhotonNetwork.PlayerList.Select(p => p.ActorNumber).ToArray();

            // If there are any players that we haven't yet processed and the character is loaded (player joins)...
            foreach (var connectedPlayer in connectedPlayerIDs)
            {
                if (!_spawnedPlayerIDs.Contains(connectedPlayer) && PlayerHandler.TryGetCharacter(connectedPlayer, out var character))
                {
                    _spawnedPlayerIDs.Add(connectedPlayer);

                    if (character.IsLocal) continue;

                    // As the host, warp them to the proper elevator
                    if (PhotonNetwork.IsMasterClient)
                    {
                        int elevatorIndex = Plugin.AirportElevatorSpawnBehavior.Value == Enums.eAirportElevatorOptions.UseAllInOrder
                            ? connectedPlayer % SpawnPoint.allSpawnPoints.Count // Sequential
                            : Random.Range(0, SpawnPoint.allSpawnPoints.Count); // Random

                        SpawnPoint sp = SpawnPoint.allSpawnPoints.FirstOrDefault(s => s.index == elevatorIndex)
                            ?? SpawnPoint.allSpawnPoints.ElementAtOrDefault(elevatorIndex)
                            ?? SpawnPoint.allSpawnPoints[0];

                        Plugin.MLS.LogInfo($"{character.characterName} joined - warping them to elevator spawn index {sp.index}.");
                        character.photonView.RPC(nameof(Character.WarpPlayerRPC), RpcTarget.All, new object[] { sp.transform.position, false });
                    }

                    if (Plugin.AirportElevatorDoorsAlwaysAnimate.Value)
                    {
                        // Play their corresponding elevator animation
                        StartCoroutine(OpenElevatorDoors(character));
                    }
                }
            }

            // Also clear items from our list that no longer exist (player leaves)
            _spawnedPlayerIDs.RemoveWhere(p => !connectedPlayerIDs.Contains(p));
        }

        private IEnumerator OpenElevatorDoors(Character character)
        {
            // Give the character a second to be moved to another elevator by the host
            yield return new WaitForSeconds(1);

            var closestElevator = _spawnPointElevatorAnimators.OrderBy(e => Vector3.Distance(e.Key.transform.position, character.Center)).FirstOrDefault();
            if (closestElevator.Value)
            {
                Plugin.MLS.LogInfo($"Playing elevator index {closestElevator.Key.index}'s animation locally for {character.characterName}'s spawn position.");
                closestElevator.Value.Play(string.Empty);
            }
        }
    }
}