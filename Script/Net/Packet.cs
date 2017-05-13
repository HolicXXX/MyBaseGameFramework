using System;

[Serializable]
public abstract class Packet : IEventBase {
	public override int ID{ get; }
}
