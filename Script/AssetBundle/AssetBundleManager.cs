using System;
using System.IO;
using System.Xml;
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
	private Action<AssetBundleEvent.StartEventArgs> _onAssetBundleStartCallback;
	private Action<AssetBundleEvent.ProgressEventArgs> _onAssetBundleProgressCallback;
	private Action<AssetBundleEvent.SuccessEventArgs> _onAssetBundleSuccessCallback;
	private Action<AssetBundleEvent.FailureEventArgs> _onAssetBundleFailureCallback;
	public event Action<AssetBundleEvent.StartEventArgs> AssetBundleStartHandle {add{ _onAssetBundleStartCallback += value;}remove{ _onAssetBundleStartCallback -= value;}}
	public event Action<AssetBundleEvent.ProgressEventArgs> AssetBundleProgressHandle {add{ _onAssetBundleProgressCallback += value;}remove{ _onAssetBundleProgressCallback -= value;}}
	public event Action<AssetBundleEvent.SuccessEventArgs> AssetBundleSuccessHandle {add{ _onAssetBundleSuccessCallback += value;}remove{ _onAssetBundleSuccessCallback -= value;}}
	public event Action<AssetBundleEvent.FailureEventArgs> AssetBundleFailureHandle {add{ _onAssetBundleFailureCallback += value;}remove{ _onAssetBundleFailureCallback -= value;}}

	private TaskPool<AssetTask> _assetTaskPool;
	public int TotalAssetAgentCount{ get { return _assetTaskPool.TotalAgentCount; } }
	public int FreeAssetAgentCount{ get { return _assetTaskPool.FreeAgentCount; } }
	public int WorkingAssetAgentCount{ get { return _assetTaskPool.WorkingAgentCount; } }
	public int WaitingAssetTaskCount{ get { return _assetTaskPool.WaitingTaskCount; } }
	private Action<AssetEvent.StartEventArgs> _onAssetStartCallback;
	private Action<AssetEvent.ProgressEventArgs> _onAssetProgressCallback;
	private Action<AssetEvent.SuccessEventArgs> _onAssetSuccessCallback;
	private Action<AssetEvent.FailureEventArgs> _onAssetFailureCallback;
	public event Action<AssetEvent.StartEventArgs> AssetStartHandle {add{ _onAssetStartCallback += value;}remove{ _onAssetStartCallback -= value;}}
	public event Action<AssetEvent.ProgressEventArgs> AssetProgressHandle {add{ _onAssetProgressCallback += value;}remove{ _onAssetProgressCallback -= value;}}
	public event Action<AssetEvent.SuccessEventArgs> AssetSuccessHandle {add{ _onAssetSuccessCallback += value;}remove{ _onAssetSuccessCallback -= value;}}
	public event Action<AssetEvent.FailureEventArgs> AssetFailureHandle {add{ _onAssetFailureCallback += value;}remove{ _onAssetFailureCallback -= value;}}

	Dictionary<string,AssetBundle> AssetBundleDict;
	Dictionary<string,object> AssetDict;
	Dictionary<string,int> LoadingAssetBundleDict;
	Dictionary<string,int> LoadingAssetDict;

	private AssetBundleManifest _manifest;
	private XmlDocument _assetInfo;
	private Dictionary<string,List<AssetBaseInfo>> _bundleToAssetDict;

	void Awake() {
		_assetBundleTaskPool = new TaskPool<AssetBundleTask> ();
		_assetTaskPool = new TaskPool<AssetTask> ();

		TimeOut = 10f;
		AssetBundleDict = new Dictionary<string, AssetBundle> ();
		AssetDict = new Dictionary<string, object> ();
		LoadingAssetBundleDict = new Dictionary<string, int> ();
		LoadingAssetDict = new Dictionary<string,int> ();

		_manifest = null;
		_assetInfo = null;
		_bundleToAssetDict = new Dictionary<string, List<AssetBaseInfo>> ();
		ConfigPreLoaded = false;

		_onAssetBundleStartCallback = args => {
			Debug.LogWarning ("AssetBundleTask ID: " + args.SerialID.ToString() + ", Name: " + args.BundleName +" Start");
		};
		_onAssetBundleProgressCallback = args => {
			Debug.Log ("AssetBundleTask ID: " + args.SerialID.ToString() + ", Name " + args.BundleName + ", Progress " + args.Progress.ToString());
		};
		_onAssetBundleSuccessCallback = args => {
			if(!args.CachedBundle.IsNull()){
				AssetBundleDict[args.BundleName] = args.CachedBundle;
			}
			LoadingAssetBundleDict.Remove(args.BundleName);
			Debug.LogWarning("AssetBundleTask ID " + args.SerialID.ToString() + ", Success. Name: " + (args.CachedBundle.IsNull() ? "" : args.BundleName));
		};
		_onAssetBundleFailureCallback = args => {
			LoadingAssetBundleDict.Remove(args.BundleName);
			Debug.LogError("AssetBundleTask ID: " + args.SerialID.ToString() + " Failed, Bundle Nname: " + args.BundleName + ", Error: " + args.Message);	
		};

		_onAssetStartCallback = args => {
			Debug.LogWarning ("AssetTask ID: " + args.SerialID.ToString() + ", Name: " + args.AssetName + " Start");
		};
		_onAssetProgressCallback = args => {
			Debug.Log ("AssetTask ID: " + args.SerialID.ToString() + ", Name: " + args.AssetName + ", Progress " + args.Progress.ToString());
		};
		_onAssetSuccessCallback = args => {
			if(!args.Asset.IsNull()){
				AssetDict[args.AssetName] = args.Asset;
			}
			LoadingAssetDict.Remove(args.AssetName);
			Debug.LogWarning("AssetTask ID: " + args.SerialID.ToString() + " Success: " + (args.Asset.IsNull() ? "" : args.AssetName));
		};
		_onAssetFailureCallback = args => {
			LoadingAssetDict.Remove(args.AssetName);
			Debug.LogError("AssetTask ID: " + args.SerialID.ToString() + ", Name: " + args.AssetName + ", Error: " + args.Message);	
		};

		this.InitAssetInfo ();
	}

	private void InitAssetInfo(){
		this.AddAssetTask (AssetBundleUtility.GetCurrentManifestBundleName(), "AssetBundleManifest", null, args => {
			_manifest = args.Asset as AssetBundleManifest;
			ConfigPreLoaded = !_assetInfo.IsNull() && !_manifest.IsNull();
			string bname = AssetBundleUtility.GetCurrentManifestBundleName();
			AssetBundleDict[bname].Unload(false);
			AssetBundleDict.Remove(bname);
		}, args => {
			Debug.LogError("Load BundleManifeset failed!!!");
		});
		this.AddAssetTask("Config","BuildConfigs.xml", null, _args => {
			var text =  _args.Asset as TextAsset;
			_assetInfo = new XmlDocument();
			_assetInfo.LoadXml(text.text);
			InitBundleToAssetList();
			ConfigPreLoaded = !_assetInfo.IsNull() && !_manifest.IsNull();
			AssetBundleDict["Config"].Unload(false);
			AssetBundleDict.Remove("Config");
		}, msg => {
			Debug.LogError("Load AssetConfig.xml failed!!!");
		});
	}

	private void InitBundleToAssetList(){
		XmlNode root = _assetInfo.SelectSingleNode ("AssetBundleConfig");
		XmlNodeList bundles = root.SelectSingleNode ("AssetBundles").ChildNodes;
		for (int i = 0; i < bundles.Count; ++i) {
			XmlNode node = bundles [i];
			string bundleName = node.Attributes.GetNamedItem ("Name").Value;
			XmlNode variantAttr = node.Attributes.GetNamedItem ("Variant");
			string variant = variantAttr.IsNull () ? "" : "." + variantAttr.Value;
			_bundleToAssetDict.Add (bundleName + variant, new List<AssetBaseInfo> ());
		}

		XmlNodeList assets = root.SelectSingleNode ("Assets").ChildNodes;
		for (int i = 0; i < bundles.Count; ++i) {
			XmlNode node = assets [i];
			string assetName = Path.GetFileName (node.Attributes.GetNamedItem ("AssetFullPath").Value);
			string bundleName = node.Attributes.GetNamedItem ("AssetBundleName").Value;
			XmlNode variantAttr = node.Attributes.GetNamedItem ("Variant");
			string variant = variantAttr.IsNull () ? "" : "." + variantAttr.Value;
			List<AssetBaseInfo> list = null;
			AssetBaseInfo info = new AssetBaseInfo (assetName, bundleName);
			if (_bundleToAssetDict.TryGetValue (bundleName + variant, out list)) {
				list.Add (info);
			}
		}
	}

	public string GetBundleNameByAssetName(string assetName){
		if (_assetInfo.IsNull () || string.IsNullOrEmpty (assetName)) {
			return null;
		}
		foreach (var pair in _bundleToAssetDict) {
			List<AssetBaseInfo> list = pair.Value;
			for (int i = 0; i < list.Count; ++i) {
				if (list [i].AssetName.Equals (assetName)) {
					return pair.Key;
				}
			}
		}
		return null;
	}

	public string[] GetAssetListByBundleNameAndVariant(string bundleName){
		if (_assetInfo.IsNull () || string.IsNullOrEmpty (bundleName)) {
			return null;
		}
		List<AssetBaseInfo> list = _bundleToAssetDict [bundleName];
		string[] ret = new string[list.Count];
		for (int i = 0; i < ret.Length; ++i) {
			ret [i] = list [i].AssetName;
		}
		return ret;
	}

	public string[] GetScenesAssetBundleName(){
		HashSet<string> ret = new HashSet<string> ();
		foreach (var pair in _bundleToAssetDict) {
			List<AssetBaseInfo> list = pair.Value;
			for (int i = 0; i < list.Count; ++i) {
				if (list [i].IsScene) {
					ret.Add (list [i].BundleName);
				}
			}
		}
		string[] rets = new string[ret.Count];
		ret.CopyTo (rets);
		return rets;
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

	public void AddAssetBundleAgent (Action<AssetBundleEvent.StartEventArgs> startCallback){
		var agent = new AssetBundleTaskAgent ();
		agent.OnAssetBundleStartCallback = startCallback;
		agent.OnAssetBundleStartCallback += OnAssetBundleStart;
		agent.AssetBundleProgressHandle += OnAssetBundleProgress;
		agent.AssetBundleSuccessHandle += OnAssetBundleSuccess;
		agent.AssetBundleFailureHandle += OnAssetBundleFailure;

		_assetBundleTaskPool.AddAgent (agent);
	}

	public string[] GetBundleDependency(string bname){
		if (_manifest.IsNull () || string.IsNullOrEmpty(bname)) {
			return new string[0];
		}
		return _manifest.GetAllDependencies (bname);
	}

	public void AddFromFileTask (string bname,Action<AssetBundleEvent.ProgressEventArgs> progCallback = null,Action<AssetBundleEvent.SuccessEventArgs> sucCallback = null,Action<AssetBundleEvent.FailureEventArgs> failCallback = null){
		if (string.IsNullOrEmpty (bname)) {
			Debug.LogError ("Invalid Bundle Name");
			return;
		}
		var path = string.Format ("{0}{1}", AssetBundleUtility.GetCurrentBundlePath(), bname);
		AddAssetBundleTask (bname, path, true, progCallback, sucCallback, failCallback);
	}

	public void AddDownloadTask (string bname,string url,Action<AssetBundleEvent.ProgressEventArgs> progCallback = null,Action<AssetBundleEvent.SuccessEventArgs> sucCallback = null,Action<AssetBundleEvent.FailureEventArgs> failCallback = null){
		if (string.IsNullOrEmpty (bname)) {
			Debug.LogError ("Invalid Bundle Name");
			return;
		}
		AddAssetBundleTask (bname, url, false, progCallback, sucCallback, failCallback);
	}
	//TODO:Rewirte these stupid codes
	private void AddAssetBundleTask (string bname,string bpath,bool fromfile = true,Action<AssetBundleEvent.ProgressEventArgs> progCallback = null,Action<AssetBundleEvent.SuccessEventArgs> sucCallback = null,Action<AssetBundleEvent.FailureEventArgs> failCallback = null){
		Debug.Log ("Try Load " + bname);
		if (TotalAssetBundleAgentCount <= 0) {
			Debug.LogWarning ("Add a bundle agent first");
			this.AddAssetBundleAgent (null);
		}
		if (AssetBundleDict.ContainsKey (bname)) {
			Debug.LogWarning ("AssetBundle " + bname + " already Exists!");
			sucCallback (new AssetBundleEvent.SuccessEventArgs (-1, bname, true, AssetBundleDict [bname]));
			return;
		}
		if (LoadingAssetBundleDict.ContainsKey (bname)) {
			Debug.LogWarning ("AssetBundle " + bname + " is Loading! Wait Until it's done.");
			CoroutineUtils.WaitUntil (() => AssetBundleDict.ContainsKey (bname) && !LoadingAssetBundleDict.ContainsKey (bname), () => {
				sucCallback (new AssetBundleEvent.SuccessEventArgs (-1, bname, true, AssetBundleDict [bname]));
			});
			return;
		}
		string[] dependencies = GetBundleDependency (bname);
		if (dependencies.Length > 0) {
			Debug.Log ("Load " + bname + " AssetBundle's Dependencies, Count: " + dependencies.Length.ToString ());
			LoadAssetBundleDependency (dependencies, args => {
				args.Progress = args.Progress / (dependencies.Length + 1f);
				if (!progCallback.IsNull ())
					progCallback (args);
			}, args => {
				var task = new AssetBundleTask (bname, bpath, TimeOut, fromfile);
				task.AssetBundleProgressHandle += pargs=>{
					if (!progCallback.IsNull ())
						progCallback (new AssetBundleEvent.ProgressEventArgs (task.ID, bname, fromfile, (dependencies.Length + pargs.Progress) / (dependencies.Length + 1f)));
				};
				task.AssetBundleSuccessHandle += sucCallback;
				task.AssetBundleFailureHandle += failCallback;

				if (!progCallback.IsNull ())
					progCallback (new AssetBundleEvent.ProgressEventArgs (task.ID, bname, fromfile, dependencies.Length / (dependencies.Length + 1f)));

				_assetBundleTaskPool.AddTask (task);
				LoadingAssetBundleDict.Add (bname, task.ID);
			}, args => {
				if (!failCallback.IsNull ()) {
					failCallback (args);
				}
			});
		} else {
			var task = new AssetBundleTask (bname, bpath, TimeOut, fromfile);
			task.AssetBundleProgressHandle += progCallback;
			task.AssetBundleSuccessHandle += sucCallback;
			task.AssetBundleFailureHandle += failCallback;
			_assetBundleTaskPool.AddTask (task);
			LoadingAssetBundleDict.Add (bname,task.ID);	
		}
	}
	//TODO:Rewirte these stupid codes
	private void LoadAssetBundleDependency (string[] dependencies, Action<AssetBundleEvent.ProgressEventArgs> progCallback = null,Action<AssetBundleEvent.SuccessEventArgs> sucCallback = null,Action<AssetBundleEvent.FailureEventArgs> failCallback = null){
		List<float> progList = new List<float> ();
		for (int i = 0; i < dependencies.Length; ++i) {
			progList.Add (0f);
			int index = i;
			string dependencyBundle = dependencies [index];
			AddFromFileTask (dependencyBundle, args => {
				progList [index] = args.Progress;
				float progSum = 0f;
				progList.ForEach (f => {
					progSum += f;
				});
				args.Progress = progSum;
				if(!progCallback.IsNull())
					progCallback (args);
			}, args => {
				progList [index] = 1f;
				float progSum = 0f;
				progList.ForEach (f => {
					progSum += f;
				});
				if (progSum == 0f + dependencies.Length && !sucCallback.IsNull()) {
					sucCallback (args);
				}
			}, args => {
				if(!failCallback.IsNull())
					failCallback (args);
			});
		}
	}

	public void AddAssetAgent (Action<AssetEvent.StartEventArgs> startCallback){
		var agent = new AssetTaskAgent ();
		agent.OnAssetStartCallback = startCallback;
		agent.OnAssetStartCallback += OnAssetStart;
		agent.AssetProgressHandle += OnAssetProgress;
		agent.AssetSuccessHandle += OnAssetSuccess;
		agent.AssetFailureHandle += OnAssetFailure;

		_assetTaskPool.AddAgent (agent);
	}

	public void AddAssetTask (string bname, string name, Action<AssetEvent.ProgressEventArgs> progCallback = null,Action<AssetEvent.SuccessEventArgs> sucCallback = null,Action<AssetEvent.FailureEventArgs> failCallback = null){
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
				AddFromFileTask (bname, null, args => {
					AddAssetTask (args.CachedBundle, name, progCallback, sucCallback, failCallback);
				});
			}
			return;
		}
		AddAssetTask (AssetBundleDict [bname], name, progCallback, sucCallback, failCallback);
	}

	private void AddAssetTask(AssetBundle ab,string name,Action<AssetEvent.ProgressEventArgs> progCallback = null,Action<AssetEvent.SuccessEventArgs> sucCallback = null,Action<AssetEvent.FailureEventArgs> failCallback = null){
		if (TotalAssetAgentCount <= 0) {
			Debug.LogWarning ("Add an asset agent first");
			this.AddAssetAgent (null);
		}
		if (AssetDict.ContainsKey (name)) {
			Debug.LogWarning ("Asset " + name + " already Exists!");
			sucCallback (new AssetEvent.SuccessEventArgs (-1, name, AssetDict [name]));
			return;
		}
		if (LoadingAssetDict.ContainsKey (name)) {
			Debug.LogWarning ("AssetBundle " + name + " is Loading! Wait Unitl it's Done.");
			CoroutineUtils.WaitUntil (() => AssetDict.ContainsKey (name) && !LoadingAssetDict.ContainsKey (name), () => {
				sucCallback (new AssetEvent.SuccessEventArgs (-1, name, AssetDict [name]));
			});
			return;
		}

		var task = new AssetTask (name, TimeOut, ab);
		task.AssetProgressHandle += progCallback;
		task.AssetSuccessHandle += sucCallback;
		task.AssetFailureHandle += failCallback;
		_assetTaskPool.AddTask (task);
		LoadingAssetDict.Add (name,task.ID);
	}

	public void AddAllAssetsTask(string bname, Action<AssetEvent.ProgressEventArgs> progCallback, Action<UnityEngine.Object[]> sucCallback, Action<AssetEvent.FailureEventArgs> failCallback){
		var coroutineInst = CoroutineManager.Instance;
		if (!AssetBundleDict.ContainsKey (bname)) {
			Debug.LogWarning ("AssetBundle " + bname + " hasn't loaded yet!Load the bundle first.");
			if (!IsAssetBundleLoading (bname)) {
				CoroutineUtils.WaitUntil (() => this.HasAssetBundle (bname), () => {
					coroutineInst.StartNewCoroutineTask (loadAllAssetsAsyn (bname, sucCallback));
				});
			} else {
				AddFromFileTask (bname, null, args => {
					coroutineInst.StartNewCoroutineTask (loadAllAssetsAsyn (bname, sucCallback));
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
		if (!abr.allAssets.IsNull ()) {
			string[] assetList = GetAssetListByBundleNameAndVariant (bname);
			for (int i = 0; i < assetList.Length; ++i) {
				LoadingAssetDict.Remove (assetList [i]);
			}
			callback (abr.allAssets);
		}
	}

	public void UnloadAssetBundle(string bname,bool unloadAllLoadedAsset = true, bool unloadDependencies = false){
		if (!AssetBundleDict.ContainsKey (bname) || AssetBundleDict [bname].IsNull()) {
			return;
		}
		var bundle = AssetBundleDict [bname];
		AssetBundleDict.Remove (bname);
		string[] assetList = GetAssetListByBundleNameAndVariant (bname);
		for (int i = 0; i < assetList.Length; ++i) {
			AssetDict.Remove (assetList [i]);
		}
		bundle.Unload (unloadAllLoadedAsset);
		if (unloadDependencies) {
			string[] dependencies = GetBundleDependency (bname);
			for (int i = 0; i < dependencies.Length; ++i) {
				UnloadAssetBundle (dependencies [i], unloadAllLoadedAsset, unloadDependencies);
			}
		}
	}

	public void UnloadAllAssetBundle(bool unloadAllLoadedAsset = true){
		if (AssetBundleDict.Count == 0) {
			return;
		}
		string[] keys = new string[AssetBundleDict.Keys.Count];
		AssetBundleDict.Keys.CopyTo (keys, 0);
		for (int i = 0; i < keys.Length; ++i) {
			if (keys[i] != "Config") {
				UnloadAssetBundle (keys[i], unloadAllLoadedAsset);
			}
		}
		UnloadAssetBundle ("Config", unloadAllLoadedAsset);
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

	public void OnAssetBundleStart(AssetBundleEvent.StartEventArgs args){
		if (!_onAssetBundleStartCallback.IsNull ()) {
			_onAssetBundleStartCallback (args);
		}
		EventPoolManager.Instance.TriggerEvent (this, args);
	}

	public void OnAssetBundleProgress(AssetBundleEvent.ProgressEventArgs args){
		if (!_onAssetBundleProgressCallback.IsNull ()) {
			_onAssetBundleProgressCallback (args);
		}
		EventPoolManager.Instance.TriggerEvent (this, args);
	}

	public void OnAssetBundleSuccess(AssetBundleEvent.SuccessEventArgs args){
		if (!_onAssetBundleSuccessCallback.IsNull ()) {
			_onAssetBundleSuccessCallback (args);
		}
		EventPoolManager.Instance.TriggerEvent (this, args);
	}

	public void OnAssetBundleFailure(AssetBundleEvent.FailureEventArgs args){
		if (!_onAssetBundleFailureCallback.IsNull ()) {
			_onAssetBundleFailureCallback (args);
		}
		EventPoolManager.Instance.TriggerEvent (this, args);
	}

	public void UnloadAsset(object asset){
		string assetName = null;
		foreach (var pair in AssetDict) {
			if (pair.Value.GetHashCode () == asset.GetHashCode ()) {
				assetName = pair.Key;
				break;
			}
		}
		if (!assetName.IsNull()) {
			AssetDict.Remove (assetName);
//			Destroy (asset as UnityEngine.Object);
		}
	}

	public void UnloadAsset(string aname){
		if (!AssetDict.ContainsKey (aname) || AssetDict [aname].IsNull()) {
			return;
		}
		var asset = AssetDict [aname];
		AssetDict.Remove (aname);
//		UnityEngine.Object.Destroy (asset);
	}

	public void UnloadAllAsset(){
		if (AssetDict.Count == 0) {
			return;
		}
//		foreach (var item in AssetDict) {
//			UnityEngine.Object.Destroy (item.Value);
//		}
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

	public void OnAssetStart(AssetEvent.StartEventArgs args){
		if (!_onAssetStartCallback.IsNull ()) {
			_onAssetStartCallback (args);
		}
		EventPoolManager.Instance.TriggerEvent (this, args);
	}

	public void OnAssetProgress(AssetEvent.ProgressEventArgs args){
		if (!_onAssetProgressCallback.IsNull ()) {
			_onAssetProgressCallback (args);
		}
		EventPoolManager.Instance.TriggerEvent (this, args);
	}

	public void OnAssetSuccess(AssetEvent.SuccessEventArgs args){
		if (!_onAssetSuccessCallback.IsNull ()) {
			_onAssetSuccessCallback (args);
		}
		EventPoolManager.Instance.TriggerEvent (this, args);
	}

	public void OnAssetFailure(AssetEvent.FailureEventArgs args){
		if (!_onAssetFailureCallback.IsNull ()) {
			_onAssetFailureCallback (args);
		}
		EventPoolManager.Instance.TriggerEvent (this, args);
	}

	public void CloseManager(){
		_assetTaskPool.ClearPool ();
		_assetBundleTaskPool.ClearPool ();
	}

	protected override void OnDestroy(){
		this.UnloadAllAsset ();
		this.UnloadAllAssetBundle ();
		_manifest = null;
		_assetInfo = null;
		base.OnDestroy ();
	}

}
