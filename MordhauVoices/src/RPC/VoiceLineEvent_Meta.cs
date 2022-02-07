using System;
using Bolt;

namespace MordhauVoices.RPC
{
    internal class VoiceLineEvent_Meta : Event_Meta, IEventFactory, IFactory
	{
		static VoiceLineEvent_Meta()
		{
			VoiceLineEvent_Meta.Instance.InitMeta();
		}

		TypeId IFactory.TypeId
		{
			get
			{
				return this.TypeId;
			}
		}

		public UniqueId TypeKey
		{
			get
			{
				return new UniqueId(197, 251, 10, 137, 11, 165, 59, 65, 175, 189, 164, 8, 249, 112, 229, 173);
			}
		}

		Type IFactory.TypeObject
		{
			get
			{
				return typeof(VoiceLineEvent);
			}
		}

		public override void InitObject(NetworkObj obj, NetworkObj_Meta.Offsets offsets)
		{
		}

		public override void InitMeta()
		{
			this.TypeId = new TypeId(995);
			this.CountStorage = 4;
			this.CountObjects = 1;
			this.CountProperties = 4;
			this.Properties = new NetworkPropertyInfo[4];
			NetworkProperty_String networkProperty_String = new NetworkProperty_String();
			networkProperty_String.PropertyMeta = this;
			networkProperty_String.Settings_Property("VoiceLine", 1, -1073741824);
			networkProperty_String.Settings_Offsets(0, 0);
			networkProperty_String.AddStringSettings(0);
			base.AddProperty(0, 0, networkProperty_String, -1);
			NetworkProperty_String networkProperty_String2 = new NetworkProperty_String();
			networkProperty_String2.PropertyMeta = this;
			networkProperty_String2.Settings_Property("VoiceType", 1, -1073741824);
			networkProperty_String2.Settings_Offsets(1, 1);
			networkProperty_String2.AddStringSettings(0);
			base.AddProperty(1, 0, networkProperty_String2, -1);
			NetworkProperty_Float networkProperty_Float = new NetworkProperty_Float();
			networkProperty_Float.PropertyMeta = this;
			networkProperty_Float.Settings_Property("Pitch", 1, -1073741824);
			networkProperty_Float.Settings_Offsets(2, 2);
			networkProperty_Float.Settings_Float(PropertyFloatCompressionSettings.Create());
			base.AddProperty(2, 0, networkProperty_Float, -1);
			NetworkProperty_Integer networkProperty_Integer = new NetworkProperty_Integer();
			networkProperty_Integer.PropertyMeta = this;
			networkProperty_Integer.Settings_Property("OwnerID", 1, -1073741824);
			networkProperty_Integer.Settings_Offsets(3, 3);
			networkProperty_Integer.Settings_Integer(PropertyIntCompressionSettings.Create());
			base.AddProperty(3, 0, networkProperty_Integer, -1);
			base.InitMeta();
			this._pool = new ObjectPool<VoiceLineEvent>();
		}

		object IFactory.Create()
		{
			return this._pool.Get();
		}

		void IFactory.Return(object objToReturn)
		{
			this._pool.Return(objToReturn as VoiceLineEvent);
		}

		void IEventFactory.Dispatch(Event ev, object target)
		{
			IVoiceLineEventListener voiceLineEventListener = target as IVoiceLineEventListener;
			if (voiceLineEventListener != null)
			{
				voiceLineEventListener.OnEvent((VoiceLineEvent)ev);
			}
		}

		internal static VoiceLineEvent_Meta Instance = new VoiceLineEvent_Meta();

		internal ObjectPool<VoiceLineEvent> _pool;
	}
}
