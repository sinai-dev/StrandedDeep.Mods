using Beam;
using UnityEngine;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MordhauVoices.Voices;
using MordhauVoices.RPC;
using System.Collections;

namespace MordhauVoices
{
    public static class Menu
    {
        enum GUIState
        {
            Closed,
            Page1,
            Page2,
            Page3,
            LOOP_BACK
        }

        public static bool BlockToolbeltHotkeys { get; private set; }

        static bool guiAllowed;
        static GUIState guiState;

        static GUISkin guiSkin;
        static readonly Rect guiRect = new(30, 30, 300, 178);

        internal static void Init()
        {
            guiSkin = UnityEngine.Object.Instantiate(GUI.skin);
            guiSkin.hideFlags = HideFlags.HideAndDontSave;
            guiSkin.label.fontSize = 16;
        }

        internal static void OnGUI()
        {
            if (!guiAllowed)
                return;

            if ((int)guiState > (int)GUIState.Closed)
            {
                if (!guiSkin)
                    Init();

                var origSkin = GUI.skin;
                GUI.skin = guiSkin;

                GUILayout.BeginArea(guiRect, GUI.skin.box);

                // Iterate from 1 to 5, same as our Input check iteration
                for (int i = 1; i < 6; i++)
                {
                    GUILayout.BeginHorizontal(GUI.skin.button, GUILayout.Height(20));
                    GUILayout.Label($"[{i}]: {TranslateChoiceToLineType(i)}");
                    GUILayout.EndHorizontal();
                }

                GUILayout.EndArea();

                GUI.skin = origSkin;
            }
        }

        internal static void Update()
        {
            guiAllowed = (Game.State == GameState.LOAD_GAME || Game.State == GameState.NEW_GAME) 
                         && PlayerRegistry.AllPlayers?.Count > 0;

            if (!guiAllowed)
                return;

            if (Input.anyKeyDown)
            {
                // check for menu show/toggle input
                if (Input.GetKeyDown(Plugin.VoiceMenuKey.Value))
                {
                    guiState = (GUIState)((int)guiState + 1);

                    if (guiState == GUIState.LOOP_BACK)
                        guiState = GUIState.Page1;
                }

                // Check for voice line input if showing menu.
                if (guiState != GUIState.Closed)
                {
                    // Iterate from 1-5 since we check for a keycode "Alpha{i}"
                    // and we also send the index to TranslateChoiceToLineType
                    for (int i = 1; i < 6; i++)
                        CheckMenuChoice(i);
                }
            }
        }

        // Translates a choice (between 1 and 5) into a VoiceLineTypes enum.
        // First we add an offset of 5 depending on (GUI page - 1). So Page1 = 0, Page2 = 5, Page3 = 10.
        // Then add the choice value to get the VoiceLineTypes enum value (0 is NONE, they start at 1)
        static VoiceLineTypes TranslateChoiceToLineType(int choice)
        {
            return (VoiceLineTypes)((((int)guiState - 1) * 5) + choice);
        }

        static void CheckMenuChoice(int choice)
        {
            Enum.TryParse($"Alpha{choice}", out KeyCode keyCode);

            if (!Input.GetKeyDown(keyCode))
                return;

            if (!VoicePlayer.IsLocalPlayerBlockedFromSending())
                RPCManager.SendVoiceLine(TranslateChoiceToLineType(choice));

            // Always run this even if we were blocked from sending.
            // We would still want to close the menu and prevent the Toolbelt hotkey from firing.
            Plugin.Instance.StartCoroutine(OnChoicePressed(keyCode));
        }

        static IEnumerator OnChoicePressed(KeyCode keyCode)
        {
            guiState = GUIState.Closed;
            BlockToolbeltHotkeys = true;

            // Always wait at least one frame
            yield return null;

            // Wait until the key is no longer pressed
            while (Input.GetKey(keyCode))
                yield return null;

            // Wait one more frame
            yield return null;

            BlockToolbeltHotkeys = false;
        }
    }
}
