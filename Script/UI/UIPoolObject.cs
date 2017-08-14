using UnityEngine;

public class UIPoolObject : CustomInterfaces.IResetable
{
	public object Instance{ get; set; }

	public UIPoolObject ()
	{
		Instance = null;
	}

	public void InitOnce(object userData){
		if (Instance.IsNull() && !userData.IsNull()) {
			Instance = Object.Instantiate ((Object)userData);
		}
	}

	public void Reset(object userData){
		if (userData != null) {
			Instance = userData;
		}
	}

	public void Destroy(){
		UnityEngine.Object.Destroy ((Object)Instance);
		Instance = null;
	}
}

