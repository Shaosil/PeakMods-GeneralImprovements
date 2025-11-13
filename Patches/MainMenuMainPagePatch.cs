using HarmonyLib;
using PeakGeneralImprovements.Patches.Shared;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace PeakGeneralImprovements.Patches
{
    internal static class MainMenuMainPagePatch
    {
        private static TextMeshProUGUI quickHostButtonText;
        private static TextMeshProUGUI quickSoloButtonText;

        [HarmonyPatch(typeof(MainMenuMainPage), nameof(Start))]
        [HarmonyPostfix]
        private static void Start()
        {
            // Add quick start buttons by copying others
            if (Plugin.AllowAirportLobbySkip.Value)
            {
                // Reset skip variable
                AirportLobbySkip.ShouldSkipAirport = false;

                // This has to be called after Steam starts up because it reads user stats. Run it each time the menu loads.
                int curAscent = Plugin.CalculateQuickStartAscent();

                Plugin.MLS.LogInfo("Adding quick start buttons to main menu.");

                var playWithFriendsButton = GameObject.Find("Button_PlayWithFriends");
                var settingsButton = GameObject.Find("Button_Settings");
                var quickHostButton = Object.Instantiate(settingsButton, settingsButton.transform.parent);
                var quickSoloButton = Object.Instantiate(settingsButton, settingsButton.transform.parent);

                quickHostButton.name = "Button_QuickHost";
                quickHostButton.transform.position = playWithFriendsButton.transform.position + new Vector3(-250, 100, 0);
                quickHostButton.GetComponentInChildren<Button>().onClick.AddListener(QuickStartMultiplayer);
                Object.Destroy(quickHostButton.GetComponentInChildren<LocalizedText>());
                quickHostButtonText = quickHostButton.GetComponentInChildren<TextMeshProUGUI>();

                quickSoloButton.name = "Button_QuickSolo";
                quickSoloButton.transform.position = playWithFriendsButton.transform.position + new Vector3(150, 120, 0);
                quickSoloButton.GetComponentInChildren<Button>().onClick.AddListener(QuickStartSolo);
                Object.Destroy(quickSoloButton.GetComponentInChildren<LocalizedText>());
                quickSoloButtonText = quickSoloButton.GetComponentInChildren<TextMeshProUGUI>();

                UpdateQuickAscent(curAscent);
            }
        }

        [HarmonyPatch(typeof(MainMenuMainPage), nameof(Update))]
        [HarmonyPostfix]
        private static void Update()
        {
            // Ascent -1
            if (Input.GetKeyDown(KeyCode.BackQuote)) UpdateQuickAscent(-1);

            // Ascent 0-7
            for (int i = 0; i <= 7; i++)
            {
                if (Input.GetKeyDown(KeyCode.Alpha0 + i) || Input.GetKeyDown(KeyCode.Keypad0 + i))
                {
                    UpdateQuickAscent(i);
                    break;
                }
            }
        }

        private static void UpdateQuickAscent(int newAscent)
        {
            Plugin.SkipAirportUsesAscentNum = newAscent;

            quickHostButtonText.text = $"QUICK HOST ({newAscent})";
            quickSoloButtonText.text = $"QUICK SOLO ({newAscent})";
        }

        private static void QuickStartMultiplayer()
        {
            AirportLobbySkip.ShouldSkipAirport = true;
            Object.FindFirstObjectByType<MainMenuMainPage>().m_playButton.onClick.Invoke();
        }

        private static void QuickStartSolo()
        {
            AirportLobbySkip.ShouldSkipAirport = true;
            Object.FindFirstObjectByType<MainMenu>().playSoloButton.onClick.Invoke();
        }
    }
}