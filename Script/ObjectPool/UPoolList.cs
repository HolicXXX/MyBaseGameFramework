using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Unity pool list, only use for GameObject.
/// </summary>
public class UPoolList {

	string _listName;
	public string ListName{
		get{ return _listName;}
	}

	/// <summary>
	/// The item dictionary, key:gameobject's hashcode, value:gameobject's UpollItem.
	/// </summary>
	Dictionary<int,UPollItem> _itemDict;

	Func<GameObject> _instantiateFunc;

	float _overTime;

	public UPoolList(string name,float ot,Func<GameObject> instFunc = null) {
		_itemDict = new Dictionary<int, UPollItem> ();
		_listName = name;
		_overTime = ot;
		_instantiateFunc = instFunc;
	}

	/// <summary>
	/// Get a freezed gameobject or a new one.
	/// </summary>
	/// <param name="parent"></param>
	/// <param name="localPos"></param>
	/// <param name="localEulerAngles"></param>
	/// <param name="localScale"></param>
	/// <returns>The gameobject.</returns>
	public GameObject GetObj(Transform parent,Vector3 localPos,Vector3 localEulerAngles,Vector3? localScale){
		GameObject go = null;
		foreach (var item in _itemDict.Values) {
			if (!item.IsUsing) {
				go = item.Active ();
				break;
			}
		}
		if (go == null) {
			if (_instantiateFunc != null) {
				go = _instantiateFunc ();
				var key = go.GetHashCode ();
				var item = new UPollItem (go);
				go = item.Active ();
				_itemDict [key] = item;
			}
		}

		if (go != null) {
			go.transform.parent = parent;
			go.transform.localPosition = localPos;
			go.transform.localEulerAngles = localEulerAngles;
			go.transform.localScale = localScale ?? Vector3.one;
		}
		return go;
	}

	/// <summary>
	/// Return a gameobject,use it's hashcode for key.
	/// </summary>
	/// <param name="go">gameobject.</param>
	public void ReturnObj(GameObject go){
		var key = go.GetHashCode ();
		if (_itemDict.ContainsKey (key)) {
			_itemDict [key].Freeze ();
		} else {
			var item = new UPollItem (go);
			item.Freeze ();
			_itemDict [key] = item;
		}
	}

	/// <summary>
	/// Destroy a gameobject and remove it from list(if it was there).
	/// </summary>
	/// <param name="go">gameobject.</param>
	public void DestroyObj(GameObject go){
		var key = go.GetHashCode ();
		if (_itemDict.ContainsKey (key)) {
			var item = _itemDict [key];
			_itemDict.Remove (key);
			item.Destroy ();
		} else {
			GameObject.Destroy (go);
		}
	}

	/// <summary>
	/// Clear the pool.
	/// </summary>
	/// <param name="destroy">If true then destroy all gameobject.</param>
	public void ClearPool(bool destroy = true){
		if (destroy) {
			foreach (var item in _itemDict.Values) {
				item.Destroy ();
			}
		}
		_instantiateFunc = null;
		_itemDict.Clear ();
	}

	/// <summary>
	/// Destroiy the over time gameobject.
	/// </summary>
	/// <param name="dt">Deltatime.</param>
	public void DestroyOverTimeObj(float dt) {
		List<int> removeList = new List<int> ();
		foreach (var pair in _itemDict) {
			if (!pair.Value.IsUsing) {
				pair.Value.OverTime += dt;
			}
			if (pair.Value.OverTime > _overTime) {
				removeList.Add (pair.Key);
			}
		}
		for (int i = 0; i < removeList.Count; ++i) {
			var item = _itemDict [removeList[i]];
			_itemDict.Remove (removeList[i]);
			item.Destroy ();
		}
	}
}
