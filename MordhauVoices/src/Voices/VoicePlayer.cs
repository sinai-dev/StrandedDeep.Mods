using Beam;
using MordhauVoices.RPC;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace MordhauVoices.Voices
{
    public class VoicePlayer
    {
        static readonly Dictionary<int, AudioSource> audioPlayers = new();
        static readonly Dictionary<int, Coroutine> playingAudioCoroutine = new();

        static AudioManager AudioMgr => _audioManager ??= Resources.FindObjectsOfTypeAll<AudioManager>().FirstOrDefault();
        static AudioManager _audioManager;

        public static void Init()
        {
            for (int i = 0; i < 2; i++)
            {
                var obj = new GameObject($"AudioPlayer_P{i + 1}");
                GameObject.DontDestroyOnLoad(obj);
                obj.hideFlags = HideFlags.HideAndDontSave;
                var source = obj.AddComponent<AudioSource>();
                audioPlayers.Add(i, source);
            }
        }

        public static bool IsLocalPlayerBlockedFromSending()
        {
            return PlayerRegistry.LocalPlayer == null
                || playingAudioCoroutine.ContainsKey(PlayerRegistry.LocalPlayer.Id);
        }

        public static void PlayClip(int playerID, AudioClip clip, float pitch)
        {
            var player = PlayerRegistry.AllPlayers.FirstOrDefault(it => it.Id == playerID);
            if (player == null)
                return;

            var source = audioPlayers[playerID];
            source.clip = clip;
            source.pitch = pitch;
            source.volume = Plugin.VoiceVolume.Value;
            source.outputAudioMixerGroup = AudioMgr._environmentMixerGroup;

            float distance = Vector3.Distance(PlayerRegistry.LocalPlayer.transform.position, player.transform.position);
            if (distance > 125)
                source.volume = 0f;
            else if (distance > 0f)
                source.volume *= 1 - (1 / (125 / distance));

            Plugin.Log.LogMessage($"Playing clip {clip.name} with volume {source.volume} (distance {distance})");

            source.Play();

            playingAudioCoroutine.Add(playerID, Plugin.Instance.StartCoroutine(PlayClipCoroutine(player, source)));
        }

        static IEnumerator PlayClipCoroutine(IPlayer player, AudioSource source)
        {
            while (source.isPlaying)
                yield return null;

            playingAudioCoroutine.Remove(player.Id);
        }
    }
}
