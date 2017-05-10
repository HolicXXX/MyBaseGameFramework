using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Unity GameObject item in pool.
/// </summary>
public class UPollItem {

	GameObject _gameObject;
	public bool IsActive{ get; private set; }

	public UPollItem(GameObject go){
		_gameObject = go;
		IsActive = _gameObject.activeSelf;
	}

	/// <summary>
	/// Active the gameobject.
	/// </summary>
	/// <returns>gameobject</returns>
	public GameObject Active(){
		if (_gameObject == null)
			return null;
		_gameObject.SetActive (true);
		IsActive = true;
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
		IsActive = false;
	}

	/// <summary>
	/// Destroy the gameobject forever.
	/// </summary>
	public void Destroy(){
		if (_gameObject == null)
			return;
		GameObject.Destroy (_gameObject);
		IsActive = false;
		_gameObject = null;
	}

}
