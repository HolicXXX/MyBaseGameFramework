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

	private Action<float> _onAssetBundleProgressCallback;
	public event Action<float> AssetBundleProgressHandle
	{
		add{
			_onAssetBundleProgressCallback += value;
		}
		remove{
			_onAssetBundleProgressCallback -= value;
		}
	}

	private Action<AssetBundle> _onAssetBundleSuccessCallback;
	public event Action<AssetBundle> AssetBundleSuccessHandle
	{
		add{
			_onAssetBundleSuccessCallback += value;
		}
		remove{
			_onAssetBundleSuccessCallback -= value;
		}
	}

	private Action<string> _onAssetBundleFailureCallback;
	public event Action<string> AssetBundleFailureHandle
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

		_onAssetBundleSuccessCallback = null;
		_onAssetBundleFailureCallback = null;
	}

	public void OnAssetBundleProgressCallback(float pr){
		if (!_onAssetBundleProgressCallback.IsNull ()) {
			_onAssetBundleProgressCallback(pr);
		}
	}

	public void OnAssetBundleSuccessCallback(AssetBundle ab){
		if (!_onAssetBundleSuccessCallback.IsNull ()) {
			_onAssetBundleSuccessCallback (ab);
		}
	}

	public void OnAssetBundleFailureCallback(string msg){
		if (!_onAssetBundleFailureCallback.IsNull ()) {
			_onAssetBundleFailureCallback (msg);
		}
	}
}
