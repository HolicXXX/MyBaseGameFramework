using System;
using System.Collections;
using UnityEngine;
using UnityEngine.Networking;

public class AssetBundleTaskAgent : ITaskAgent<AssetBundleTask> {

	public AssetBundleTask Task{ get; private set; }
	public float WaitedTime{ get; private set; }

	private UnityWebRequest _unityRequest;
	private bool _loadFromFile;
	private int? _coroutineTaskId;
	private float _progress;
	public float Progress
	{ 
		get{ return this._progress;} 
		private set{ 
			this._progress = value;
			if (!_onAssetBundleProgressCallback.IsNull ()) {
				_onAssetBundleProgressCallback (this._progress);
			}
		} 
	}

	public Action<int> OnAssetBundleStartCallback;
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

	public AssetBundleTaskAgent(){
		Task = null;
		WaitedTime = 0f;
		_unityRequest = null;
		_progress = 0f;
		_loadFromFile = true;
		_coroutineTaskId = null;

		OnAssetBundleStartCallback = null;
		_onAssetBundleProgressCallback = null;
		_onAssetBundleFailureCallback = null;
		_onAssetBundleSuccessCallback = null;
	}

	public void Update(float dt){
		if (this.Task.IsNull ()) {
			return;
		}

		if (this.Task.Status == TaskStatus.TS_DOING) {
			if (this._loadFromFile) {
				if (this._progress == 1f) {//Means done
					this.LoadSucccess(this.Task.CacheBundle);
				} else if(this._coroutineTaskId.IsNull() || !CoroutineManager.Instance.HasCoroutineTask(this._coroutineTaskId.Value)) {
					this.LoadFailure (string.Format ("Load from file failure, coroutine missing, Bundle Name:{0}, Path:{1}", this.Task.BundleName, this.Task.BundlePath));
				}
			} else {
				if (_unityRequest.isError) {
					this.LoadFailure (_unityRequest.error);
				}
				else if(_unityRequest.isDone){
					this.LoadSucccess (DownloadHandlerAssetBundle.GetContent(_unityRequest));
				}
				this.LoadProgress (this._unityRequest.downloadProgress);
//				not sure if it's necessary
//				this.WaitedTime += dt;
//				if (this.WaitedTime >= this.Task.TimeOut) {
//					this.LoadFailure("Task " + this.Task.ID + " TimeOut");
//				}
			}

		}
	}

	public void Start (AssetBundleTask task){
		this.Task = task;
		this.Task.Status = TaskStatus.TS_DOING;
		this._loadFromFile = this.Task.LoadFromFile;

		_onAssetBundleProgressCallback += this.Task.OnAssetBundleProgressCallback;
		_onAssetBundleFailureCallback += this.Task.OnAssetBundleFailureCallback;
		_onAssetBundleSuccessCallback += this.Task.OnAssetBundleSuccessCallback;

		if (!OnAssetBundleStartCallback.IsNull ()) {
			OnAssetBundleStartCallback (this.Task.ID);
		}

		Load ();
		this.WaitedTime = 0f;
		this._progress = 0f;
	}

	void Load (){
		if (this._loadFromFile) {
			this._coroutineTaskId = CoroutineManager.Instance.StartNewCoroutineTask (LoadFromFile ());
		} else {
			this._unityRequest = UnityWebRequest.GetAssetBundle (Task.BundlePath);
			this._unityRequest.Send ();
		}
	}

	IEnumerator LoadFromFile (){
		var request = AssetBundle.LoadFromFileAsync (this.Task.BundlePath);
		yield return request;
		this.Task.CacheBundle = request.assetBundle;
		this.LoadProgress (1f);
	}

	public void Reset (){
		if (!this.Task.IsNull ()) {
			_onAssetBundleProgressCallback -= this.Task.OnAssetBundleProgressCallback;
			_onAssetBundleFailureCallback -= this.Task.OnAssetBundleFailureCallback;
			_onAssetBundleSuccessCallback -= this.Task.OnAssetBundleSuccessCallback;
			this.Task = null;
		}
		this.WaitedTime = 0f;
		this._progress = 0f;
	}

	public void Close (){
		Reset ();
		Dispose ();
	}

	public void Dispose (){
		if (!_unityRequest.IsNull ()) {
			_unityRequest.Dispose ();
			_unityRequest = null;
		}
		if (!_coroutineTaskId.IsNull ()) {
			CoroutineManager.Instance.StopCoroutineTask (_coroutineTaskId.Value);
			_coroutineTaskId = null;
		}
	}

	void LoadProgress (float pr){
		this.Progress = pr;
	}

	void LoadFailure (string msg){
		Task.Status = TaskStatus.TS_ERROR;
		Task.Done = true;

		if (!_onAssetBundleFailureCallback.IsNull ()) {
			_onAssetBundleFailureCallback (msg);
		}

		this.Dispose ();
	}

	void LoadSucccess (AssetBundle ab){
		Task.Status = TaskStatus.TS_DONE;
		Task.Done = true;

		if (!_onAssetBundleSuccessCallback.IsNull ()) {
			_onAssetBundleSuccessCallback (ab);
		}

		this.Dispose ();
	}


}
