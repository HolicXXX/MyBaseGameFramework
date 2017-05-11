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
		InitConfigOnce ();
	}

	public void InitConfigOnce(){
		StartCoroutine (LoadConfigFile ());
	}

	public void LoadGroup (string groupName){
		if (!_entityDict.ContainsKey (groupName) || _entityDict [groupName].GroupAssetsCount > 0)
			return;
		StartCoroutine (_entityDict [groupName].LoadGroup ());
	}

	public bool IsGroupComplete(string groupName){
		bool ret = false;
		if (_entityDict.ContainsKey (groupName) && _entityDict [groupName].IsComplete)
			ret = true;
		return ret;
	}

	public void UnloadGroup(string groupName){
		if (!_entityDict.ContainsKey (groupName) || _entityDict [groupName].GroupAssetsCount > 0)
			return;
		_entityDict [groupName].ClearGroup ();
	}

	IEnumerator LoadConfigFile(){
		yield return AssetBundleManager.Instance.LoadAssetAsyn ("config", "Assets/Script/Config/EntityConfig.json", obj => {
			var asset = obj as TextAsset;
			string jstr = asset.text;
			_config = JsonUtils.ReadJsonString(jstr);
		});
		for (int i = 0; i < _config.Count; ++i) {
			string gn = _config [i] ["GroupName"].ToString ();
			string[] assets = new string[_config [i] ["assets"].Count];
			for (int j = 0; j < assets.Length; ++j) {
				assets [j] = _config [i] ["assets"] [j] ["path"].ToString ();
			}
			_entityDict.Add (gn, new EntityGroup (gn, assets));
		}
	}

	public Func<GameObject> GetInstantiateFunc(string objname){
		foreach (var grp in _entityDict.Values) {
			var go = grp.GetEntityGameObject(objname);
			if(go != null){
				return ()=>{
					return GameObject.Instantiate(go,null,false);
				};
			}
		}
		return null;
	}
}
