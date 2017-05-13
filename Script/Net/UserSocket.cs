
using System.Net.Sockets;

public partial class SocketChannel{
	public class ReceiveState {

		public byte[] Buffer{ get; }
		public int Length{ get; set;}
		public int ReceivedLength{ get; set;}
		public int BufferSize{ get { return Buffer.Length; } }

		public ReceiveState(int bufferSize){
			Buffer = new byte[bufferSize < 0 ? 0 : bufferSize];
		}

		public void ResetLength(int packetHeaderLength){
			Length = packetHeaderLength;
			ReceivedLength = 0;
		}
	}

	public class UserSocket {
		public Socket Socket{ get; private set; }
		public object UserData{ get; set; }
		public bool Connected{get{ return Socket.Connected; }}
		public UserSocket(Socket s,object data){
			this.Socket = s;
			this.UserData = data;
		}
		public void Close(){
			this.Socket.Close ();
			this.Socket = null;
			this.UserData = null;
		}
	}
}

