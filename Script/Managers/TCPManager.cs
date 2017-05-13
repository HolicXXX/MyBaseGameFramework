using System;
using System.IO;
using System.Collections;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using UnityEngine;

public class MsgEvent{
	public string EventType;
	public Constant.MsgBase Msg;
}

public class TCPManager : Singleton<TCPManager> {

	int _port;
	IPAddress ipAddress;
	public Socket GameSocket{ get; private set; }

	Queue<byte[]> recBufQueue;

	void Awake() {
		recBufQueue = new Queue<byte[]> ();
	}

	// Use this for initialization
	void Start () {
	}
	
	// Update is called once per frame
	void Update () {
		while (recBufQueue.Count > 0) {
			var msg = ParseMsg (recBufQueue.Dequeue ());
			if(!msg.IsNull())
				Messenger<Constant.MsgBase>.Broadcast(msg.EventType,msg.Msg);
		}
	}

	public const int MAX_MSG_LENGTH = 2048;
	byte[] recBuffer = new byte[MAX_MSG_LENGTH];

	#region Client Code Block
	public void TCPConnect(string address,int port){
		if (!GameSocket.IsNull())
			return;
		GameSocket = new Socket (AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
		try{
			_port = port;
			ipAddress = IPAddress.Parse (address);
			IAsyncResult result = GameSocket.BeginConnect (new IPEndPoint (ipAddress, _port), new AsyncCallback (OnConnectSuccess), GameSocket);
			bool success = result.AsyncWaitHandle.WaitOne (5000, true);
			if (!success) {
				OnConnectOutofTime ();
			}
		}
		catch(System.Exception ex){
			OnConnectFailed (ex.Message);
		}
	}

	void OnConnectSuccess(IAsyncResult res){
		Debug.Log ("Connect Success");
		GameSocket.BeginReceive (recBuffer, 0, recBuffer.Length, SocketFlags.None, new System.AsyncCallback(OnReceiveMsg), GameSocket);
	}

	void OnConnectOutofTime(){
		Debug.Log("Connect out of time");
		//reconnect
		GameSocket = null;
	}

	void OnConnectFailed(string ex){
		Debug.Log ("Connect Failed: " + ex);
		GameSocket = null;
	}

	void OnReceiveMsg(IAsyncResult ar){
		var sock = ar.AsyncState as Socket;
		int readByte = 0;
		try{
			readByte = sock.EndReceive(ar);
		}
		catch(System.Exception ex){
			OnReceiveMsgError (ex.Message);
		}
		try{
			if (readByte > 0) {
//				var res = this.ParseMsg (recBuffer);
				lock (recBufQueue) {
//					string log = $"Receive MsgEvent: {res.EventType}\nMsgType: {res.Msg.type}\nMsgData: {res.Msg.data}";
//					Debug.Log (log);
					byte[] rec = new byte[recBuffer.Length];
					Buffer.BlockCopy(recBuffer,0,rec,0,rec.Length);
					recBufQueue.Enqueue (rec);
				}
			}
		}
		catch(System.Exception ex){
			OnReceiveMsgError (ex.Message);
		}

		GameSocket.BeginReceive (recBuffer, 0, recBuffer.Length, SocketFlags.None, new System.AsyncCallback(OnReceiveMsg), GameSocket);
	}

	void OnReceiveMsgError(string ex){
		Debug.Log ("Receive Msg error: " + ex);
	}

	MsgEvent ParseMsg(byte[] buf){
		MsgEvent ret = null;
		try{
			using(MemoryStream ms = new MemoryStream(buf))
			{
				byte[] eventlen = new byte[4];//int32 event length
				ms.Read (eventlen, 0, eventlen.Length);
				byte[] eventType = new byte[BitConverter.ToInt32 (eventlen, 0)];//string event
				ms.Read (eventType, 0, eventType.Length);
				byte[] msgtype = new byte[4];//int32 msg type
				ms.Read (msgtype, 0, msgtype.Length);
				byte[] data = new byte[ms.Length - ms.Position];//string msg data
				ms.Read (data, 0, data.Length);
				string sdata = StringUtil.GetBase64DecodeString(data);
				ret = new MsgEvent (){ 
					EventType = StringUtil.GetBase64DecodeString(eventType), 
					Msg = { type = BitConverter.ToInt32 (msgtype, 0), data = sdata } 
				};
			}
		}
		catch(System.Exception ex){
			Debug.Log ("Parse Msg error: " + ex.Message);
		}
		return ret;
	}

	public void SendMsg(Constant.MsgBase msg,string eventStr = ""){
		Debug.Log (GameSocket.Connected);
		if (GameSocket.IsNull() || !GameSocket.Connected)
			return;
		try{
			byte[] buf;
			using(MemoryStream ms = new MemoryStream())
			{
				var eventBytes = StringUtil.GetBase64EncodeBytes (eventStr);
				var len = eventBytes.Length;
				var lenBytes = BitConverter.GetBytes (len);
				ms.Write (lenBytes, 0, lenBytes.Length);
				ms.Write (eventBytes, 0, eventBytes.Length);
				var msgtype = msg.type; 
				var msgTpBytes = BitConverter.GetBytes (msgtype);
				ms.Write (msgTpBytes, 0, msgTpBytes.Length);
				var msgdata = StringUtil.GetBase64EncodeBytes (msg.data);
				ms.Write (msgdata, 0, msgdata.Length);
				buf = ms.GetBuffer ();
			}
			GameSocket.BeginSend (buf, 0, buf.Length, SocketFlags.None, new AsyncCallback (OnSendMsgCallback), GameSocket);
		}
		catch(System.Exception ex){
			Debug.Log ("Send Msg error: " + ex.Message);
		}
	}

	void OnSendMsgCallback(IAsyncResult ar){
		Socket worker = ar.AsyncState as Socket;
		worker.EndSend (ar);
		Debug.Log ("Send Msg Done");
	}

	public void CloseConnect(){
		if (!GameSocket.IsNull() && GameSocket.Connected) {
			GameSocket.Close ();
		}
		GameSocket = null;
	}

	#endregion

	#region Server Code Block

	Queue<Socket> _serverSocketQueue;

	public void TCPBind(string address,int port){
		if (!GameSocket.IsNull())
			return;
		if (_serverSocketQueue.IsNull ()) {
			_serverSocketQueue = new Queue<Socket> ();
		}
		GameSocket = new Socket (AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
		try{
			_port = port;
			ipAddress = IPAddress.Parse (address);
			GameSocket.Bind(new IPEndPoint(IPAddress.Any, port));
			GameSocket.Listen(10);
			GameSocket.BeginAccept(new AsyncCallback(OnAccept),GameSocket);
			Debug.Log ("Server Done");
		}
		catch(System.Exception ex){
			Debug.Log ("Bind or Accept error: " + ex.Message);
			GameSocket = null;
		}
	}

	void OnAccept(IAsyncResult ar){
		Debug.Log ("Server Accept");
		Socket socket = ar.AsyncState as Socket;

		Socket worker = socket.EndAccept(ar);
		worker.BeginReceive (recBuffer, 0, recBuffer.Length, SocketFlags.None, new System.AsyncCallback (OnReceiveMsg), worker);

		_serverSocketQueue.Enqueue (worker);

		socket.BeginAccept(new System.AsyncCallback(OnAccept), socket);
	}

	public void BroadMsg(Constant.MsgBase msg,string eventStr = ""){
		if (GameSocket.IsNull())
			return;
		try{
			byte[] buf;
			using(MemoryStream ms = new MemoryStream())
			{
				var eventBytes = StringUtil.GetBase64EncodeBytes (eventStr);
				var len = eventBytes.Length;
				var lenBytes = BitConverter.GetBytes (len);
				ms.Write (lenBytes, 0, lenBytes.Length);
				ms.Write (eventBytes, 0, eventBytes.Length);
				var msgtype = msg.type; 
				var msgTpBytes = BitConverter.GetBytes (msgtype);
				ms.Write (msgTpBytes, 0, msgTpBytes.Length);
				var msgdata = StringUtil.GetBase64EncodeBytes (msg.data);
				ms.Write (msgdata, 0, msgdata.Length);
				buf = ms.GetBuffer ();
			}
			foreach(var s in _serverSocketQueue){
				s.BeginSend (buf, 0, buf.Length, SocketFlags.Broadcast, new AsyncCallback (OnSendMsgCallback), s);
			}
		}
		catch(System.Exception ex){
			Debug.Log ("Send Msg error: " + ex.Message);
		}
	}

	#endregion

	protected override void OnDestroy(){
		CloseConnect ();
		base.OnDestroy ();
	}
}
