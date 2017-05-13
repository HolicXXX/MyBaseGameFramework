using System;
using System.IO;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;

public class NetWorkHelper {

	public void Serialize<T>(SocketChannel channel,Stream destination,T packet) where T : Packet{
		BinaryFormatter bf = new BinaryFormatter ();
		bf.Serialize (destination, packet);
	}

	public Packet Deserialize(SocketChannel channel,Stream source){
		Packet packet;
		BinaryFormatter bf = new BinaryFormatter ();
		packet = bf.Deserialize (source) as Packet;
		return packet;
	}
}
