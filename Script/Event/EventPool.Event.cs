
/// <summary>
/// Event handler.
/// </summary>
public delegate void EventHandler<T>(object sender,T e) where T : IEventBase;

public partial class EventPool<T>{
	
	/// <summary>
	/// Event.Used in pool
	/// </summary>
	public class Event {
		public object Sender{ get; private set; }
		public T EventArgs{ get; private set; }

		public Event(object sender,T e){
			Sender = sender;
			EventArgs = e;
		}
	}
}

