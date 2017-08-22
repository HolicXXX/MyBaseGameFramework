using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Canvas))]
[RequireComponent(typeof(CanvasScaler))]
[RequireComponent(typeof(GraphicRaycaster))]
public class UIManager : Singleton<UIManager> {

	public static Vector2 UI_Reference_Resolusion = new Vector2(750f,1334f);

	private Dictionary<string,IUIGroup> m_uiGroups;
	private List<int> m_UISerialIdLoading;
	private List<string> m_UIAssetNamesLoading;
	private HashSet<int> m_UISerialIdToRelease;
	private LinkedList<IUIBase> m_RecycleQueue;
	private Dictionary<string,PoolList<UIPoolObject>> m_uiPoolDict;
	private int m_serialId;

	public Type GroupHelperType{ get; set; }
	[SerializeField]
	private Transform m_instanceRoot;

	public IUIBaseHelper m_uiHelper{ get; set; }

	private Action<UIEvent.OpenSuccessEventArgs> _onOpenUISuccessCallback;
	public event Action<UIEvent.OpenSuccessEventArgs> OpenUISuccessHandler{
		add{
			_onOpenUISuccessCallback += value;
		}
		remove{
			_onOpenUISuccessCallback -= value;
		}
	}
	private Action<UIEvent.OpenFailureEventArgs> _onOpenUIFailureCallback;
	public event Action<UIEvent.OpenFailureEventArgs> OpenUIFailureHandler{
		add{
			_onOpenUIFailureCallback += value;
		}
		remove{
			_onOpenUIFailureCallback -= value;
		}
	}
	private Action<UIEvent.OpenProgressEventArgs> _onOpenUIUpdateCallback;
	public event Action<UIEvent.OpenProgressEventArgs> OpenUIUpdateHandler{
		add{
			_onOpenUIUpdateCallback += value;
		}
		remove{
			_onOpenUIUpdateCallback -= value;
		}
	}
	private Action<UIEvent.CloseCompleteEventArgs> _onCloseUICompleteCallback;
	public event Action<UIEvent.CloseCompleteEventArgs> CloseUICompleteHandler{
		add{
			_onCloseUICompleteCallback += value;
		}
		remove{
			_onCloseUICompleteCallback -= value;
		}
	}

	public int GroupCount{ get { return m_uiGroups.Count; } }
	private int _instantCapacity;
	public int InstantCapacity {
		get{ return _instantCapacity; }
		set{ 
			_instantCapacity = value; 
			foreach (var pair in m_uiPoolDict) {
				pair.Value.PoolCapacity = _instantCapacity;
			}
		}
	}

	void Awake () {
		m_uiGroups = new Dictionary<string, IUIGroup> ();
		m_UISerialIdLoading = new List<int> ();
		m_UIAssetNamesLoading = new List<string> ();
		m_UISerialIdToRelease = new HashSet<int> ();
		m_RecycleQueue = new LinkedList<IUIBase> ();
		m_uiPoolDict = new Dictionary<string, PoolList<UIPoolObject>> ();
		m_uiHelper = new DefaultUIHelper ();
		m_serialId = 0;

		GroupHelperType = typeof(DefaultUIGroupHelper);

		_onOpenUISuccessCallback = null;
		_onOpenUIFailureCallback = null;
		_onOpenUIUpdateCallback = null;
		_onCloseUICompleteCallback = null;
	}

	void Start () {
		if (m_instanceRoot.IsNull ()) {
			m_instanceRoot = (new GameObject ("UI_Instance_Root")).transform;
			m_instanceRoot.SetParent (gameObject.transform);
			m_instanceRoot.localScale = Vector3.one;
			m_instanceRoot.gameObject.layer = LayerMask.NameToLayer ("UI");
			m_instanceRoot.gameObject.AddComponent<Canvas> ().renderMode = RenderMode.ScreenSpaceOverlay;
			CanvasScaler scaler = m_instanceRoot.gameObject.AddComponent<CanvasScaler> ();
			scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
			scaler.referenceResolution = UI_Reference_Resolusion;
			m_instanceRoot.gameObject.AddComponent<GraphicRaycaster> ();
		}
	}
	
	// Update is called once per frame
	void Update () {
		while (m_RecycleQueue.Count > 0) {
			IUIBase ui = m_RecycleQueue.First.Value;
			m_RecycleQueue.RemoveFirst ();
			UIPoolObject item = new UIPoolObject ();
			item.Instance = ui;
			PoolList<UIPoolObject> list = null;
			if (m_uiPoolDict.TryGetValue (ui.UIAssetName, out list)) {
				list.ReturnObj (item);
			} else {
				list = new PoolList<UIPoolObject> (_instantCapacity);
				list.ReturnObj (item);
				m_uiPoolDict.Add (ui.UIAssetName, list);
			}
		}

		foreach (KeyValuePair<string, IUIGroup> uiGroup in m_uiGroups)
		{
			uiGroup.Value.Update(Time.deltaTime);
		}
	}

	public void ClearManager(){
		CloseAllLoadedUI (null);
		m_uiGroups.Clear ();
		m_UISerialIdLoading.Clear ();
		m_UIAssetNamesLoading.Clear ();
		m_UISerialIdToRelease.Clear ();
		m_RecycleQueue.Clear ();
	}

	public bool HasUIGroup(string uiGroupName){
		if (!string.IsNullOrEmpty (uiGroupName) && m_uiGroups.ContainsKey (uiGroupName)) {
			return true;
		}
		return false;
	}

	public IUIGroup GetUIGroup(string uiGroupName){
		IUIGroup uiGroup = null;
		if(!string.IsNullOrEmpty(uiGroupName) && m_uiGroups.TryGetValue(uiGroupName, out uiGroup)){
			return uiGroup;
		}
		return null;
	}

	public IUIGroup[] GetAllUIGroups(){
		IUIGroup[] list = new UIGroupBase[m_uiGroups.Count];
		int index = 0;
		foreach (var pair in m_uiGroups) {
			list [index++] = pair.Value;
		}
		return list;
	}

	public bool AddUIGroup(string uiGroupName, int uiGroupDepth){
		IUIGroupHelper helper = (new GameObject ()).AddComponent (GroupHelperType) as IUIGroupHelper;
		if (helper.IsNull ()) {
			Debug.LogError("Create UI Group Helper Failed");
			return false;
		}
		return AddUIGroup (uiGroupName, uiGroupDepth, helper);
	}

	public bool AddUIGroup(string uiGroupName, int uiGroupDepth, IUIGroupHelper uiGroupHelper){
		if (string.IsNullOrEmpty (uiGroupName) || m_uiGroups.ContainsKey (uiGroupName) || uiGroupHelper.IsNull ()) {
			Debug.LogError("Add UI Group Failed");
			return false;
		}
		uiGroupHelper.name = "UI_Group_" + uiGroupName;
		uiGroupHelper.gameObject.layer = LayerMask.NameToLayer ("UI");
		Transform transform = uiGroupHelper.transform;
		transform.SetParent (m_instanceRoot);
		transform.localScale = Vector3.one;

		m_uiGroups.Add (uiGroupName, new UIGroupBase (uiGroupName, uiGroupDepth, uiGroupHelper));
		return true;
	}

	public bool HasUI(int serialId){
		foreach (var pair in m_uiGroups) {
			IUIGroup uiGroup = pair.Value;
			if (uiGroup.HasUI (serialId)) {
				return true;
			}
		}
		return false;
	}

	public bool HasUI(string uiAssetName){
		if (string.IsNullOrEmpty (uiAssetName)) {
			return false;
		}
		foreach (var pair in m_uiGroups) {
			IUIGroup uiGroup = pair.Value;
			if (uiGroup.HasUI (uiAssetName)) {
				return true;
			}
		}
		return false;
	}

	public IUIBase GetUI(int serialId){
		IUIBase ret = null;
		foreach (var pair in m_uiGroups) {
			IUIGroup uiGroup = pair.Value;
			ret = uiGroup.GetUI (serialId);
			if (!ret.IsNull()) {
				break;
			}
		}
		return ret;
	}

	public IUIBase GetUI(string uiAssetName){
		if (string.IsNullOrEmpty (uiAssetName)) {
			return null;
		}
		IUIBase ret = null;
		foreach (var pair in m_uiGroups) {
			IUIGroup uiGroup = pair.Value;
			ret = uiGroup.GetUI (uiAssetName);
			if (!ret.IsNull()) {
				break;
			}
		}
		return ret;
	}

	public IUIBase[] GetAllUIByAssetName(string uiAssetName){
		List<IUIBase> ret = new List<IUIBase> ();
		if (string.IsNullOrEmpty (uiAssetName)) {
			return ret.ToArray ();
		}
		foreach (var pair in m_uiGroups) {
			ret.AddRange (pair.Value.GetUIs (uiAssetName));
		}
		return ret.ToArray ();
	}

	public IUIBase[] GetAllLoadedUIs(){
		List<IUIBase> ret = new List<IUIBase> ();
		foreach (var pair in m_uiGroups) {
			ret.AddRange (pair.Value.GetAllUIs ());
		}
		return ret.ToArray ();
	}

	public int[] GetAllLoadingUISerialId(){
		return m_UISerialIdLoading.ToArray ();
	}

	public bool IsUILoading(int serialId){
		return m_UISerialIdLoading.Contains (serialId);
	}

	public bool IsUIAssetLoading(string uiAssetName){
		return !string.IsNullOrEmpty (uiAssetName) && m_UIAssetNamesLoading.Contains (uiAssetName);
	}

	public bool IsValidUI(IUIBase ui){
		return !ui.IsNull () && HasUI (ui.SerialID);
	}

	public bool IsAssetLoaded(string uiAssetName){
		return !string.IsNullOrEmpty (uiAssetName) && m_uiPoolDict.ContainsKey (uiAssetName);
	}

	public int OpenUI(string uiAssetName, string uiGroupName, bool pauseCoveredUI, object userData){
		if (m_uiHelper.IsNull () || string.IsNullOrEmpty (uiAssetName) || string.IsNullOrEmpty (uiGroupName)) {
			return -1;
		}
		UIGroupBase uiGroup = GetUIGroup (uiGroupName) as UIGroupBase;
		if (uiGroup.IsNull ()) {
			return -1;
		}

		int serialId = m_serialId++;
		bool isNew = false;
		PoolList<UIPoolObject> list = null;
		if (!m_uiPoolDict.TryGetValue (uiAssetName, out list)) {
			list = new PoolList<UIPoolObject> (_instantCapacity);
			m_UISerialIdLoading.Add (serialId);
			m_UIAssetNamesLoading.Add (uiAssetName);
			isNew = true;
		}
		OpenUIInfo info = new OpenUIInfo (serialId, uiGroup, pauseCoveredUI, userData);
		string bundleName = AssetBundleManager.Instance.GetBundleNameByAssetName (uiAssetName);
		float? duration = null;
		AssetBundleManager.Instance.AddAssetTask (bundleName, uiAssetName, args => {
			if(!duration.HasValue){
				duration = Time.deltaTime;
			}else{
				duration += Time.deltaTime;
			}
			LoadUIProgressCallback (args.AssetName, args.Progress, info);
		}, args => {
			LoadUISuccessCallback (args.AssetName, args.Asset, duration.GetValueOrDefault(), info, isNew);
		}, args => {
			LoadUIFailureCallback (args.AssetName, args.Message, info);
		});	
		return serialId;
	}

	private void OpenUI(string uiAssetName, object uiInst, OpenUIInfo info, bool isNewInstance, float duration){
		try{
			IUIBase ui = m_uiHelper.CreateUI(uiInst,info.UIGroup,info.UserData);//This is where UI add to group in scene
			if(ui.IsNull()){
				throw new Exception("Can not create UI in Helper");
			}
			ui.OnInit(info.SerialId,uiAssetName,info.UIGroup,info.PauseCoveredUI,isNewInstance,info.UserData);
			info.UIGroup.AddUI(ui);
			ui.OnEnter(info.UserData);
			info.UIGroup.Refresh();
			UIEvent.OpenSuccessEventArgs args = new UIEvent.OpenSuccessEventArgs( ui,duration,info.UserData);
			if(!_onOpenUISuccessCallback.IsNull()){
				_onOpenUISuccessCallback(args);
			}
			EventPoolManager.Instance.TriggerEvent(this,args);
		}catch(Exception e){
			UIEvent.OpenFailureEventArgs args = new UIEvent.OpenFailureEventArgs (info.SerialId, uiAssetName, info.UIGroup.Name, info.PauseCoveredUI, e.ToString (), info.UserData);
			if (!_onOpenUIFailureCallback.IsNull()) {
				_onOpenUIFailureCallback (args);
			}
			EventPoolManager.Instance.TriggerEvent (this, args);
		}
	}

	public void CloseUI(int serialId, object userData){
		if (IsUILoading (serialId)) {
			m_UISerialIdToRelease.Add (serialId);
			return;
		}
		IUIBase ui = GetUI (serialId);
		if (ui.IsNull ()) {
			Debug.LogError ("Can not find ui with id " + serialId.ToString ());
			return;
		}
		IUIGroup uiGroup = ui.UIGroup;
		if (uiGroup.IsNull ()) {
			Debug.LogError ("UIGroup is invalid. ");
			return;
		}
		uiGroup.RemoveUI (ui);
		ui.OnExit (userData);
		uiGroup.Refresh ();
		UIEvent.CloseCompleteEventArgs args = new UIEvent.CloseCompleteEventArgs (serialId, ui.UIAssetName, uiGroup, userData);
		if (!_onCloseUICompleteCallback.IsNull ()) {
			_onCloseUICompleteCallback (args);
		}
		m_RecycleQueue.AddLast (ui);

		EventPoolManager.Instance.TriggerEvent (this, args);
	}

	public void CloseAllLoadedUI(object userData){
		IUIBase[] allUIs = GetAllLoadedUIs ();
		for (int i = 0; i < allUIs.Length; ++i) {
			CloseUI (allUIs [i].SerialID, userData);
		}
	}

	public void CloseAllLoadingUI(){
		foreach (int id in m_UISerialIdLoading) {
			m_UISerialIdToRelease.Add (id);
		}
	}

	public void RefocusUI(IUIBase ui, object userData){
		if (ui.IsNull ()) {
			Debug.LogError ("UI is invalid.");
			return;
		}
		IUIGroup uiGroup = ui.UIGroup;
		if (uiGroup.IsNull ()) {
			Debug.LogError ("UIGroup is invalid. ");
			return;
		}
		uiGroup.RefocusUI (ui, userData);
		uiGroup.Refresh ();
		ui.OnFocus (userData);
	}

	private void LoadUISuccessCallback (string uiAssetName, object uiAsset, float duration, object userData, bool isNewInstance){
		OpenUIInfo info = userData as OpenUIInfo;
		if (info.IsNull ()) {
			Debug.LogError ("Invalid OpenUIInfo");
			return;
		}

		m_UISerialIdLoading.Remove (info.SerialId);
		m_UIAssetNamesLoading.Remove (uiAssetName);
		if (m_UISerialIdToRelease.Contains (info.SerialId)) {
			Debug.Log ("Release UI " + info.SerialId.ToString () + " On LoadSuccess");
			m_UISerialIdToRelease.Remove (info.SerialId);
			m_uiHelper.ReleaseUI (uiAsset,null);
			return;
		}
		var poolObj = m_uiPoolDict [uiAssetName].GetObj (uiAsset);
		OpenUI (uiAssetName, poolObj.Instance, info, isNewInstance, duration);
	}

	private void LoadUIFailureCallback (string uiAssetName, string errorMsg, object userData){
		OpenUIInfo info = userData as OpenUIInfo;
		if (info.IsNull ()) {
			Debug.LogError ("Invalid OpenUIInfo");
			return;
		}
		m_UISerialIdLoading.Remove (info.SerialId);
		m_UIAssetNamesLoading.Remove (uiAssetName);
		m_UISerialIdToRelease.Remove (info.SerialId);
		string errorMessage = string.Format("Load UI Failure asset name {0}, error message {1}",uiAssetName,errorMsg);
		UIEvent.OpenFailureEventArgs args = new UIEvent.OpenFailureEventArgs (info.SerialId, uiAssetName, info.UIGroup.Name, info.PauseCoveredUI, errorMessage, info.UserData);
		if (!_onOpenUIFailureCallback.IsNull ()) {
			_onOpenUIFailureCallback (args);
		}
		EventPoolManager.Instance.TriggerEvent (this, args);
	}

	private void LoadUIProgressCallback (string uiAssetName, float progress, object userData){
		OpenUIInfo info = userData as OpenUIInfo;
		if (info.IsNull ()) {
			Debug.LogError ("Invalid OpenUIInfo");
			return;
		}
		UIEvent.OpenProgressEventArgs args = new UIEvent.OpenProgressEventArgs (info.SerialId, uiAssetName, info.UIGroup.Name, info.PauseCoveredUI, progress, info.UserData);
		if (!_onOpenUIUpdateCallback.IsNull ()) {
			_onOpenUIUpdateCallback (args);
		}
		EventPoolManager.Instance.TriggerEvent (this, args);
	}


}
