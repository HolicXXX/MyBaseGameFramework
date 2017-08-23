using System;
using UnityEngine;

public class AssetTask : ITask {

	public static int TaskSerialID = 0;

	public int ID{ get; private set; }
	public bool Done{ get; set; }
	public TaskStatus Status{ get; set; }

	public string AssetName{ get; set; }
	public bool IsScene{ get; set; }
	public float TimeOut{ get; set; }
	public AssetBundle CachedAssetBundle{ get; private set; }

	private Action<AssetEvent.ProgressEventArgs> _onAssetProgressCallback;
	public event Action<AssetEvent.ProgressEventArgs> AssetProgressHandle
	{
		add{
			_onAssetProgressCallback += value;
		}
		remove{
			_onAssetProgressCallback -= value;
		}
	}

	private Action<AssetEvent.SuccessEventArgs> _onAssetSuccessCallback;
	public event Action<AssetEvent.SuccessEventArgs> AssetSuccessHandle
	{
		add{
			_onAssetSuccessCallback += value;
		}
		remove{
			_onAssetSuccessCallback -= value;
		}
	}

	private Action<AssetEvent.FailureEventArgs> _onAssetFailureCallback;
	public event Action<AssetEvent.FailureEventArgs> AssetFailureHandle
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
		IsScene = aname.EndsWith (".unity");
		TimeOut = timeout;
		CachedAssetBundle = bundle;

		_onAssetProgressCallback = null;
		_onAssetSuccessCallback = null;
		_onAssetFailureCallback = null;
	}

	public void OnAssetProgressCallback(AssetEvent.ProgressEventArgs args){
		if (!_onAssetProgressCallback.IsNull ()) {
			_onAssetProgressCallback(args);
		}
	}

	public void OnAssetSuccessCallback(AssetEvent.SuccessEventArgs args){
		if (!_onAssetSuccessCallback.IsNull ()) {
			_onAssetSuccessCallback (args);
		}
	}

	public void OnAssetFailureCallback(AssetEvent.FailureEventArgs args){
		if (!_onAssetFailureCallback.IsNull ()) {
			_onAssetFailureCallback (args);
		}
	}
}
