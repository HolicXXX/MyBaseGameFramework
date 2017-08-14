using System;

public class IUIBaseInfo
{
	public IUIBase UIInstance{ get; private set; }
	public bool Paused{ get; set; }
	public bool Covered{ get; set; }

	public IUIBaseInfo (IUIBase inst)
	{
		UIInstance = inst;
		Paused = true;
		Covered = true;
	}
}

