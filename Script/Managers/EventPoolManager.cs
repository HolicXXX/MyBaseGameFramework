using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EventPoolManager : Singleton<EventPoolManager> {

	/// <summary>
	/// The event pool.
	/// </summary>
	EventPool<IEventBase> _eventPool;

	void Awake(){
		_eventPool = new EventPool<IEventBase> ();
	}

	void Start () {
	}

	void Update () {
		_eventPool.Update ();
	}

	/// <summary>
	/// Check the handle.
	/// </summary>
	/// <returns><c>true</c>, if handle was registed, <c>false</c> otherwise.</returns>
	/// <param name="id">Event Idr.</param>
	/// <param name="handle">Handle.</param>
	public bool CheckHandle(int id,EventHandler<IEventBase> handle){
		return _eventPool.CheckHandler (id, handle);
	}

	/// <summary>
	/// Register the event handle.
	/// </summary>
	/// <param name="id">Event Idr.</param>
	/// <param name="handle">Handle.</param>
	public void RegisterEventHandle(int id,EventHandler<IEventBase> handle){
		_eventPool.RegisterEventHandle (id, handle);
	}

	/// <summary>
	/// Unregister event handle.
	/// </summary>
	/// <param name="id">Event Id.</param>
	/// <param name="handle">Handle.</param>
	public void UnRegisterEventHandle(int id,EventHandler<IEventBase> handle){
		_eventPool.RegisterEventHandle (id, handle);
	}

	/// <summary>
	/// Trigger the event.
	/// </summary>
	/// <param name="sender">Sender.</param>
	/// <param name="e">Event.</param>
	/// <param name="atOnce">If set to <c>true</c> at once.</param>
	public void TriggerEvent(object sender,IEventBase e,bool atOnce = false){
		if (atOnce) {
			_eventPool.TriggerEventAtOnce (sender, e);
		} else {
			_eventPool.TriggerEvent (sender, e);
		}
	}

	/// <summary>
	/// Clear the pool.
	/// </summary>
	public void ClearPool(){
		_eventPool.ClearPool ();
	}
}
