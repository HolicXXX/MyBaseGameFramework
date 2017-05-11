using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Unity GameObject item in pool.
/// </summary>
public class UPollItem {

	GameObject _gameObject;
	public bool IsUsing{ get; private set; }

	public float OverTime {
		get;
		set;
	}

	public UPollItem(GameObject go){
		_gameObject = go;
		IsUsing = false;
		OverTime = 0f;
	}

	/// <summary>
	/// Active the gameobject.
	/// </summary>
	/// <returns>gameobject</returns>
	public GameObject Active(){
		if (_gameObject == null)
			return null;
		_gameObject.SetActive (true);
		IsUsing = true;
		return _gameObject;
	}

	/// <summary>
	/// Freeze the gameobject, not destroy.
	/// </summary>
	public void Freeze(){
		if (_gameObject == null)
			return;
		_gameObject.transform.parent = null;
		_gameObject.SetActive (false);
		IsUsing = false;
		OverTime = 0f;
	}

	/// <summary>
	/// Destroy the gameobject forever.
	/// </summary>
	public void Destroy(){
		if (_gameObject == null)
			return;
		GameObject.Destroy (_gameObject);
		IsUsing = false;
		_gameObject = null;
	}

}
