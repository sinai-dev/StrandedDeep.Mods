using Beam;
using Bolt;
using BoltInternal;
using MordhauVoices.Voices;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace MordhauVoices.RPC
{
    public class RPCManager : GlobalEventListener, IVoiceLineEventListener
    {
        public static void SendVoiceLine(VoiceLineTypes voiceLineType)
        {
            try
            {
                var evt = VoiceLineEvent.Create();

                var voiceType = Plugin.VoiceType.Value;

                evt.VoiceLine = string.Join("|", voiceLineType.ToString(), VoiceLibrary.GetRandomIndex(voiceType, voiceLineType));
                evt.VoiceType = voiceType.ToString();
                evt.Pitch = Plugin.Pitch.Value;
                evt.OwnerID = PlayerRegistry.LocalPlayer.Id;

                evt.Send();
            }
            catch (Exception ex)
            {
                Plugin.Log.LogWarning($"Error sending RPC: {ex}");
            }
        }

        public void OnEvent(VoiceLineEvent evt)
        {
            //Plugin.Log.LogWarning($"Received: Player {evt.OwnerID} - {evt.VoiceLine}, {evt.VoiceType}, pitch {evt.Pitch}");

            Enum.TryParse(evt.VoiceType, out VoiceTypes voiceType);

            string[] lineSplit = evt.VoiceLine.Split('|');
            Enum.TryParse(lineSplit[0], out VoiceLineTypes voiceLine);
            int randomIndex = int.Parse(lineSplit[1]);

            var clip = VoiceLibrary.GetVoiceLine(voiceType, voiceLine, randomIndex);
            VoicePlayer.PlayClip(evt.OwnerID, clip, evt.Pitch);
        }
    }
}
