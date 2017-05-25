using ProtoBuf;
using ProtoBuf.Meta;
using System;
using System.IO;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;

public class NetWorkHelper {

	public void Serialize<T>(SocketChannel channel,Stream destination,T packet) where T : Packet{
//		BinaryFormatter bf = new BinaryFormatter ();
//		bf.Serialize (destination, packet);
//		PacketHead ph = new PacketHead (packet.ID);
//		Serializer.SerializeWithLengthPrefix (destination, ph, PrefixStyle.Fixed32);
		Serializer.Serialize (destination, packet);
	}

	public T Deserialize<T>(SocketChannel channel,Stream source){
//		Packet packet;
//		BinaryFormatter bf = new BinaryFormatter ();
//		packet = bf.Deserialize (source) as Packet;
//		return packet;
//		PacketHead packetHead = Serializer.DeserializeWithLengthPrefix<PacketHead>(source, PrefixStyle.Fixed32);
		return Serializer.Deserialize<T> (source);
	}
}
