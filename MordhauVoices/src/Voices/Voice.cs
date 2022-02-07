using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace MordhauVoices.Voices
{
    public class Voice
    {
        public VoiceTypes Type { get; }

        readonly Dictionary<VoiceLineTypes, List<AudioClip>> voiceLines = new();
        readonly Dictionary<VoiceLineTypes, int> previousRandomIndex = new();

        public Voice(VoiceTypes name)
        {
            this.Type = name;
        }

        public int GetRandomClipIndex(VoiceLineTypes type)
        {
            if (type == VoiceLineTypes.NONE)
                return -1;

            if (!previousRandomIndex.TryGetValue(type, out int previous))
                previous = -1;

            var list = voiceLines[type];
            var eligable = new List<int>();
            for (int i = 0; i < list.Count; i++)
            {
                if (i == previous)
                    continue;
                eligable.Add(i);
            }

            var next = eligable[UnityEngine.Random.Range(0, eligable.Count)];
            previousRandomIndex[type] = next;
            return next;
        }

        public AudioClip GetClip(VoiceLineTypes type, int index)
        {
            if (type == VoiceLineTypes.NONE)
                return null;

            return voiceLines[type][index];
        }

        internal void AddClip(AudioClip clip, string fileName)
        {
            VoiceLineTypes type = VoiceLineTypes.NONE;

            if (fileName.Contains("battle") || fileName.Contains("charge") || fileName.Contains("forward"))
                type = VoiceLineTypes.Battlecry;
            else if (fileName.Contains("follow"))
                type = VoiceLineTypes.Follow;
            else if (fileName.Contains("friend"))
                type = VoiceLineTypes.Friendly;
            else if (fileName.Contains("hello"))
                type = VoiceLineTypes.Hello;
            else if (fileName.Contains("help"))
                type = VoiceLineTypes.Help;
            else if (fileName.Contains("hold"))
                type = VoiceLineTypes.Hold;
            else if (fileName.Contains("insult"))
                type = VoiceLineTypes.Insult;
            else if (fileName.Contains("intimidate"))
                type = VoiceLineTypes.Intimidate;
            else if (fileName.Contains("laugh"))
                type = VoiceLineTypes.Laugh;
            else if (fileName.Contains("respect"))
                type = VoiceLineTypes.Respect;
            else if (fileName.Contains("retreat"))
                type = VoiceLineTypes.Retreat;
            else if (fileName.Contains("sorry"))
                type = VoiceLineTypes.Sorry;
            else if (fileName.Contains("thanks"))
                type = VoiceLineTypes.Thanks;
            else if (fileName.Contains("yes"))
                type = VoiceLineTypes.Yes;
            else if (fileName.Contains("no"))
                type = VoiceLineTypes.No;

            if (type == VoiceLineTypes.NONE)
                return;

            ProcessClip(clip);

            if (!voiceLines.TryGetValue(type, out List<AudioClip> list))
                voiceLines.Add(type, list = new());

            list.Add(clip);
        }

        const float MAX_AMP = 0.72f;

        void ProcessClip(AudioClip clip)
        {
            float[] clipSampleData = new float[clip.samples * clip.channels];
            clip.GetData(clipSampleData, 0);

            float highest = 0f;
            for (int i = 0; i < clipSampleData.Length; i++)
            {
                float sample = clipSampleData[i];
                if (sample > highest)
                    highest = sample;
            }

            if (highest >= MAX_AMP)
                return;

            float ampRatio = MAX_AMP / highest;

            for (int i = 0; i < clipSampleData.Length; i++)
                clipSampleData[i] = clipSampleData[i] * ampRatio;

            clip.SetData(clipSampleData, 0);

            // // debug
            // float oldHighest = highest;
            // for (int i = 0; i < clipSampleData.Length; i++)
            // {
            //     float sample = clipSampleData[i];
            //     if (sample > highest)
            //         highest = sample;
            // }
            // Plugin.Log.LogWarning($"Processed {this.Type}-{clip.name}, oldHighest: {oldHighest}, new: {highest}");
        }
    }
}
