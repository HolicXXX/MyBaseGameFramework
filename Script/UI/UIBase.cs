using UnityEngine;
using System.Collections;

public abstract class UIBase : MonoBehaviour, IUIBase
{
	public int SerialID{ get; private set; }
	public string UIName{ get{ return gameObject.name;} set{ gameObject.name = value;} }
	public string UIAssetName{ get; private set; }
	public object Handler{ get{ return gameObject;} }
	public IUIGroup UIGroup{ get; private set; }
	public int DepthInUIGroup{ get; private set; }
	public bool PauseCoveredUI{ get; private set; }

	public virtual void OnInit (int serialId, string uiAssetName, IUIGroup uiGroup, bool pauseCoveredUI, bool isNewInstance,object userData){
		SerialID = serialId;
		UIAssetName = uiAssetName;
		if (isNewInstance) {
			UIGroup = uiGroup;
		}
		DepthInUIGroup = 0;
		PauseCoveredUI = pauseCoveredUI;
	}

	public virtual void OnEnter (object userData){
		gameObject.SetActive (true);
	}

	public virtual void OnPause (){
		gameObject.SetActive (false);
	}

	public virtual void OnUpdate (float dt){
		
	}

	public virtual void OnResume (){
		gameObject.SetActive (true);
	}

	public virtual void OnExit (object userData){
		gameObject.SetActive (false);
	}

	public virtual void OnFocus (object userData){
		
	}

	public virtual void OnCover (){
	
	}

	public virtual void OnReveal (){
	
	}

	public virtual void OnDepthChanged (int uiGroupDepth,int depthInUIGroup){
		DepthInUIGroup = depthInUIGroup;
	}

}

