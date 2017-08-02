using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AssetBundleManager : Singleton<AssetBundleManager> {

	private TaskPool<AssetBundleTask> _taskPool;
	public float TimeOut{ get; set; }

	public int TotalAgentCount{ get { return _taskPool.TotalAgentCount; } }
	public int FreeAgentCount{ get { return _taskPool.FreeAgentCount; } }
	public int WorkingAgentCount{ get { return _taskPool.WorkingAgentCount; } }
	public int WaitingTaskCount{ get { return _taskPool.WaitingTaskCount; } }

	private Action<int> _onAssetBundleStartCallback;
	private Action<AssetBundle> _onAssetBundleSuccessCallback;
	private Action<string> _onAssetBundleFailureCallback;

	public event Action<int> AssetBundleStartHandle {add{ _onAssetBundleStartCallback += value;}remove{ _onAssetBundleStartCallback -= value;}}
	public event Action<AssetBundle> AssetBundleSuccessHandle {add{ _onAssetBundleSuccessCallback += value;}remove{ _onAssetBundleSuccessCallback -= value;}}
	public event Action<string> AssetBundleFailureHandle {add{ _onAssetBundleFailureCallback += value;}remove{ _onAssetBundleFailureCallback -= value;}}

	Dictionary<string,AssetBundle> AssetBundleDict;

	void Awake() {
		_taskPool = new TaskPool<AssetBundleTask> ();
		TimeOut = 30f;
		AssetBundleDict = new Dictionary<string, AssetBundle> ();

		_onAssetBundleStartCallback = id => {
			Debug.LogWarning ("ABTask " + id + " Start");
		};
		_onAssetBundleSuccessCallback = ab => {
			if(!ab.IsNull()){
				AssetBundleDict[ab.name] = ab;
			}
			Debug.LogWarning("ABTask Success: " + (ab.IsNull() ? "" : ab.name));
		};
		_onAssetBundleFailureCallback = msg => {
			Debug.LogError("ABTask Error: " + msg);	
		};
	}
	void Start () {
	}
	void Update (){
		_taskPool.Update (Time.deltaTime);
	}

	/// <summary>
	/// Determines whether this instance has asset bundle the specified bname.
	/// </summary>
	/// <returns><c>true</c> if this instance has asset bundle the specified bname; otherwise, <c>false</c>.</returns>
	/// <param name="bname">AssetBundle name.</param>
	public bool HasAssetBundle (string bname){
		return AssetBundleDict.ContainsKey (bname) && !AssetBundleDict [bname].IsNull();
	}

	public void AddAgent (Action<int> startCallback){
		var agent = new AssetBundleTaskAgent ();
		agent.OnAssetBundleStartCallback = startCallback;
		agent.OnAssetBundleStartCallback += OnAssetBundleStart;
		agent.AssetBundleSuccessHandle += OnAssetBundleSuccess;
		agent.AssetBundleFailureHandle += OnAssetBundleFailure;

		_taskPool.AddAgent (agent);
	}

	public int AddFromFileTask (string bname,Action<float> progCallback = null,Action<AssetBundle> sucCallback = null,Action<string> failCallback = null){
		var path = string.Format ("{0}/{1}", Application.streamingAssetsPath, bname);
		return AddAssetBundleTask (bname, path, true, progCallback, sucCallback, failCallback);
	}

	public int AddDownloadTask (string bname,string url,Action<float> progCallback = null,Action<AssetBundle> sucCallback = null,Action<string> failCallback = null){
		return AddAssetBundleTask (bname, url, false, progCallback, sucCallback, failCallback);
	}

	private int AddAssetBundleTask (string bname,string bpath,bool fromfile = true,Action<float> progCallback = null,Action<AssetBundle> sucCallback = null,Action<string> failCallback = null){
		if (TotalAgentCount <= 0) {
			Debug.LogWarning ("Add an agent first");
		}
		if (AssetBundleDict.ContainsKey (bname)) {
			Debug.LogWarning ("AssetBundle " + bname + " already Exists!");
			return -1;
		}
		var task = new AssetBundleTask (bname, bpath, TimeOut, fromfile);
		task.AssetBundleProgressHandle += progCallback;
		task.AssetBundleSuccessHandle += sucCallback;
		task.AssetBundleFailureHandle += failCallback;

		return task.ID;
	}

	public int AddAssetTask (string bname, string name, Action<UnityEngine.Object> callback){
		if (!AssetBundleDict.ContainsKey (bname)) {
			Debug.LogError ("AssetBundle " + bname + " hasn't loaded yet!Load the bundle first.");
			return -1;
		}
		return CoroutineManager.Instance.StartNewCoroutineTask (loadAssetAsyn (bname, name, callback));
	}

	public int AddAllAssetsTask(string bname, Action<UnityEngine.Object[]> callback){
		if (!AssetBundleDict.ContainsKey (bname)) {
			Debug.LogError ("AssetBundle " + bname + " hasn't loaded yet!Load the bundle first.");
			return -1;
		}
		return CoroutineManager.Instance.StartNewCoroutineTask (loadAllAssetsAsyn (bname, callback));
	}

	IEnumerator loadAssetAsyn(string bname, string aname, Action<UnityEngine.Object> callback) {
//		if (!AssetBundleDict.ContainsKey (bname) || AssetBundleDict [bname].IsNull()) {
//			yield return loadAssetBundleAsyn (bname,null);
//		}
		var bundle = AssetBundleDict [bname];
		var abr = bundle.LoadAssetAsync (aname);
		yield return abr;
		callback (abr.asset ?? null);
	}
	IEnumerator loadAllAssetsAsyn (string bname, Action<UnityEngine.Object[]> callback) {
//		if (!AssetBundleDict.ContainsKey (bname) || AssetBundleDict [bname].IsNull()) {
//			yield return loadAssetBundleAsyn (bname,null);
//		}
		var bundle = AssetBundleDict [bname];
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

	public bool RemoveTask(int serialID){
		return !_taskPool.RemoveTask (serialID).IsNull ();
	}

	public void RemoveAllTasks(){
		_taskPool.RemoveAllTasks ();
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

	public void CloseManager(){
		_taskPool.ClearPool ();
	}

	protected override void OnDestroy(){
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
