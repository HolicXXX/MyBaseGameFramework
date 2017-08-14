using System;
using UnityEngine;

public class AssetTask : ITask {

	public static int TaskSerialID = 0;

	public int ID{ get; private set; }
	public bool Done{ get; set; }
	public TaskStatus Status{ get; set; }

	public string AssetName{ get; set; }
	public float TimeOut{ get; set; }
	public AssetBundle CachedAssetBundle{ get; private set; }

	private Action<float> _onAssetProgressCallback;
	public event Action<float> AssetProgressHandle
	{
		add{
			_onAssetProgressCallback += value;
		}
		remove{
			_onAssetProgressCallback -= value;
		}
	}

	private Action<UnityEngine.Object> _onAssetSuccessCallback;
	public event Action<UnityEngine.Object> AssetSuccessHandle
	{
		add{
			_onAssetSuccessCallback += value;
		}
		remove{
			_onAssetSuccessCallback -= value;
		}
	}

	private Action<string> _onAssetFailureCallback;
	public event Action<string> AssetFailureHandle
	{
		add{
			_onAssetFailureCallback += value;
		}
		remove{
			_onAssetFailureCallback -= value;
		}
	}

	public AssetTask(string aname,float timeout,AssetBundle bundle){
		ID = AssetTask.TaskSerialID++;
		Done = false;
		Status = TaskStatus.TS_TODO;
		AssetName = aname;
		TimeOut = timeout;
		CachedAssetBundle = bundle;

		_onAssetProgressCallback = null;
		_onAssetSuccessCallback = null;
		_onAssetFailureCallback = null;
	}

	public void OnAssetProgressCallback(float pr){
		if (!_onAssetProgressCallback.IsNull ()) {
			_onAssetProgressCallback(pr);
		}
	}

	public void OnAssetSuccessCallback(UnityEngine.Object asset){
		if (!_onAssetSuccessCallback.IsNull ()) {
			_onAssetSuccessCallback (asset);
		}
	}

	public void OnAssetFailureCallback(string msg){
		if (!_onAssetFailureCallback.IsNull ()) {
			_onAssetFailureCallback (msg);
		}
	}
}
