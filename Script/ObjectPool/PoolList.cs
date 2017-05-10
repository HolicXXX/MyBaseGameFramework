using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Pool list.This is for custom class not Unity's
/// </summary>
public class PoolList<T> where T : CustomInterfaces.IResetable, new() {

	Queue<T> _objList;

	public PoolList(int capacity = 0){
		_objList = new Queue<T> (capacity);
	}

	public T GetObj(){
		T obj;
		if (_objList.Count > 0) {
			obj = _objList.Dequeue ();
			obj.Reset ();
		} else {
			obj = new T ();
			obj.InitOnce ();
		}
		return obj;
	}

	public void ReturnObj(T obj){
		_objList.Enqueue (obj);
	}

	public void ClearPool(){
		_objList.Clear ();
	}
}
