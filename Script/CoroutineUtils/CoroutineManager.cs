using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CoroutineManager : Singleton<CoroutineManager> {

	private Dictionary<int,CoroutineTask> _coroutineDict;

	public int CoroutineCount{ get { return _coroutineDict.Count; } }

	void Awake () {
		_coroutineDict = new Dictionary<int, CoroutineTask> ();
	}

	public bool IsCoroutineTaskDone (int id){
		CoroutineTask task = null;
		return _coroutineDict.TryGetValue (id, out task) && task.Done;
	}

	public bool HasCoroutineTask (int id){
		return  _coroutineDict.ContainsKey (id);
	}

	private void OnCoroutineFineshedHandle (int id){
		#if DEBUG
		Debug.LogFormat("Coroutine {0} Finished",id);
		#endif
		StopCoroutineTask (id);
	}

	public int StartNewCoroutineTask (IEnumerator coroutine,Action<int> callback = null){
		CoroutineTask task = new CoroutineTask (coroutine);
		if (!callback.IsNull ())
			task.FineshedHandle += callback;
		task.FineshedHandle += this.OnCoroutineFineshedHandle;
		task.SavedCoroutien = StartCoroutine (task.Start ());
		_coroutineDict [task.ID] = task;
		return task.ID;
	}

	public bool PauseCoroutineTask (int id){
		CoroutineTask task = null;
		if (_coroutineDict.TryGetValue (id, out task)) {
			task.Paused = true;
			return true;
		}
		return false;
	}

	public bool ResumeCoroutineTask (int id){
		CoroutineTask task = null;
		if (_coroutineDict.TryGetValue (id, out task)) {
			task.Paused = false;
			return true;
		}
		return false;
	}

	public bool StopCoroutineTask (int id){
		CoroutineTask task = null;
		if (_coroutineDict.TryGetValue (id, out task)) {
			task.FineshedHandle -= OnCoroutineFineshedHandle;
			StopCoroutine (task.SavedCoroutien);
			task.Dispose ();
			_coroutineDict.Remove (id);
			return true;
		}
		return false;
	}

	public CoroutineTask GetCoroutineTask (int id){
		CoroutineTask task = null;
		_coroutineDict.TryGetValue (id, out task);
		return task;
	}

	public void StopAllCoroutineTasks (){
		foreach (var key in _coroutineDict.Keys) {
			StopCoroutineTask (key);
		}
		_coroutineDict.Clear ();
	}

	// Update is called once per frame
	void Update () {
	}
}
