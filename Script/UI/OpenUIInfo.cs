using System;

public class OpenUIInfo
{
	public int SerialId{ get; private set; }
	public IUIGroup UIGroup{ get; private set; }
	public bool PauseCoveredUI{ get; private set; }
	public object UserData{ get; private set; }
	public OpenUIInfo (int serialId, IUIGroup uiGroup, bool pauseCoveredUI, object userData)
	{
		SerialId = serialId;
		UIGroup = uiGroup;
		PauseCoveredUI = pauseCoveredUI;
		UserData = userData;
	}
}

