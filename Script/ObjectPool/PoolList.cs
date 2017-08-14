using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Pool list.This is for custom class not Unity's
/// </summary>
public class PoolList<T> where T : CustomInterfaces.IResetable, new() {
	
	Queue<T> _objList;
	Dictionary<int,T> _outerObjDict;

	private int _poolCapacity;
	public int PoolCapacity{
		get{ return _poolCapacity; } 
		set{ 
			_poolCapacity = value;
			while (_objList.Count > _poolCapacity) {
				var obj = _objList.Dequeue ();
				obj.Destroy ();
			}
		}
	}

	public int PoolOuterObjectCount{
		get{ 
			return _outerObjDict.Count;
		}
	}

	public PoolList(int capacity = 10){
		PoolCapacity = capacity;
		_objList = new Queue<T> ();
		_outerObjDict = new Dictionary<int, T> ();
	}

	public T GetObj(object userData = null){
		T obj;
		if (_objList.Count > 0) {
			obj = _objList.Dequeue ();
			obj.Reset (userData);
		} else {
			obj = new T ();
			obj.InitOnce (userData);
		}
		_outerObjDict.Add (obj.Instance.GetHashCode (), obj);
		return obj;
	}

	public void ReturnObj(T obj){
		if (_objList.Count < PoolCapacity) {
			_objList.Enqueue (obj);	
		} else {
			obj.Destroy ();
		}
		int hash = obj.Instance.GetHashCode ();
		if (_outerObjDict.ContainsKey (hash)) {
			_outerObjDict.Remove (hash);
		}
	}

	public void ClearPool(){
		foreach (T obj in _objList) {
			obj.Destroy ();
		}
		_objList.Clear ();
	}
}
