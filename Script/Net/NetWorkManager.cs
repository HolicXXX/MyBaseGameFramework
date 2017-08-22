using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NetWorkManager : Singleton<NetWorkManager> {

	Dictionary<string,SocketChannel> _channelDicts;
	EventPool<Packet> _packetPool;

	public NetWorkHelper NetHelper{ get; set; }

	Action<NetWorkEvent.ConnectedEventArgs> _ChannelConnected;
	Action<NetWorkEvent.ClosedEventArgs> _ChannelClosed;
	Action<NetWorkEvent.SendEventArgs> _ChannelSended;
	Action<SocketChannel,Packet> _ChannelReceived;
	Action<NetWorkEvent.ErrorEventArgs> _ChannelError;
	Action<NetWorkEvent.CustomErrorEventArgs> _ChannelCustomError;

	public event Action<NetWorkEvent.ConnectedEventArgs> ChannelConnectedHandle {add{ _ChannelConnected += value;}remove{ _ChannelConnected -= value;}}
	public event Action<NetWorkEvent.ClosedEventArgs> ChannelClosedHandle {add{ _ChannelClosed += value;}remove{ _ChannelClosed -= value;}}
	public event Action<NetWorkEvent.SendEventArgs> ChannelSendedHandle {add{ _ChannelSended += value;}remove{ _ChannelSended -= value;}}
	public event Action<SocketChannel,Packet> ChannelReceivedHandle {add{ _ChannelReceived += value;}remove{ _ChannelReceived -= value;}}
	public event Action<NetWorkEvent.ErrorEventArgs> ChannelErrorHandle {add{ _ChannelError += value;}remove{ _ChannelError -= value;}}
	public event Action<NetWorkEvent.CustomErrorEventArgs> ChannelCustomErrorHandle {add{ _ChannelCustomError += value;}remove{ _ChannelCustomError -= value;}}

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
		_packetPool.Update ();
	}

	private void OnNetworkChannelConnected(NetWorkEvent.ConnectedEventArgs args)
	{
		if (_ChannelConnected != null)
		{
			lock (_ChannelConnected)
			{
				_ChannelConnected(args);
			}
		}
		EventPoolManager.Instance.TriggerEvent (this, args);
	}

	private void OnNetworkChannelClosed(NetWorkEvent.ClosedEventArgs args)
	{
		if (_ChannelClosed != null)
		{
			lock (_ChannelClosed)
			{
				_ChannelClosed(args);
			}
		}
		EventPoolManager.Instance.TriggerEvent (this, args);
	}

	private void OnNetworkChannelSended(NetWorkEvent.SendEventArgs args)
	{
		if (_ChannelSended != null)
		{
			lock (_ChannelSended)
			{
				_ChannelSended(args);
			}
		}
		EventPoolManager.Instance.TriggerEvent (this, args);
	}

	private void OnNetworkChannelReceived(SocketChannel networkChannel, Packet packet)
	{
		_packetPool.TriggerEvent(networkChannel, packet);
	}

	private void OnNetworkChannelError(NetWorkEvent.ErrorEventArgs args)
	{
		if (_ChannelError != null)
		{
			lock (_ChannelError)
			{
				_ChannelError(args);
			}
		}
		EventPoolManager.Instance.TriggerEvent (this, args);
	}

	private void OnNetworkChannelCustomError(NetWorkEvent.CustomErrorEventArgs args)
	{
		if (_ChannelCustomError != null)
		{
			lock (_ChannelCustomError)
			{
				_ChannelCustomError(args);
			}
		}
		EventPoolManager.Instance.TriggerEvent (this, args);
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
