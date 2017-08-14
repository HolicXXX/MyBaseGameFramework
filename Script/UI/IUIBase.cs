
public interface IUIBase {

	int SerialID{ get; }
	string UIName{ get; }
	string UIAssetName{ get; }
	object Handler{ get; }
	IUIGroup UIGroup{ get; }
	int DepthInUIGroup{ get; }
	bool PauseCoveredUI{ get; }

	void OnInit (int serialId, string uiAssetName, IUIGroup uiGroup, bool pauseCoveredUIForm, bool isNewInstance,object userData);
	void OnEnter (object userData);
	void OnPause ();
	void OnUpdate (float dt);
	void OnResume ();
	void OnExit (object userData);
	void OnFocus (object userData);
	void OnCover ();
	void OnReveal ();
	void OnDepthChanged (int uiGroupDepth,int depthInUIGroup);
}
