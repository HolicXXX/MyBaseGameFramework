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
	public IEnumerator LoadAssetBundleAsyn(string bname) {
		if (AssetBundleDict.ContainsKey (bname) && AssetBundleDict [bname] != null) {
			yield break;
		}
		string fullpath = $"{Application.streamingAssetsPath}/{bname}";
		var abcr = AssetBundle.LoadFromFileAsync (fullpath);
		yield return abcr;
		AssetBundleDict [bname] = abcr.assetBundle ?? null;
	}
	/// <summary>
	/// Loads one asset in the bundle asyn.
	/// </summary>
	/// <param name="bname">Target Bundle name.</param>
	/// <param name="aname">Asset name.</param>
	/// <param name="callback">Callback.</param>
	public IEnumerator LoadAssetAsyn(string bname, string aname, Action<UnityEngine.Object> callback) {
		if (!AssetBundleDict.ContainsKey (bname) || AssetBundleDict [bname] == null) {
			yield return LoadAssetBundleAsyn (bname);
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
	public IEnumerator LoadAllAssetsAsyn (string bname, Action<UnityEngine.Object[]> callback) {
		if (!AssetBundleDict.ContainsKey (bname) || AssetBundleDict [bname] == null) {
			yield return LoadAssetBundleAsyn (bname);
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
		if (!AssetBundleDict.ContainsKey (bname) || AssetBundleDict [bname] == null) {
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
