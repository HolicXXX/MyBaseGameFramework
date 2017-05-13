using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AssetBundleManager : Singleton<AssetBundleManager> {

	Dictionary<string,AssetBundle> AssetBundleDict;

	void Awake() {
		AssetBundleDict = new Dictionary<string, AssetBundle> ();
	}
	void Start () {
	}
	protected override void OnDestroy(){
		this.UnloadAllAssetBundle ();
		base.OnDestroy ();
	}

	/// <summary>
	/// Loads the assetbundle asyn.
	/// </summary>
	/// <param name="bname">Bundle name.</param>
	public void LoadAssetBundleAsyn(string bname,Action cb = null){
		StartCoroutine (loadAssetBundleAsyn (bname, cb));
	}

	IEnumerator loadAssetBundleAsyn(string bname,Action cb) {
		if (AssetBundleDict.ContainsKey (bname) && !AssetBundleDict [bname].IsNull()) {
			yield break;
		}
		string fullpath = $"{Application.streamingAssetsPath}/{bname}";
		var abcr = AssetBundle.LoadFromFileAsync (fullpath);
		yield return abcr;
		AssetBundleDict [bname] = abcr.assetBundle ?? null;
		if (!cb.IsNull())
			cb ();
	}

	/// <summary>
	/// Loads one asset in the bundle asyn.
	/// </summary>
	/// <param name="bname">Target Bundle name.</param>
	/// <param name="aname">Asset name.</param>
	/// <param name="callback">Callback.</param>
	public void LoadAssetAsyn(string bname, string aname, Action<UnityEngine.Object> callback){
		StartCoroutine (loadAssetAsyn (bname, aname, callback));
	}
	IEnumerator loadAssetAsyn(string bname, string aname, Action<UnityEngine.Object> callback) {
		if (!AssetBundleDict.ContainsKey (bname) || AssetBundleDict [bname].IsNull()) {
			yield return loadAssetBundleAsyn (bname,null);
		}
		var bundle = AssetBundleDict [bname];
		var abr = bundle.LoadAssetAsync (aname);
		yield return abr;
		callback (abr.asset ?? null);
	}

	/// <summary>
	/// Loads all assets in the bundle asyn.
	/// </summary>
	/// <param name="bname">Target Bundle name.</param>
	/// <param name="callback">Callback receive result</param>
	public void LoadAllAssetsAsyn(string bname, Action<UnityEngine.Object[]> callback){
		StartCoroutine (loadAllAssetsAsyn (bname, callback));
	}
	IEnumerator loadAllAssetsAsyn (string bname, Action<UnityEngine.Object[]> callback) {
		if (!AssetBundleDict.ContainsKey (bname) || AssetBundleDict [bname].IsNull()) {
			yield return loadAssetBundleAsyn (bname,null);
		}
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
}
