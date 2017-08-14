using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using LitJson;

public class AssetBundleManager : Singleton<AssetBundleManager> {

	public float TimeOut{ get; set; }
	public bool ConfigPreLoaded{ get ; set; }

	private TaskPool<AssetBundleTask> _assetBundleTaskPool;
	public int TotalAssetBundleAgentCount{ get { return _assetBundleTaskPool.TotalAgentCount; } }
	public int FreeAssetBundleAgentCount{ get { return _assetBundleTaskPool.FreeAgentCount; } }
	public int WorkingAssetBundleAgentCount{ get { return _assetBundleTaskPool.WorkingAgentCount; } }
	public int WaitingAssetBundleTaskCount{ get { return _assetBundleTaskPool.WaitingTaskCount; } }
	private Action<int> _onAssetBundleStartCallback;
	private Action<AssetBundle> _onAssetBundleSuccessCallback;
	private Action<string> _onAssetBundleFailureCallback;
	public event Action<int> AssetBundleStartHandle {add{ _onAssetBundleStartCallback += value;}remove{ _onAssetBundleStartCallback -= value;}}
	public event Action<AssetBundle> AssetBundleSuccessHandle {add{ _onAssetBundleSuccessCallback += value;}remove{ _onAssetBundleSuccessCallback -= value;}}
	public event Action<string> AssetBundleFailureHandle {add{ _onAssetBundleFailureCallback += value;}remove{ _onAssetBundleFailureCallback -= value;}}

	private TaskPool<AssetTask> _assetTaskPool;
	public int TotalAssetAgentCount{ get { return _assetTaskPool.TotalAgentCount; } }
	public int FreeAssetAgentCount{ get { return _assetTaskPool.FreeAgentCount; } }
	public int WorkingAssetAgentCount{ get { return _assetTaskPool.WorkingAgentCount; } }
	public int WaitingAssetTaskCount{ get { return _assetTaskPool.WaitingTaskCount; } }
	private Action<int> _onAssetStartCallback;
	private Action<UnityEngine.Object> _onAssetSuccessCallback;
	private Action<string> _onAssetFailureCallback;
	public event Action<int> AssetStartHandle {add{ _onAssetStartCallback += value;}remove{ _onAssetStartCallback -= value;}}
	public event Action<UnityEngine.Object> AssetSuccessHandle {add{ _onAssetSuccessCallback += value;}remove{ _onAssetSuccessCallback -= value;}}
	public event Action<string> AssetFailureHandle {add{ _onAssetFailureCallback += value;}remove{ _onAssetFailureCallback -= value;}}

	Dictionary<string,AssetBundle> AssetBundleDict;
	Dictionary<string,UnityEngine.Object> AssetDict;
	Dictionary<string,int> LoadingAssetBundleDict;
	Dictionary<string,int> LoadingAssetDict;

	private JsonData _assetInfo;

	void Awake() {
		_assetBundleTaskPool = new TaskPool<AssetBundleTask> ();
		_assetTaskPool = new TaskPool<AssetTask> ();

		TimeOut = 30f;
		AssetBundleDict = new Dictionary<string, AssetBundle> ();
		AssetDict = new Dictionary<string, UnityEngine.Object> ();
		LoadingAssetBundleDict = new Dictionary<string, int> ();
		LoadingAssetDict = new Dictionary<string,int> ();

		_assetInfo = null;
		ConfigPreLoaded = false;

		_onAssetBundleStartCallback = id => {
			Debug.LogWarning ("AssetBundleTask " + id + " Start");
		};
		_onAssetBundleSuccessCallback = ab => {
			if(!ab.IsNull()){
				AssetBundleDict[ab.name] = ab;
			}
			LoadingAssetBundleDict.Remove(ab.name);
			Debug.LogWarning("AssetBundleTask Success: " + (ab.IsNull() ? "" : ab.name));
		};
		_onAssetBundleFailureCallback = msg => {
			LoadingAssetBundleDict.Remove(msg);
			Debug.LogError("AssetBundleTask Error: " + msg);	
		};

		_onAssetStartCallback = id => {
			Debug.LogWarning ("AssetTask " + id + " Start");
		};
		_onAssetSuccessCallback = asset => {
			if(!asset.IsNull()){
				AssetDict[asset.name] = asset;
			}
			LoadingAssetDict.Remove(asset.name);
			Debug.LogWarning("AssetTask Success: " + (asset.IsNull() ? "" : asset.name));
		};
		_onAssetFailureCallback = msg => {
			LoadingAssetDict.Remove(msg);
			Debug.LogError("AssetTask Error: " + msg);	
		};

		this.InitAssetInfo ();
	}

	private void InitAssetInfo(){
		this.AddAssetTask("Config","AssetConfig.json", null, asset => {
			var text =  asset as TextAsset;
			_assetInfo = JsonUtils.ReadJsonString(text.text);
			ConfigPreLoaded = true;
		}, msg => {
			Debug.LogError("Load AssetConfig.json failed!!!");
		});
	}

	public string GetBundleNameWithAssetName(string assetName){
		if (_assetInfo.IsNull () || string.IsNullOrEmpty (assetName)) {
			return null;
		}
		if (_assetInfo ["AssetToBundle"].Keys.Contains (assetName)) {
			return _assetInfo ["AssetToBundle"] [assetName].ToString ();
		} else {
			return null;
		}
	}

	public string[] GetAssetNameListWithBundleName(string bundleName){
		if (_assetInfo.IsNull () || string.IsNullOrEmpty (bundleName)) {
			return null;
		}
		if (_assetInfo ["BundleToAssetList"].Keys.Contains (bundleName)) {
			var list = _assetInfo ["BundleToAssetList"] [bundleName];
			string[] ret = new string[list.Count];
			for (int i = 0; i < list.Count; ++i) {
				ret [i] = list [i].ToString ();
			}
			return ret;
		}
		return null;
	}

	void Start () {
	}
	void Update (){
		_assetBundleTaskPool.Update (Time.fixedUnscaledDeltaTime);
		_assetTaskPool.Update (Time.fixedUnscaledDeltaTime);
	}

	/// <summary>
	/// Determines whether this instance has asset bundle the specified bname.
	/// </summary>
	/// <returns><c>true</c> if this instance has asset bundle the specified bname; otherwise, <c>false</c>.</returns>
	/// <param name="bname">AssetBundle name.</param>
	public bool HasAssetBundle (string bname){
		return AssetBundleDict.ContainsKey (bname) && !AssetBundleDict [bname].IsNull();
	}

	public bool HasAsset (string aname){
		return AssetDict.ContainsKey (aname) && !AssetDict [aname].IsNull ();
	}

	public bool IsAssetBundleLoading (string bname){
		return LoadingAssetBundleDict.ContainsKey (bname);
	}

	public bool IsAssetLoading (string aname){
		return LoadingAssetDict.ContainsKey (aname);
	}

	public int GetLoadingAssetBundleTaskID (string bname){
		int ret = -1;
		LoadingAssetBundleDict.TryGetValue (bname, out ret);
		return ret;
	}

	public int GetLoadingAssetTaskID (string name){
		int ret = -1;
		LoadingAssetDict.TryGetValue (name, out ret);
		return ret;
	}

	public void AddAssetBundleAgent (Action<int> startCallback){
		var agent = new AssetBundleTaskAgent ();
		agent.OnAssetBundleStartCallback = startCallback;
		agent.OnAssetBundleStartCallback += OnAssetBundleStart;
		agent.AssetBundleSuccessHandle += OnAssetBundleSuccess;
		agent.AssetBundleFailureHandle += OnAssetBundleFailure;

		_assetBundleTaskPool.AddAgent (agent);
	}

	public void AddFromFileTask (string bname,Action<float> progCallback = null,Action<AssetBundle> sucCallback = null,Action<string> failCallback = null){
		if (string.IsNullOrEmpty (bname)) {
			Debug.LogError ("Invalid Bundle Name");
			return;
		}
		var path = string.Format ("{0}/{1}", Application.streamingAssetsPath, bname);
		AddAssetBundleTask (bname, path, true, progCallback, sucCallback, failCallback);
	}

	public void AddDownloadTask (string bname,string url,Action<float> progCallback = null,Action<AssetBundle> sucCallback = null,Action<string> failCallback = null){
		if (string.IsNullOrEmpty (bname)) {
			Debug.LogError ("Invalid Bundle Name");
			return;
		}
		AddAssetBundleTask (bname, url, false, progCallback, sucCallback, failCallback);
	}

	private void AddAssetBundleTask (string bname,string bpath,bool fromfile = true,Action<float> progCallback = null,Action<AssetBundle> sucCallback = null,Action<string> failCallback = null){
		if (TotalAssetBundleAgentCount <= 0) {
			Debug.LogWarning ("Add an agent first");
			this.AddAssetBundleAgent (null);
		}
		if (AssetBundleDict.ContainsKey (bname)) {
			Debug.LogWarning ("AssetBundle " + bname + " already Exists!");
			sucCallback (AssetBundleDict [bname]);
			return;
		}
		if (LoadingAssetBundleDict.ContainsKey (bname)) {
			Debug.LogWarning ("AssetBundle " + bname + " is Loading!");
			return;
		}
		var task = new AssetBundleTask (bname, bpath, TimeOut, fromfile);
		task.AssetBundleProgressHandle += progCallback;
		task.AssetBundleSuccessHandle += sucCallback;
		task.AssetBundleFailureHandle += failCallback;
		_assetBundleTaskPool.AddTask (task);
		LoadingAssetBundleDict.Add (bname,task.ID);
	}

	public void AddAssetAgent (Action<int> startCallback){
		var agent = new AssetTaskAgent ();
		agent.OnAssetStartCallback = startCallback;
		agent.OnAssetStartCallback += OnAssetStart;
		agent.AssetSuccessHandle += OnAssetSuccess;
		agent.AssetFailureHandle += OnAssetFailure;

		_assetTaskPool.AddAgent (agent);
	}

	public void AddAssetTask (string bname, string name, Action<float> progCallback = null,Action<UnityEngine.Object> sucCallback = null,Action<string> failCallback = null){
		if (string.IsNullOrEmpty (bname)) {
			Debug.LogError ("Invalid Bundle Name");
			return;
		}
		if (!AssetBundleDict.ContainsKey (bname)) {
			Debug.LogWarning ("AssetBundle " + bname + " hasn't loaded yet!Load the bundle first.");
			if (IsAssetBundleLoading (bname)) {
				CoroutineUtils.WaitUntil (() => this.HasAssetBundle (bname), () => {
					AddAssetTask (AssetBundleDict [bname], name, progCallback, sucCallback, failCallback);
				});
			} else {
				AddFromFileTask (bname, null, ab => {
					AddAssetTask (ab, name, progCallback, sucCallback, failCallback);
				});
			}
		}
		AddAssetTask (AssetBundleDict [bname], name, progCallback, sucCallback, failCallback);
	}

	private void AddAssetTask(AssetBundle ab,string name,Action<float> progCallback = null,Action<UnityEngine.Object> sucCallback = null,Action<string> failCallback = null){
		if (TotalAssetAgentCount <= 0) {
			Debug.LogWarning ("Add an agent first");
			this.AddAssetAgent (null);
		}
		if (AssetDict.ContainsKey (name)) {
			Debug.LogWarning ("Asset " + name + " already Exists!");
			sucCallback (AssetDict [name]);
			return;
		}
		if (LoadingAssetDict.ContainsKey (name)) {
			Debug.LogWarning ("AssetBundle " + name + " is Loading!");
			return;
		}

		var task = new AssetTask (name, TimeOut, ab);
		task.AssetProgressHandle += progCallback;
		task.AssetSuccessHandle += sucCallback;
		task.AssetFailureHandle += failCallback;
		_assetTaskPool.AddTask (task);
		LoadingAssetDict.Add (name,task.ID);
	}
		
	public void AddAllAssetsTask(string bname, Action<float> progCallback, Action<UnityEngine.Object[]> sucCallback, Action<string> failCallback){
		var coroutineInst = CoroutineManager.Instance;
		if (!AssetBundleDict.ContainsKey (bname)) {
			Debug.LogWarning ("AssetBundle " + bname + " hasn't loaded yet!Load the bundle first.");
			if (!IsAssetBundleLoading (bname)) {
				CoroutineUtils.WaitUntil (() => this.HasAssetBundle (bname), () => {
					coroutineInst.StartNewCoroutineTask (loadAllAssetsAsyn (bname, sucCallback));
				});
			} else {
				AddFromFileTask (bname, null, ab => {
					coroutineInst.StartNewCoroutineTask (loadAllAssetsAsyn (ab, sucCallback));
				});
			}
		}
		coroutineInst.StartNewCoroutineTask (loadAllAssetsAsyn (bname, sucCallback));
	}
	//Might Need Rewrite
	IEnumerator loadAllAssetsAsyn (string bname, Action<UnityEngine.Object[]> callback) {
		var bundle = AssetBundleDict [bname];
		var abr = bundle.LoadAllAssetsAsync ();
		yield return abr;
		callback (abr.allAssets ?? null);
	}
	IEnumerator loadAllAssetsAsyn (AssetBundle bundle, Action<UnityEngine.Object[]> callback) {
		var abr = bundle.LoadAllAssetsAsync ();
		yield return abr;
		callback (abr.allAssets ?? null);
	}

	/// <summary>
	/// Unloads the asset bundle with a name.
	/// </summary>
	/// <param name="bname">Bundle name.</param>
	/// <param name="unloadAllLoadedObj">If set to <c>true</c> unload all loaded object.</param>
	public void UnloadAssetBundle(string bname,bool unloadAllLoadedObj = false){
		if (!AssetBundleDict.ContainsKey (bname) || AssetBundleDict [bname].IsNull()) {
			return;
		}
		var bundle = AssetBundleDict [bname];
		AssetBundleDict.Remove (bname);
		bundle.Unload (unloadAllLoadedObj);
	}

	/// <summary>
	/// Unloads all asset bundle.
	/// </summary>
	/// <param name="unloadAllLoadedObj">If set to <c>true</c> unload all loaded object.</param>
	public void UnloadAllAssetBundle(bool unloadAllLoadedObj = false){
		if (AssetBundleDict.Count == 0) {
			return;
		}
		foreach (var pair in AssetBundleDict) {
			var bundle = pair.Value;
			bundle.Unload (unloadAllLoadedObj);
		}
		AssetBundleDict.Clear ();
	}

	public bool RemoveAssetBundleTask(int serialID){
		bool suc = !_assetBundleTaskPool.RemoveTask (serialID).IsNull ();
		if (suc) {
			string rm = null;
			foreach (var item in LoadingAssetBundleDict) {
				if (item.Value == serialID) {
					rm = item.Key;
				}
			}
			if (!rm.IsNull ()) {
				LoadingAssetBundleDict.Remove (rm);
			}
		}
		return suc;
	}

	public void RemoveAllAssetBundleTasks(){
		_assetBundleTaskPool.RemoveAllTasks ();
		LoadingAssetBundleDict.Clear ();
	}

	public void OnAssetBundleStart(int id){
		if (!_onAssetBundleStartCallback.IsNull ()) {
			_onAssetBundleStartCallback (id);
		}
	}

	public void OnAssetBundleSuccess(AssetBundle ab){
		if (!_onAssetBundleSuccessCallback.IsNull ()) {
			_onAssetBundleSuccessCallback (ab);
		}
	}

	public void OnAssetBundleFailure(string msg){
		if (!_onAssetBundleFailureCallback.IsNull ()) {
			_onAssetBundleFailureCallback (msg);
		}
	}

	public void UnloadAsset(string aname){
		if (!AssetDict.ContainsKey (aname) || AssetDict [aname].IsNull()) {
			return;
		}
		var asset = AssetDict [aname];
		AssetDict.Remove (aname);
		UnityEngine.Object.Destroy (asset);
	}

	public void UnloadAllAsset(){
		if (AssetDict.Count == 0) {
			return;
		}
		foreach (var item in AssetDict) {
			UnityEngine.Object.Destroy (item.Value);
		}
		AssetDict.Clear ();
	}

	public bool RemoveAssetTask(int serialID){
		bool suc = !_assetTaskPool.RemoveTask (serialID).IsNull ();
		if (suc) {
			string rm = null;
			foreach (var item in LoadingAssetDict) {
				if (item.Value == serialID) {
					rm = item.Key;
				}
			}
			if (!rm.IsNull ()) {
				LoadingAssetDict.Remove (rm);
			}
		}
		return suc;
	}

	public void RemoveAllAssetTasks(){
		_assetTaskPool.RemoveAllTasks ();
		LoadingAssetDict.Clear ();
	}

	public void OnAssetStart(int id){
		if (!_onAssetStartCallback.IsNull ()) {
			_onAssetStartCallback (id);
		}
	}

	public void OnAssetSuccess(UnityEngine.Object asset){
		if (!_onAssetSuccessCallback.IsNull ()) {
			_onAssetSuccessCallback (asset);
		}
	}

	public void OnAssetFailure(string msg){
		if (!_onAssetFailureCallback.IsNull ()) {
			_onAssetFailureCallback (msg);
		}
	}

	public void CloseManager(){
		_assetTaskPool.ClearPool ();
		_assetBundleTaskPool.ClearPool ();
	}

	protected override void OnDestroy(){
		this.UnloadAllAsset ();
		this.UnloadAllAssetBundle ();
		base.OnDestroy ();
	}

	#region Old API
//	/// <summary>
//	/// Loads the assetbundle asyn.
//	/// </summary>
//	/// <param name="bname">Bundle name.</param>
//	public void LoadAssetBundleAsyn(string bname,Action cb = null){
//		StartCoroutine (loadAssetBundleAsyn (bname, cb));
//	}
//	IEnumerator loadAssetBundleAsyn(string bname,Action cb) {
//		if (AssetBundleDict.ContainsKey (bname) && !AssetBundleDict [bname].IsNull()) {
//			yield break;
//		}
//		string fullpath = String.Format ("{0}/{1}", Application.streamingAssetsPath, bname);//$"{Application.streamingAssetsPath}/{bname}";
//		var abcr = AssetBundle.LoadFromFileAsync (fullpath);
//		yield return abcr;
//		AssetBundleDict [bname] = abcr.assetBundle ?? null;
//		if (!cb.IsNull())
//			cb ();
//	}
//	/// <summary>
//	/// Loads one asset in the bundle asyn.
//	/// </summary>
//	/// <param name="bname">Target Bundle name.</param>
//	/// <param name="aname">Asset name.</param>
//	/// <param name="callback">Callback.</param>
//	public void LoadAssetAsyn(string bname, string aname, Action<UnityEngine.Object> callback){
//		StartCoroutine (loadAssetAsyn (bname, aname, callback));
//	}
//	/// <summary>
//	/// Loads all assets in the bundle asyn.
//	/// </summary>
//	/// <param name="bname">Target Bundle name.</param>
//	/// <param name="callback">Callback receive result</param>
//	public void LoadAllAssetsAsyn(string bname, Action<UnityEngine.Object[]> callback){
//		StartCoroutine (loadAllAssetsAsyn (bname, callback));
//	}
	#endregion
}
