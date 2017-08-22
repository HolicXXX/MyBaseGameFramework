using System;

public class NetWorkEvent
{
	public class ConnectedEventArgs : IEventBase
	{
		public override int ID{ get{ return (int)EventID.Network_Connected;} }
		public ConnectedEventArgs(SocketChannel channel, object userData){
			Channel = channel;
			UserData = userData;
		}

		public SocketChannel Channel{ get; set; }
		public object UserData{ get; set; }
	}

	public class ClosedEventArgs : IEventBase
	{
		public override int ID{ get{ return (int)EventID.Network_Closed;} }
		public ClosedEventArgs(SocketChannel channel){
			Channel = channel;
		}

		public SocketChannel Channel{ get; set; }
	}

	public class SendEventArgs : IEventBase
	{
		public override int ID{ get{ return (int)EventID.Network_SendPacket;} }
		public SendEventArgs(SocketChannel channel,int sendBytesCount,object userData){
			Channel = channel;
			SendBytesCount = sendBytesCount;
			UserData = userData;
		}

		public SocketChannel Channel{ get; set; }
		public int SendBytesCount { get; set; }
		public object UserData { get; set; }
	}

	public class ErrorEventArgs : IEventBase
	{
		public override int ID{ get{ return (int)EventID.Network_Error;} }
		public ErrorEventArgs(SocketChannel channel,NetworkErrorCode code,string msg){
			Channel = channel;
			ErrorCode = code;
			Message = msg;
		}

		public SocketChannel Channel{ get; set; }
		public NetworkErrorCode ErrorCode { get; set; }
		public string Message { get; set; }
	}

	public class CustomErrorEventArgs : IEventBase
	{
		public override int ID{ get{ return (int)EventID.Network_CustomError;} }
		public CustomErrorEventArgs(SocketChannel channel,object userData){
			Channel = channel;
			UserData = userData;
		}

		public SocketChannel Channel{ get; set; }
		public object UserData{ get; set; }
	}
}

