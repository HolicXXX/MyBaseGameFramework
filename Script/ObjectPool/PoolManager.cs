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

	int _releaseCount = 0;

	float _overTimeDelta = 0f;

	void Update(){
		++_releaseCount;
		_overTimeDelta += Time.deltaTime;
		if (_releaseCount > 10) {
			foreach (var list in _uPoolList.Values) {
				list.DestroyOverTimeObj (_overTimeDelta);
			}
			_releaseCount = 0;
			_overTimeDelta = 0f;
		}
	}

	public UPoolList GetUPoolList(string listName,float overTime = 3f){
		UPoolList ret = null;
		if (!_uPoolList.ContainsKey (listName)) {
			ret = new UPoolList (listName, overTime, EntityFactory.Instance.GetInstantiateFunc (listName));
			_uPoolList [listName] = ret;
		} else {
			ret = _uPoolList [listName];
		}
		return ret;
	}

	public PoolList<T> GetPoolList<T>(string listName, int capacity = 10) where T : CustomInterfaces.IResetable,new() {
		PoolList<T> ret = null;
		if (!_poolList.ContainsKey (listName)) {
			ret = new PoolList<T> (capacity);
			_poolList [listName] = ret;
		} else {
			ret = _poolList [listName] as PoolList<T>;
		}
		return ret;
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
			var func = EntityFactory.Instance.GetInstantiateFunc (listName);
			var list = new UPoolList (listName,3f,func);
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
			var func = EntityFactory.Instance.GetInstantiateFunc (listName);
			list = new UPoolList (listName,3f,func);
			_uPoolList [listName] = list;
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
	public T GetObj<T>(string listName, object userData = null) where T : CustomInterfaces.IResetable, new(){
		T ret;
		PoolList<T> list;
		if (_poolList.ContainsKey (listName)) {
			list = _poolList [listName] as PoolList<T>;
		} else {
			list = new PoolList<T> ();
			_poolList.Add (listName, list);
		}
		ret = list.GetObj (userData);
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

	protected override void OnDestroy(){
		base.OnDestroy ();
		ClearPool (false);
		ClearPool ();
	}
}
