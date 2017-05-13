using System;

/// <summary>
/// Interface Eventbase.
/// </summary>
[Serializable]
public abstract class IEventBase : EventArgs {

	public abstract int ID{ get;}

}
