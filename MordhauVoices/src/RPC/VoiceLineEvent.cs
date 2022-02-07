using System;
using Bolt;

namespace MordhauVoices.RPC
{
	public class VoiceLineEvent : Event
	{
		public VoiceLineEvent() : base(VoiceLineEvent_Meta.Instance)
		{
		}

		public string VoiceLine
		{
			get
			{
				return this.Storage.Values[this.OffsetStorage].String;
			}
			set
			{
				string @string = this.Storage.Values[this.OffsetStorage].String;
				this.Storage.Values[this.OffsetStorage].String = value;
				if (NetworkValue.Diff(@string, value))
				{
				}
			}
		}

		public string VoiceType
		{
			get
			{
				return this.Storage.Values[this.OffsetStorage + 1].String;
			}
			set
			{
				string @string = this.Storage.Values[this.OffsetStorage + 1].String;
				this.Storage.Values[this.OffsetStorage + 1].String = value;
				if (NetworkValue.Diff(@string, value))
				{
				}
			}
		}

		public float Pitch
		{
			get
			{
				return this.Storage.Values[this.OffsetStorage + 2].Float0;
			}
			set
			{
				float @float = this.Storage.Values[this.OffsetStorage + 2].Float0;
				this.Storage.Values[this.OffsetStorage + 2].Float0 = value;
				if (NetworkValue.Diff(@float, value))
				{
				}
			}
		}

		public int OwnerID
		{
			get
			{
				return this.Storage.Values[this.OffsetStorage + 3].Int0;
			}
			set
			{
				int @int = this.Storage.Values[this.OffsetStorage + 3].Int0;
				this.Storage.Values[this.OffsetStorage + 3].Int0 = value;
				if (NetworkValue.Diff(@int, value))
				{
				}
			}
		}

		public override string ToString()
		{
			return string.Format("[VoiceLineEvent VoiceLine={0} VoiceType={1} Pitch={2} OwnerID={3}]", new object[]
			{
				this.VoiceLine,
				this.VoiceType,
				this.Pitch,
				this.OwnerID
			});
		}

		public static VoiceLineEvent Create(BoltEntity entity, EntityTargets targets)
		{
			if (!entity)
			{
				throw new ArgumentNullException("entity");
			}
			if (!entity.IsAttached)
			{
				throw new BoltException("You can not raise events on entities which are not attached", Array.Empty<object>());
			}
			VoiceLineEvent voiceLineEvent = Factory.NewEvent(VoiceLineEvent_Meta.Instance.TypeKey) as VoiceLineEvent;
			VoiceLineEvent result;
			if (voiceLineEvent == null)
			{
				result = null;
			}
			else
			{
				voiceLineEvent.Targets = (int)targets;
				voiceLineEvent.TargetEntity = entity.Entity;
				voiceLineEvent.Reliability = 0;
				result = voiceLineEvent;
			}
			return result;
		}

		public static VoiceLineEvent Create(BoltEntity entity)
		{
			return VoiceLineEvent.Create(entity, EntityTargets.Everyone);
		}

		private static VoiceLineEvent Create(byte targets, BoltConnection connection, ReliabilityModes reliability)
		{
			VoiceLineEvent voiceLineEvent = Factory.NewEvent(VoiceLineEvent_Meta.Instance.TypeKey) as VoiceLineEvent;
			VoiceLineEvent result;
			if (voiceLineEvent == null)
			{
				result = null;
			}
			else
			{
				voiceLineEvent.Targets = (int)targets;
				voiceLineEvent.TargetConnection = connection;
				voiceLineEvent.Reliability = reliability;
				result = voiceLineEvent;
			}
			return result;
		}

		public static VoiceLineEvent Create(GlobalTargets targets)
		{
			return VoiceLineEvent.Create((byte)targets, null, ReliabilityModes.ReliableOrdered);
		}

		public static VoiceLineEvent Create(GlobalTargets targets, ReliabilityModes reliability)
		{
			return VoiceLineEvent.Create((byte)targets, null, reliability);
		}

		public static VoiceLineEvent Create(BoltConnection connection)
		{
			return VoiceLineEvent.Create(10, connection, ReliabilityModes.ReliableOrdered);
		}

		public static VoiceLineEvent Create(BoltConnection connection, ReliabilityModes reliability)
		{
			return VoiceLineEvent.Create(10, connection, reliability);
		}

		public static VoiceLineEvent Create()
		{
			return VoiceLineEvent.Create(2, null, ReliabilityModes.ReliableOrdered);
		}

		public static VoiceLineEvent Create(ReliabilityModes reliability)
		{
			return VoiceLineEvent.Create(2, null, reliability);
		}

		public static bool Post(BoltEntity entity, EntityTargets targets, string VoiceLine, string VoiceType, float Pitch, int OwnerID)
		{
			VoiceLineEvent voiceLineEvent = VoiceLineEvent.Create(entity, targets);
			bool result;
			if (voiceLineEvent == null)
			{
				result = false;
			}
			else
			{
				voiceLineEvent.VoiceLine = VoiceLine;
				voiceLineEvent.VoiceType = VoiceType;
				voiceLineEvent.Pitch = Pitch;
				voiceLineEvent.OwnerID = OwnerID;
				voiceLineEvent.Send();
				result = true;
			}
			return result;
		}

		public static bool Post(BoltEntity entity, string VoiceLine, string VoiceType, float Pitch, int OwnerID)
		{
			return VoiceLineEvent.Post(entity, EntityTargets.Everyone, VoiceLine, VoiceType, Pitch, OwnerID);
		}

		private static bool Post(byte targets, BoltConnection connection, ReliabilityModes reliability, string VoiceLine, string VoiceType, float Pitch, int OwnerID)
		{
			VoiceLineEvent voiceLineEvent = VoiceLineEvent.Create(targets, connection, reliability);
			bool result;
			if (voiceLineEvent == null)
			{
				result = false;
			}
			else
			{
				voiceLineEvent.VoiceLine = VoiceLine;
				voiceLineEvent.VoiceType = VoiceType;
				voiceLineEvent.Pitch = Pitch;
				voiceLineEvent.OwnerID = OwnerID;
				voiceLineEvent.Send();
				result = true;
			}
			return result;
		}

		public static bool Post(GlobalTargets targets, string VoiceLine, string VoiceType, float Pitch, int OwnerID)
		{
			return VoiceLineEvent.Post((byte)targets, null, ReliabilityModes.ReliableOrdered, VoiceLine, VoiceType, Pitch, OwnerID);
		}

		public static bool Post(GlobalTargets targets, ReliabilityModes reliability, string VoiceLine, string VoiceType, float Pitch, int OwnerID)
		{
			return VoiceLineEvent.Post((byte)targets, null, reliability, VoiceLine, VoiceType, Pitch, OwnerID);
		}

		public static bool Post(BoltConnection connection, string VoiceLine, string VoiceType, float Pitch, int OwnerID)
		{
			return VoiceLineEvent.Post(10, connection, ReliabilityModes.ReliableOrdered, VoiceLine, VoiceType, Pitch, OwnerID);
		}

		public static bool Post(BoltConnection connection, ReliabilityModes reliability, string VoiceLine, string VoiceType, float Pitch, int OwnerID)
		{
			return VoiceLineEvent.Post(10, connection, reliability, VoiceLine, VoiceType, Pitch, OwnerID);
		}

		public static bool Post(string VoiceLine, string VoiceType, float Pitch, int OwnerID)
		{
			return VoiceLineEvent.Post(2, null, ReliabilityModes.ReliableOrdered, VoiceLine, VoiceType, Pitch, OwnerID);
		}

		public static bool Post(ReliabilityModes reliability, string VoiceLine, string VoiceType, float Pitch, int OwnerID)
		{
			return VoiceLineEvent.Post(2, null, reliability, VoiceLine, VoiceType, Pitch, OwnerID);
		}
	}
}
