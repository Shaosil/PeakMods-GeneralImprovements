using System.Collections.Generic;
using System.Linq;
using Photon.Pun;
using UnityEngine;

namespace PeakGeneralImprovements.Objects
{
    internal class AirportCheckInKioskUpdater : MonoBehaviourPun
    {
        private HashSet<int> _spawnedPlayerIDs = new HashSet<int>();
        private List<Animator> _elevatorAnimators = new List<Animator>();

        private void Start()
        {
            if (Plugin.AirportElevatorDoorsAlwaysAnimate.Value)
            {
                // Sort the elevators by their position and store their animators
                _elevatorAnimators = FindObjectsByType<GameObject>(FindObjectsSortMode.None)
                    .Where(g => g.TryGetComponent(out Animator animator) && animator.runtimeAnimatorController?.name == "Elevator")
                    .OrderBy(g => g.transform.position.x)
                    .Select(g => g.GetComponent<Animator>())
                    .ToList();
            }
        }

        private void Update()
        {
            // Only the host checks for joining players
            if (!PhotonNetwork.IsMasterClient) return;

            int[] connectedPlayerIDs = PhotonNetwork.PlayerList.Select(p => p.ActorNumber).ToArray();

            // If there are any players that we haven't yet processed and the character is loaded (player joins)...
            foreach (var connectedPlayer in connectedPlayerIDs)
            {
                if (!_spawnedPlayerIDs.Contains(connectedPlayer) && PlayerHandler.TryGetCharacter(connectedPlayer, out var character))
                {
                    _spawnedPlayerIDs.Add(connectedPlayer);

                    if (character.IsLocal) continue;

                    // Warp them to the proper elevator
                    int elevatorIndex = Plugin.AirportElevatorSpawnBehavior.Value == Enums.eAirportElevatorOptions.UseAllInOrder
                        ? connectedPlayer % SpawnPoint.allSpawnPoints.Count // Sequential
                        : Random.Range(0, SpawnPoint.allSpawnPoints.Count); // Random

                    SpawnPoint sp = SpawnPoint.allSpawnPoints.FirstOrDefault(s => s.index == elevatorIndex)
                        ?? SpawnPoint.allSpawnPoints.ElementAtOrDefault(elevatorIndex)
                        ?? SpawnPoint.allSpawnPoints[0];

                    // Send the signal to everyone to play their corresponding elevator animation and warp them there
                    Plugin.MLS.LogInfo($"{character.characterName} joined - warping them to elevator spawn index {sp.index}.");
                    photonView.RPC(nameof(OpenElevatorDoors), RpcTarget.All, new object[] { elevatorIndex, character.characterName });
                    character.photonView.RPC(nameof(Character.WarpPlayerRPC), RpcTarget.All, new object[] { sp.transform.position, false });
                }
            }

            // Also clear items from our list that no longer exist (player leaves)
            _spawnedPlayerIDs.RemoveWhere(p => !connectedPlayerIDs.Contains(p));
        }

        [PunRPC]
        private void OpenElevatorDoors(int elevatorIndex, string charName)
        {
            if (_elevatorAnimators.Count > elevatorIndex)
            {
                Plugin.MLS.LogInfo($"Playing elevator index {elevatorIndex}'s animation locally for {charName}'s spawn position.");
                _elevatorAnimators[elevatorIndex].Play(string.Empty);
            }
        }
    }
}