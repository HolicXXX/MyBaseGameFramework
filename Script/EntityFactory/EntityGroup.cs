using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EntityGroup {

	public string GroupName{ get; private set; }

	public int GroupAssetsCount{ get { return _entityDict.Count; } }

	public bool IsComplete{ get; private set; }

	string[] _assetPath;

	Dictionary<string,EntityInfo> _entityDict;

	public EntityGroup(string gn,string[] ap){
		GroupName = gn;
		_assetPath = ap;
		_entityDict = new Dictionary<string, EntityInfo> ();
		IsComplete = false;
	}

	public IEnumerator LoadGroup(){
		if (GroupAssetsCount != 0)
			yield break;
		for (int i = 0; i < _assetPath.Length; ++i) {
			if (_entityDict.ContainsKey (_assetPath [i]))
				continue;
			yield return AssetBundleManager.Instance.LoadAssetAsyn (GroupName, _assetPath [i], obj => {
				GameObject go = obj as GameObject;
				_entityDict.Add(go.name,new EntityInfo(go.name,GroupName,go));
			});
		}
		IsComplete = true;
		Debug.Log ($"Group load is complete: {IsComplete}");
	}

	public void ClearGroup(){
		foreach (var ei in _entityDict.Values) {
			ei.ReleaseInfo ();
		}
		_entityDict.Clear ();
	}

	public GameObject GetEntityGameObject(string name){
		if (_entityDict.ContainsKey (name)) {
			return _entityDict [name].Entity;
		}
		return null;
	}
}
