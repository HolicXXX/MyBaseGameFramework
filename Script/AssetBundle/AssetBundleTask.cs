using System;
using UnityEngine;

public class AssetBundleTask : ITask {

	public static int TaskSerialID = 0;

	public int ID{ get; private set; }
	public bool Done{ get; set; }
	public TaskStatus Status{ get; set; }

	public string BundleName{ get; set; }
	public string BundlePath{ get; set; }
	public float TimeOut{ get; set; }
	public bool LoadFromFile{ get; set; }
	public AssetBundle CacheBundle{ get; set; }

	private Action<AssetBundleEvent.ProgressEventArgs> _onAssetBundleProgressCallback;
	public event Action<AssetBundleEvent.ProgressEventArgs> AssetBundleProgressHandle
	{
		add{
			_onAssetBundleProgressCallback += value;
		}
		remove{
			_onAssetBundleProgressCallback -= value;
		}
	}

	private Action<AssetBundleEvent.SuccessEventArgs> _onAssetBundleSuccessCallback;
	public event Action<AssetBundleEvent.SuccessEventArgs> AssetBundleSuccessHandle
	{
		add{
			_onAssetBundleSuccessCallback += value;
		}
		remove{
			_onAssetBundleSuccessCallback -= value;
		}
	}

	private Action<AssetBundleEvent.FailureEventArgs> _onAssetBundleFailureCallback;
	public event Action<AssetBundleEvent.FailureEventArgs> AssetBundleFailureHandle
	{
		add{
			_onAssetBundleFailureCallback += value;
		}
		remove{
			_onAssetBundleFailureCallback -= value;
		}
	}

	public AssetBundleTask(string bname,string bpath,float timeout,bool fromFile){
		ID = AssetBundleTask.TaskSerialID++;
		Done = false;
		Status = TaskStatus.TS_TODO;
		BundleName = bname;
		BundlePath = bpath;
		TimeOut = timeout;
		LoadFromFile = fromFile;
		CacheBundle = null;

		_onAssetBundleProgressCallback = null;
		_onAssetBundleSuccessCallback = null;
		_onAssetBundleFailureCallback = null;
	}

	public void OnAssetBundleProgressCallback(AssetBundleEvent.ProgressEventArgs args){
		if (!_onAssetBundleProgressCallback.IsNull ()) {
			_onAssetBundleProgressCallback(args);
		}
	}

	public void OnAssetBundleSuccessCallback(AssetBundleEvent.SuccessEventArgs args){
		if (!_onAssetBundleSuccessCallback.IsNull ()) {
			_onAssetBundleSuccessCallback (args);
		}
	}

	public void OnAssetBundleFailureCallback(AssetBundleEvent.FailureEventArgs args){
		if (!_onAssetBundleFailureCallback.IsNull ()) {
			_onAssetBundleFailureCallback (args);
		}
	}
}
