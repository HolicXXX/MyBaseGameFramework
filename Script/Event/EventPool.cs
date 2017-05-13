using System;
using System.Collections.Generic;

/// <summary>
/// Event pool.
/// </summary>
/// <typeparam>T is IEventBase </typeparam>
public partial class EventPool<T> where T : IEventBase {
	
	/// <summary>
	/// The handles' dictionary.
	/// </summary>
	Dictionary<int,EventHandler<T>> _handlesDict;

	/// <summary>
	/// The events' queue.
	/// </summary>
	Queue<Event> _eventQueue;

	/// <summary>
	/// Get the current events' count.
	/// </summary>
	/// <value>The event count.</value>
	public int EventCount{
		get{ return _eventQueue.Count;}
	}

	public EventPool(){
		_handlesDict = new Dictionary<int, EventHandler<T>> ();
		_eventQueue = new Queue<Event> ();
	}

	/// <summary>
	/// Deal with the events in queue per frame.
	/// </summary>
	public void Update(){
		while (_eventQueue.Count > 0) {
			Event e;
			lock (_eventQueue) {
				e = _eventQueue.Dequeue ();
			}
			HandleEvent (e.Sender, e.EventArgs);
		}
	}

	/// <summary>
	/// Check the handler.
	/// </summary>
	/// <returns><c>true</c>, if handler was already register, <c>false</c> otherwise.</returns>
	/// <param name="id">Event Id.</param>
	/// <param name="handle">Handle.</param>
	public bool CheckHandler(int id,EventHandler<T> handle){
		if (handle.IsNull() || !_handlesDict.ContainsKey (id) || _handlesDict [id].IsNull()) {
			return false;
		} else {
			foreach (var h in _handlesDict[id].GetInvocationList()) {
				if (h == handle)
					return true;
			}
		}
		return false;
	}

	/// <summary>
	/// Register the event handle for event id.
	/// </summary>
	/// <param name="id">Event Idr.</param>
	/// <param name="handle">Handle.</param>
	public void RegisterEventHandle(int id,EventHandler<T> handle){
		EventHandler<T> handles = null;
		if (!_handlesDict.TryGetValue (id, out handles) || handles.IsNull()) {
			_handlesDict [id] = handle;
		} else {
			handles += handle;
			_handlesDict [id] = handles;
		}
	}

	/// <summary>
	/// Unregister event handle for the id.
	/// </summary>
	/// <param name="id">Event Id.</param>
	/// <param name="handle">Handle.</param>
	public void UnRegisterEventHandle(int id,EventHandler<T> handle){
		if (!handle.IsNull() && _handlesDict.ContainsKey (id) && !_handlesDict [id].IsNull()) {
			_handlesDict [id] -= handle;
		}
	}

	/// <summary>
	/// Trigger the event(into the queue).
	/// </summary>
	/// <param name="sender">Sender.</param>
	/// <param name="e">Event.</param>
	public void TriggerEvent(object sender, T e){
		Event eve = new Event (sender, e);
		lock (_eventQueue) {
			_eventQueue.Enqueue (eve);
		}
	}

	/// <summary>
	/// Triggers the event at once.
	/// </summary>
	/// <param name="sender">Sender.</param>
	/// <param name="e">Event.</param>
	public void TriggerEventAtOnce(object sender,T e){
		HandleEvent (sender, e);
	}

	/// <summary>
	/// Deal the event with the handle that registered.
	/// </summary>
	/// <param name="sender">Sender.</param>
	/// <param name="e">E.</param>
	void HandleEvent(object sender,T e){
		EventHandler<T> handles = null;
		if (_handlesDict.TryGetValue (e.ID, out handles)) {
			if (!handles.IsNull()) {
				handles (sender, e);
			}
		}
	}

	/// <summary>
	/// Clear the pool.
	/// </summary>
	public void ClearPool(){
		lock (_eventQueue) {
			_eventQueue.Clear ();
		}
		_handlesDict.Clear ();
	}
}
