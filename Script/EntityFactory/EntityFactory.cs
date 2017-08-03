using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using LitJson;

public class EntityFactory : Singleton<EntityFactory> {

	Dictionary<string,EntityGroup> _entityDict;

	JsonData _config;

	void Awake() {
		_entityDict = new Dictionary<string, EntityGroup> ();
	}

	void Start () {
		//TODO: use json or xml to save bundle name and path
		InitConfigOnce ("config", "Assets/Script/Config/EntityConfig.json");
	}

	/// <summary>
	/// Init the config once ("config" is a file that records all bundle information).
	/// </summary>
	/// <param name="bundleName">config's Bundle name.</param>
	/// <param name="fullname">config's Fullname in bundle.</param>
	void InitConfigOnce(string bundleName,string fullname){
		Func<int> assetFunc = ()=>AssetBundleManager.Instance.AddAssetTask (bundleName, fullname, obj => {
			var asset = obj as TextAsset;
			string jstr = asset.text;
			_config = JsonUtils.ReadJsonString (jstr);

			for (int i = 0; i < _config.Count; ++i) {
				string gn = _config [i] ["GroupName"].ToString ();
				string[] assets = new string[_config [i] ["assets"].Count];
				for (int j = 0; j < assets.Length; ++j) {
					assets [j] = _config [i] ["assets"] [j] ["path"].ToString ();
				}
				_entityDict.Add (gn, new EntityGroup (gn, assets));
			}
		});
		if (!AssetBundleManager.Instance.HasAssetBundle (bundleName)) {
			AssetBundleManager.Instance.AddFromFileTask (bundleName, null, ab => {
				assetFunc ();
			},msg=>{
				Debug.LogError("Init EntityConfig Bundle: " + bundleName + " Failed : " + msg);
			});
		} else {
			assetFunc ();
		}
	}

	/// <summary>
	/// Load the group.
	/// </summary>
	/// <param name="groupName">Group name.</param>
	public void LoadGroup (string groupName){
		if (!_entityDict.ContainsKey (groupName) || _entityDict [groupName].GroupAssetsCount > 0)
			return;
		_entityDict [groupName].LoadGroup ();
	}

	/// <summary>
	/// Load the group with a callback.
	/// </summary>
	/// <param name="groupName">Group name.</param>
	/// <param name="cb">Callback.</param>
	public void LoadGroup(string groupName,Action cb = null){
		StartCoroutine (LoadGroupWithCb (groupName, cb));
	}

	IEnumerator LoadGroupWithCb(string groupName,Action cb){
		if (!_entityDict.ContainsKey (groupName) || _entityDict [groupName].GroupAssetsCount > 0)
			yield break;
		_entityDict [groupName].LoadGroup (cb);
	}

	/// <summary>
	/// Determines whether a group is complete with the specified groupName.
	/// </summary>
	/// <returns><c>true</c> if this group complete with the specified groupName; otherwise, <c>false</c>.</returns>
	/// <param name="groupName">Group name.</param>
	public bool IsGroupComplete(string groupName){
		bool ret = false;
		if (_entityDict.ContainsKey (groupName) && _entityDict [groupName].IsComplete)
			ret = true;
		return ret;
	}

	/// <summary>
	/// Unload the group with the specified groupName.
	/// </summary>
	/// <param name="groupName">Group name.</param>
	public void UnloadGroup(string groupName){
		if (!_entityDict.ContainsKey (groupName) || _entityDict [groupName].GroupAssetsCount > 0)
			return;
		_entityDict [groupName].ClearGroup ();
	}

	/// <summary>
	/// Get the instantiate func for a gameobject entity with it's name.
	/// </summary>
	/// <returns>The instantiate func.</returns>
	/// <param name="objname">GameObject name.</param>
	public Func<GameObject> GetInstantiateFunc(string objname){
		foreach (var grp in _entityDict.Values) {
			var go = grp.GetEntityGameObject(objname);
			if(!go.IsNull()){
				return ()=>{
					return GameObject.Instantiate(go,null,false);
				};
			}
		}
		return null;
	}
}
