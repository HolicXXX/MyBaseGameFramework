using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NetWorkManager : Singleton<NetWorkManager> {

	Dictionary<string,SocketChannel> _channelDicts;
	EventPool<Packet> _packetPool;

	public NetWorkHelper NetHelper{ get; set; }

	Action<SocketChannel,object> _ChannelConnected;
	Action<SocketChannel> _ChannelClosed;
	Action<SocketChannel,int,object> _ChannelSended;
	Action<SocketChannel,Packet> _ChannelReceived;
	Action<SocketChannel,NetworkErrorCode,string> _ChannelError;
	Action<SocketChannel,object> _ChannelCustomError;

	public event Action<SocketChannel,object> ChannelConnectedHandle {add{ _ChannelConnected += value;}remove{ _ChannelConnected -= value;}}
	public event Action<SocketChannel> ChannelClosedHandle {add{ _ChannelClosed += value;}remove{ _ChannelClosed -= value;}}
	public event Action<SocketChannel,int,object> ChannelSendedHandle {add{ _ChannelSended += value;}remove{ _ChannelSended -= value;}}
	public event Action<SocketChannel,Packet> ChannelReceivedHandle {add{ _ChannelReceived += value;}remove{ _ChannelReceived -= value;}}
	public event Action<SocketChannel,NetworkErrorCode,string> ChannelErrorHandle {add{ _ChannelError += value;}remove{ _ChannelError -= value;}}
	public event Action<SocketChannel,object> ChannelCustomErrorHandle {add{ _ChannelCustomError += value;}remove{ _ChannelCustomError -= value;}}

	public int ChannelCount{ get { return _channelDicts.Count; } }

	void Awake(){
		_channelDicts = new Dictionary<string, SocketChannel> ();
		_packetPool = new EventPool<Packet> ();

		NetHelper = new NetWorkHelper ();

		_ChannelConnected = null;
		_ChannelClosed = null;
		_ChannelSended = null;
		_ChannelReceived = null;
		_ChannelError = null;
		_ChannelCustomError = null;
	}

	void Start () {
		
	}

	void Update () {
	}

	private void OnNetworkChannelConnected(SocketChannel networkChannel, object userData)
	{
		if (_ChannelConnected != null)
		{
			lock (_ChannelConnected)
			{
				_ChannelConnected(networkChannel, userData);
			}
		}
	}

	private void OnNetworkChannelClosed(SocketChannel networkChannel)
	{
		if (_ChannelClosed != null)
		{
			lock (_ChannelClosed)
			{
				_ChannelClosed(networkChannel);
			}
		}
	}

	private void OnNetworkChannelSended(SocketChannel networkChannel, int bytesSent, object userData)
	{
		if (_ChannelSended != null)
		{
			lock (_ChannelSended)
			{
				_ChannelSended(networkChannel, bytesSent, userData);
			}
		}
	}

	private void OnNetworkChannelReceived(SocketChannel networkChannel, Packet packet)
	{
		_packetPool.TriggerEvent(networkChannel, packet);
	}

	private void OnNetworkChannelError(SocketChannel networkChannel, NetworkErrorCode errorCode, string errorMessage)
	{
		if (_ChannelError != null)
		{
			lock (_ChannelError)
			{
				_ChannelError(networkChannel, errorCode, errorMessage);
			}
		}
	}

	private void OnNetworkChannelCustomError(SocketChannel networkChannel, object customErrorData)
	{
		if (_ChannelCustomError != null)
		{
			lock (_ChannelCustomError)
			{
				_ChannelCustomError(networkChannel, customErrorData);
			}
		}
	}

	public void RegisterPacketHandle(int packetID, EventHandler<Packet> handle){
		_packetPool.RegisterEventHandle (packetID, handle);
	}

	public SocketChannel AddChannle(string name){
		if (HasChannel (name) || NetHelper.IsNull ())
			return null;

		SocketChannel sc = new SocketChannel (name, NetHelper);
		sc.OnChannelConnected += OnNetworkChannelConnected;
		sc.OnChannelClosed += OnNetworkChannelClosed;
		sc.OnChannelSended += OnNetworkChannelSended;
		sc.OnChannelReceived += OnNetworkChannelReceived;
		sc.OnChannelError += OnNetworkChannelError;
		sc.OnChannelCustomError += OnNetworkChannelCustomError;
		_channelDicts.Add (name, sc);
		return sc;
	}

	public bool HasChannel(string name){
		return _channelDicts.ContainsKey (name ?? string.Empty);
	}

	public SocketChannel GetChannel (string name){
		SocketChannel ret = null;
		_channelDicts.TryGetValue (name ?? string.Empty, out ret);
		return ret;
	}

	public SocketChannel[] GetAllChannels(){
		int i = 0;
		SocketChannel[] ret = new SocketChannel[_channelDicts.Count];
		foreach (SocketChannel sc in _channelDicts.Values) {
			ret [i++] = sc;
		}
		return ret;
	}

	void ChannelOut(SocketChannel sc){
		sc.Close ();
		sc.OnChannelConnected -= OnNetworkChannelConnected;
		sc.OnChannelClosed -= OnNetworkChannelClosed;
		sc.OnChannelSended -= OnNetworkChannelSended;
		sc.OnChannelReceived -= OnNetworkChannelReceived;
		sc.OnChannelError -= OnNetworkChannelError;
		sc.OnChannelCustomError -= OnNetworkChannelCustomError;
	}

	public void DestroyAllChannel(){
		foreach (SocketChannel sc in _channelDicts.Values) {
			ChannelOut (sc);
		}
	}

	public void DestroyChannle(string name){
		SocketChannel sc = null;
		if (_channelDicts.TryGetValue (name ?? string.Empty, out sc)) {
			ChannelOut (sc);
			_channelDicts.Remove (name);
		}
	}
}
