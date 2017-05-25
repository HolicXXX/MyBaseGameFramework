using System;
using System.IO;
using System.Net;
using System.Net.Sockets;

public partial class SocketChannel : IDisposable {

	public SocketChannel(string name,NetWorkHelper helper){
		Name = name ?? string.Empty;
		this.Type = NetWorkType.NWT_UNKNOWN;
		_socket = null;
		_socketReceive = null;
		_isActive = false;
		_isDesposed = false;
		_networkHelper = helper;

		PacketHeaderLength = DefaultPacketHeaderLength;
		MaxPacketLength = DefaultMaxPacketLength;

		OnChannelConnected = null;
		OnChannelClosed = null;
		OnChannelSended = null;
		OnChannelReceived = null;
		OnChannelError = null;
		OnChannelCustomError = null;
	}

	const int DefaultPacketHeaderLength = 4;
	const int DefaultMaxPacketLength = 1024 * 32;
//	const float DefaultHeartBeatInterval = 30f;

	UserSocket _socket;
	ReceiveState _socketReceive;
	bool _isActive;
	bool _isDesposed;
	NetWorkHelper _networkHelper{ get; }

	public string Name{ get;}
	public bool Connected{ get { return _socket.Connected; } }
	public NetWorkType Type{ get; private set; }
	public int PacketHeaderLength{ get; private set; }
	public int MaxPacketLength{ get; private set; }
//	public float HeartBeatInterval{ get; set; }

	public Action<SocketChannel,object> OnChannelConnected;
	public Action<SocketChannel> OnChannelClosed;
	public Action<SocketChannel,int,object> OnChannelSended;
	public Action<SocketChannel,Packet> OnChannelReceived;
	public Action<SocketChannel,NetworkErrorCode,string> OnChannelError;
	public Action<SocketChannel,object> OnChannelCustomError;

	public IPAddress LocalIPAddress
	{
		get{ 
			if (_socket.IsNull () || _socket.Socket.IsNull ())
				return null;
			
			var ep = (IPEndPoint)_socket.Socket.LocalEndPoint;
			return ep.Address;
		}
	}

	public int LocalPort
	{
		get{ 
			if (_socket.IsNull () || _socket.Socket.IsNull ())
				return -1;
			
			var ep = (IPEndPoint)_socket.Socket.LocalEndPoint;
			return ep.Port;
		}
	}

	public IPAddress RemoteIPAddress
	{
		get{ 
			if (_socket.IsNull () || _socket.Socket.IsNull ())
				return null;

			var ep = (IPEndPoint)_socket.Socket.RemoteEndPoint;
			return ep.Address;
		}
	}

	public int RemotePort
	{
		get{ 
			if (_socket.IsNull () || _socket.Socket.IsNull ())
				return -1;

			var ep = (IPEndPoint)_socket.Socket.RemoteEndPoint;
			return ep.Port;
		}
	}

	public int ReceiveBufferSize
	{
		get{ 
			if (_socket.IsNull () || _socket.Socket.IsNull ())
				return 0;
			return _socket.Socket.ReceiveBufferSize;
		}
		set{ 
			if (!_socket.IsNull () && !_socket.Socket.IsNull ()) {
				_socket.Socket.ReceiveBufferSize = value;
			}
		}
	}

	public int SendBufferSize
	{
		get{ 
			if (_socket.IsNull () || _socket.Socket.IsNull ())
				return 0;
			return _socket.Socket.SendBufferSize;
		}
		set{ 
			if (!_socket.IsNull () && !_socket.Socket.IsNull ()) {
				_socket.Socket.SendBufferSize = value;
			}
		}
	}

	public void Connect(IPAddress address,int port,int packetHeaderLength, int maxPacketLength, object userData){
		InitSocket (address.AddressFamily, packetHeaderLength, maxPacketLength, userData);

		if (_socket == null || _socket.Socket == null) {
			return;
		}
		try{
			_socket.Socket.BeginConnect (address, port, ConnectCallback, _socket);
		}
		catch(Exception ex){
			if (!OnChannelError.IsNull ()) {
				OnChannelError (this, NetworkErrorCode.NRC_CONNECTERROR, ex.Message);
			}
		}
	}

	void ConnectCallback(IAsyncResult ar){
		UserSocket usocket = ar.AsyncState as UserSocket;
		try{
			usocket.Socket.EndConnect(ar);
		}
		catch(Exception ex){
			_isActive = false;
			if (!OnChannelError.IsNull ()) {
				OnChannelError (this, NetworkErrorCode.NRC_CONNECTERROR, ex.Message);
				return;
			}
		}

		_isActive = true;
		if (!OnChannelConnected.IsNull ()) {
			OnChannelConnected (this, usocket.UserData);
		}

		Receive ();
	}

	void InitSocket (AddressFamily addressFamily, int packetHeaderLength, int maxPacketLength,object userData){
		if (!_socket.IsNull () && !_socket.Socket.IsNull ()) {
			_socket.Close ();
		}

		this.PacketHeaderLength = packetHeaderLength;
		this.MaxPacketLength = maxPacketLength;

		switch (addressFamily) {
		case AddressFamily.InterNetwork:
			this.Type = NetWorkType.NWT_IPV4;
			break;
		case AddressFamily.InterNetworkV6:
			this.Type = NetWorkType.NWT_IPV6;
			break;
		default:
			this.Type = NetWorkType.NWT_UNKNOWN;
			break;
		}

		Socket sock = new Socket (addressFamily, SocketType.Stream, ProtocolType.Tcp);
		_socket = new UserSocket (sock, userData);
		_socketReceive = new ReceiveState (maxPacketLength);
		_socketReceive.ResetLength (packetHeaderLength);
	}

	public void Close(){
		if (_socket.IsNull () || _socket.Socket.IsNull ())
			return;
		_isActive = false;
		try{
			_socket.Socket.Shutdown(SocketShutdown.Both);
		}
		catch{
		}
		finally{
			_socket.Close ();
			_socket = null;
			_socketReceive = null;
			if (!OnChannelClosed.IsNull ()) {
				OnChannelClosed (this);
			}
		}
	}

	public void Receive(){
		try{
			_socket.Socket.BeginReceive(_socketReceive.Buffer,
				_socketReceive.ReceivedLength,
				_socketReceive.Length - _socketReceive.ReceivedLength,
				SocketFlags.None,
				ReceivedCallback,
				_socket
			);
		}
		catch(Exception ex){
			_isActive = false;
			if (!OnChannelError.IsNull ()) {
				OnChannelError (this, NetworkErrorCode.NRC_RECEIVEERROR, ex.Message);
			}
		}
	}

	void ReceivedCallback(IAsyncResult ar){
		UserSocket usocket = ar.AsyncState as UserSocket;
		int receivedBytes = 0;
		try{
			receivedBytes = usocket.Socket.EndReceive(ar);
		}
		catch(Exception ex){
			_isActive = false;
			if (!OnChannelError.IsNull ()) {
				OnChannelError (this, NetworkErrorCode.NRC_RECEIVEERROR, ex.Message);
				return;
			}
		}

		if (receivedBytes <= 0) {
			Close ();
			return;
		}

		_socketReceive.ReceivedLength += receivedBytes;
		if (_socketReceive.ReceivedLength < _socketReceive.Length) {
			Receive ();
			return;
		}

		bool isSuccess = false;
		try{
			isSuccess = ReceiveProcess();
		}
		catch(Exception ex){
			_isActive = false;
			if (!OnChannelError.IsNull ()) {
				OnChannelError (this, NetworkErrorCode.NRC_STEAMERROR, ex.Message);
				return;
			}
		}

		if (isSuccess)
			Receive ();
	}

	bool ReceiveProcess(){

		if (_socketReceive.Length < PacketHeaderLength) {
			throw new Exception ("Length is smaller than length header");
		}

		if (_socketReceive.Length == PacketHeaderLength) {
			int packetLength = Utility.Converter.GetIntFromBytes (_socketReceive.Buffer);
			if (packetLength <= 0) {
				string errorMessage = "Packet length is invalid.";
				if (OnChannelError != null)
				{
					OnChannelError(this, NetworkErrorCode.NRC_HEADERERROR, errorMessage);
					return false;
				}

				throw new Exception(errorMessage);
			}

			_socketReceive.Length += packetLength;
			if (_socketReceive.Length > _socketReceive.BufferSize) {
				string errorMessage = "Packet length is larger than buffer size.";
				if (OnChannelError != null)
				{
					OnChannelError(this, NetworkErrorCode.NRC_HEADERERROR, errorMessage);
					return false;
				}

				throw new Exception(errorMessage);
			}

			return true;
		}

		Packet packet = null;
		try{
			int packetLength = _socketReceive.Length - PacketHeaderLength;
			using(MemoryStream ms = new MemoryStream(_socketReceive.Buffer,PacketHeaderLength,packetLength,false))
			{
				lock(_networkHelper)
				{
					packet = _networkHelper.Deserialize<Packet>(this,ms);
				}
			}

			_socketReceive.ResetLength(PacketHeaderLength);
			if(packet.IsNull()){
				if(!OnChannelError.IsNull()){
					OnChannelError(this,NetworkErrorCode.NRC_RECEIVEERROR,"null packet");
				}
			}else{
				if(!OnChannelReceived.IsNull()){
					OnChannelReceived(this,packet);
				}
			}
		}
		catch(Exception ex){
			_isActive = false;
			if (!OnChannelError.IsNull ()) {
				OnChannelError (this, NetworkErrorCode.NRC_DESERIALIZEERROR, ex.Message);
				return false;
			}
			throw;
		}

		return true;
	}

	public void Send(byte[] buffer,int offset,int size,object userData){
		try{
			_socket.UserData = userData;
			_socket.Socket.BeginSend(buffer,offset,size,SocketFlags.None,SentCallback,_socket);
		}
		catch(Exception ex){
			_isActive = false;
			if (!OnChannelError.IsNull ()) {
				OnChannelError (this, NetworkErrorCode.NRC_SENDERROR, ex.Message);
				return;
			}
		}
	}

	public void Send<T>(T packet,object userData = null) where T : Packet{
		try{
			int length, packetLength = 0;
			byte[] buffer = new byte[MaxPacketLength];
			using(MemoryStream ms = new MemoryStream(buffer,true))
			{
				ms.Seek(PacketHeaderLength,SeekOrigin.Begin);
				_networkHelper.Serialize(this,ms,packet);
				length = (int)ms.Position;
			}
			packetLength = length - PacketHeaderLength;
			Utility.Converter.GetBytesFromInt(packetLength).CopyTo(buffer,0);
			Send(buffer,0,length,userData);
		}
		catch(Exception ex){
			_isActive = false;
			if (!OnChannelError.IsNull ()) {
				OnChannelError (this, NetworkErrorCode.NRC_SERIALIZEERROR, ex.Message);
			}
			return;
		}
	}

	void SentCallback(IAsyncResult ar){
		UserSocket usocket = ar.AsyncState as UserSocket;
		int bytesSent = 0;
		try{
			bytesSent = usocket.Socket.EndSend(ar);
		}
		catch(Exception ex){
			_isActive = false;
			if (!OnChannelError.IsNull ()) {
				OnChannelError (this, NetworkErrorCode.NRC_SENDERROR, ex.Message);
				return;
			}
		}

		if (!OnChannelSended.IsNull ()) {
			OnChannelSended (this, bytesSent, usocket.UserData);
		}
	}

	public void Dispose(){
		if (!_isDesposed) {
			Close ();
			_isDesposed = true;
		}
	}

}
