using System;
using System.Collections;
using UnityEngine;

public static class CoroutineUtils {
	private class IntervalItem
	{
		Action _func;
		float _interval;
		int _repeatTimes;
		public IntervalItem(Action func, float interval, int repeatTimes){
			_func = func;
			_interval = interval;
			_repeatTimes = repeatTimes;
		}

		public IEnumerator Start(){
			int count = 0;
			while(true){
				yield return new WaitForSecondsRealtime(_interval);
				_func();
				if(_repeatTimes > 0 && ++count >= _repeatTimes){
					break;
				}
			}
		}

		public IEnumerator NextFrame(){
			yield return null;
			_func ();
		}

		public IEnumerator WaitUntil(Func<bool> predicate){
			yield return new WaitUntil (predicate);
			if(!_func.IsNull())
				_func ();
		}
	}
	public static int SetInterval (Action func, float interval = .3f, int repeatTimes = 1){
		var item = new IntervalItem (func, interval, repeatTimes);
		return CoroutineManager.Instance.StartNewCoroutineTask (item.Start());
	}
	public static bool RemoveInterval (int id){
		return CoroutineManager.Instance.StopCoroutineTask (id);
	}

	public static void CallNextFrame(Action func){
		var item = new IntervalItem (func, 0, 1);
		CoroutineManager.Instance.StartNewCoroutineTask (item.NextFrame ());
	}

	public static int WaitUntil(Func<bool> predicate, Action afterFunc){
		var item = new IntervalItem (afterFunc, 0, 1);
		return CoroutineManager.Instance.StartNewCoroutineTask (item.WaitUntil (predicate));
	}
	public static bool RemoveWaitUntil(int id){
		return CoroutineManager.Instance.StopCoroutineTask (id);
	}

}
