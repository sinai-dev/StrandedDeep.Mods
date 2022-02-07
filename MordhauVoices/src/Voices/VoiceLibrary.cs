using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace MordhauVoices.Voices
{
    public static class VoiceLibrary
    {
        static readonly Dictionary<VoiceTypes, Voice> voices = new();

        public static int GetRandomIndex(VoiceTypes voiceType, VoiceLineTypes lineType)
        {
            return voices[voiceType].GetRandomClipIndex(lineType);
        }

        public static AudioClip GetVoiceLine(VoiceTypes voiceType, VoiceLineTypes line, int randomIndex)
        {
            return voices[voiceType].GetClip(line, randomIndex);
        }

        internal static void Init()
        {
            Plugin.Log.LogMessage($"Initializing voices...");

            Plugin.Instance.StartCoroutine(InitCoroutine());
        }

        static AssetBundleCreateRequest LoadBundleAsync()
        {
            var path = Path.Combine(BepInEx.Paths.PluginPath, "mordhauvoices.bundle");
            return AssetBundle.LoadFromFileAsync(path);
        }

        internal static IEnumerator InitCoroutine()
        {
            var bundleRequest = LoadBundleAsync();

            while (!bundleRequest.isDone)
                yield return null;

            var bundle = bundleRequest.assetBundle;

            foreach (var path in bundle.GetAllAssetNames())
            {
                var assetRequest = bundle.LoadAssetAsync<AudioClip>(path);

                while (!assetRequest.isDone)
                    yield return null;

                var asset = assetRequest.asset as AudioClip;
                var split = path.Split('/');
                AddVoiceLine(asset, split[2], split[3]);
            }

            Plugin.Log.LogMessage($"Finished loading voices.");
        }

        static void AddVoiceLine(AudioClip clip, string folderName, string fileName)
        {
            VoiceTypes type = FolderNameToVoiceType(folderName);

            if (!voices.ContainsKey(type))
                voices.Add(type, new(type));

            voices[type].AddClip(clip, fileName);
        }

        static VoiceTypes FolderNameToVoiceType(string folderName)
        {
            return folderName switch
            {
                "barbarian" => VoiceTypes.Barbarian,
                "commoner" => VoiceTypes.Commoner,
                "cruel" => VoiceTypes.Cruel,
                "eager" => VoiceTypes.Eager,
                "foppish" => VoiceTypes.Foppish,
                "knight" => VoiceTypes.Knight,
                "plain" => VoiceTypes.Plain,
                "scot" => VoiceTypes.Raider,
                "young" => VoiceTypes.Young,
                _ => throw new NotImplementedException($"{folderName}")
            };
        }
    }
}
