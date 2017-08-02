using System;
using System.Collections;
using UnityEngine;

public class CoroutineTask : ITask {

	public static int TaskSerialID = 0;

	public int ID{ get; set; }
	public bool Done{ get; set; }
	public TaskStatus Status{ get; set; }

	public IEnumerator TargetCoroutine{ get; set; }
	public Coroutine SavedCoroutien{ get; set; }
	public bool Paused{ get; set; }
	private Action<int> _finishedCallback;
	public event Action<int> FineshedHandle
	{
		add{
			_finishedCallback += value;
		}
		remove{
			_finishedCallback -= value;
		}
	}

	public CoroutineTask(IEnumerator coroutine){
		ID = CoroutineTask.TaskSerialID++;
		Done = false;
		Status = TaskStatus.TS_TODO;

		TargetCoroutine = coroutine;
		SavedCoroutien = null;
		_finishedCallback = null;
	}

	public IEnumerator Start(){
		yield return null;//make sure it's next frame
		while(!Done) {
			if(Paused)
				yield return null;
			else {
				if(TargetCoroutine != null && TargetCoroutine.MoveNext()) {
					yield return TargetCoroutine.Current;
				}
				else {
					Done = true;
					Status = TaskStatus.TS_DONE;
				}
			}
		}
		if (!_finishedCallback.IsNull ()) {
			_finishedCallback (ID);
		}
	}

	public void Dispose(){
		TargetCoroutine = null;
		SavedCoroutien = null;
		Done = true;
		Status = TaskStatus.TS_DONE;
		_finishedCallback = null;
	}

}
