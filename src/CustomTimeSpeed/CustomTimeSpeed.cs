using Beam;
using BepInEx.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ModPack.CustomTimeSpeed
{
    public class CustomTimeSpeed : SubPlugin
    {
        public override string Name => "Custom Time Speed";

        private static ConfigEntry<int> TimeScale;
        private const int DEFAULT_TIMESCALE = 36;

        public override void Initialize()
        {
            TimeScale = Bind("Time Scale",
                DEFAULT_TIMESCALE, 
                "In-game seconds per real second. 36 = default, 1 = realtime (slower)", 
                TimeScale_SettingChanged);

            base.Initialize();
        }

        private void TimeScale_SettingChanged(SettingChangedEventArgs args)
        {
            if (this.Enabled)
                GameTime.TIME_SCALE = (int)args.ChangedSetting.BoxedValue;
        }

        public override void OnEnabled()
        {
            base.OnEnabled();
            GameTime.TIME_SCALE = TimeScale.Value;
        }

        public override void OnDisabled()
        {
            base.OnDisabled();
            GameTime.TIME_SCALE = DEFAULT_TIMESCALE;
        }
    }
}
