using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PoolManager : Singleton<PoolManager> {

	Dictionary<string,UPoolList> _uPoolList;

	Hashtable _poolList;

	void Awake(){
		_poolList = new Hashtable();
		_uPoolList = new Dictionary<string, UPoolList> ();
	}

	void Start(){
	}
	/// <summary>
	/// Return a gameobject.
	/// </summary>
	/// <param name="listName">List name.</param>
	/// <param name="obj">gameobject.</param>
	public void ReturnObj(string listName,GameObject obj){
		if (_uPoolList.ContainsKey (listName)) {
			_uPoolList [listName].ReturnObj (obj);
		} else {
			var list = new UPoolList (listName);
			list.ReturnObj (obj);
			_uPoolList.Add (listName, list);
		}
	}

	/// <summary>
	/// Return a custom object.
	/// </summary>
	/// <param name="listName">List name.</param>
	/// <param name="obj">object.</param>
	/// <typeparam name="T">type parameter.</typeparam>
	public void ReturnObj<T>(string listName,T obj) where T : CustomInterfaces.IResetable, new(){
		if (_poolList.ContainsKey (listName)) {
			var list = _poolList [listName] as PoolList<T>;
			list.ReturnObj (obj);
		} else {
			var list = new PoolList<T>();
			list.ReturnObj (obj);
			_poolList.Add (listName, list);
		}
	}

	/// <summary>
	/// Get a gameobject.
	/// </summary>
	/// <returns>The gameobject.</returns>
	/// <param name="listName">List name.</param>
	/// <param name="parent">Parent transform.</param>
	/// <param name="localPos">Local position.</param>
	/// <param name="localEulerAngles">Local euler angles.</param>
	/// <param name="localScale">Local scale.</param>
	public GameObject GetObj(string listName,Transform parent = null,Vector3 localPos = new Vector3(),Vector3 localEulerAngles = new Vector3(),Vector3? localScale = null){
		UPoolList list;
		GameObject go = null;
		if (_uPoolList.ContainsKey (listName)) {
			list = _uPoolList [listName];
		} else {
			list = new UPoolList (listName);
		}
		go = list.GetObj (parent,localPos,localEulerAngles,localScale);
		return go;
	}

	/// <summary>
	/// Get a custom object.
	/// </summary>
	/// <returns>The custom object.</returns>
	/// <param name="listName">List name.</param>
	/// <typeparam name="T">type parameter.</typeparam>
	public T GetObj<T>(string listName) where T : CustomInterfaces.IResetable, new(){
		T ret;
		PoolList<T> list;
		if (_poolList.ContainsKey (listName)) {
			list = _poolList [listName] as PoolList<T>;
		} else {
			list = new PoolList<T> ();
			_poolList.Add (listName, list);
		}
		ret = list.GetObj ();
		return ret;
	}

	/// <summary>
	/// Remove a gameobject list.
	/// </summary>
	/// <param name="listName">List name.</param>
	/// <param name="destroy">If set to true then destroy all gameobjects that the list contains.</param>
	public void RemoveList(string listName, bool destroy){
		if (!_uPoolList.ContainsKey (listName))
			return;
		var list = _uPoolList [listName];
		list.ClearPool (destroy);
		_uPoolList.Remove (listName);
	}

	/// <summary>
	/// Removes a custom object list.
	/// </summary>
	/// <param name="listName">List name.</param>
	/// <typeparam name="T">type parameter.</typeparam>
	public void RemoveList<T>(string listName) where T : CustomInterfaces.IResetable, new(){
		if (_poolList.ContainsKey (listName))
			return;
		var list = _poolList [listName] as PoolList<T>;
		list.ClearPool ();
		_poolList.Remove (listName);
	}

	/// <summary>
	/// Clear the gameobject pool.
	/// </summary>
	/// <param name="destroy">If set to true the destroy all gameobjects that UPollList contains.</param>
	public void ClearPool(bool destroy){
		foreach (var list in _uPoolList.Values) {
			list.ClearPool (destroy);
		}
		_uPoolList.Clear ();
	}

	/// <summary>
	/// Clear the custom pool.
	/// </summary>
	public void ClearPool() {
		_poolList.Clear ();
	}

	void OnDestroy(){
		base.OnDestroy ();
		ClearPool (false);
		ClearPool ();
	}
}
