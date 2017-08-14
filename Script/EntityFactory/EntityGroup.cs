using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Entity group.
/// </summary>
public class EntityGroup {

	public string GroupName{ get; private set; }

	public int GroupAssetsCount{ get { return _entityDict.Count; } }

	/// <summary>
	/// whether this Group is complete.
	/// </summary>
	/// <value><c>true</c> if this group is complete; otherwise, <c>false</c>.</value>
	public bool IsComplete{ get; private set; }

	/// <summary>
	/// The assets' path that this group contains.
	/// </summary>
	string[] _assetPath;

	Dictionary<string,EntityInfo> _entityDict;

	/// <summary>
	/// Initializes a new instance of the <see cref="EntityGroup"/> class.
	/// </summary>
	/// <param name="gn">Group nname.</param>
	/// <param name="ap">Assets path.</param>
	public EntityGroup(string gn,string[] ap){
		GroupName = gn;
		_assetPath = ap;
		_entityDict = new Dictionary<string, EntityInfo> ();
		IsComplete = false;
	}

	/// <summary>
	/// Load the group.
	/// </summary>
	/// <param name="cb">Callback.</param>
	public void LoadGroup(Action cb = null){
		if (GroupAssetsCount != 0)
			return;
		for (int i = 0; i < _assetPath.Length; ++i) {
			if (_entityDict.ContainsKey (_assetPath [i]))
				continue;
			AssetBundleManager.Instance.AddAssetTask (GroupName, _assetPath [i], null, obj => {
				GameObject go = obj as GameObject;
				_entityDict.Add (go.name, new EntityInfo (go.name, GroupName, go));
				if (_entityDict.Count == _assetPath.Length) {
					IsComplete = true;
					if (!cb.IsNull ())
						cb ();
				}
			},null);

		}
	}

	/// <summary>
	/// Clear the group.
	/// </summary>
	public void ClearGroup(){
		foreach (var ei in _entityDict.Values) {
			ei.ReleaseInfo ();
		}
		_entityDict.Clear ();
	}

	/// <summary>
	/// Get the game object entity.
	/// </summary>
	/// <returns>The game object entity.</returns>
	/// <param name="name">Entity Name.</param>
	public GameObject GetEntityGameObject(string name){
		if (_entityDict.ContainsKey (name)) {
			return _entityDict [name].Entity;
		}
		return null;
	}
}
