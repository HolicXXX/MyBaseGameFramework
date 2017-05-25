using ProtoBuf;
using System;

//public class PacketHead{
//	public int ID{ get; private set; }
//	public PacketHead(int id){
//		ID = id;
//	}
//}

[Serializable]
[ProtoContract]
public abstract class Packet : IEventBase {
	[ProtoMember(1)]
	public override int ID{ get; }
	[ProtoMember(2)]
	public abstract string Message{ get; }
}
