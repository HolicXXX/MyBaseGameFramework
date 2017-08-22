using System;
using System.Collections;
using UnityEngine;
using UnityEngine.Networking;

public class AssetBundleTaskAgent : ITaskAgent<AssetBundleTask> {

	public AssetBundleTask Task{ get; private set; }
	public float WaitedTime{ get; private set; }

	private UnityWebRequest _unityRequest;
	private AssetBundleCreateRequest _abCreateRequest;
	private bool _loadFromFile;
	private float _progress;
	public float Progress
	{ 
		get{ return this._progress;} 
		private set{ 
			this._progress = value;
			if (!_onAssetBundleProgressCallback.IsNull ()) {
				_onAssetBundleProgressCallback (new AssetBundleEvent.ProgressEventArgs (this.Task.ID, this.Task.BundleName, this.Task.LoadFromFile, this._progress));
			}
		} 
	}

	public Action<AssetBundleEvent.StartEventArgs> OnAssetBundleStartCallback;
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

	public AssetBundleTaskAgent(){
		Task = null;
		WaitedTime = 0f;
		_unityRequest = null;
		_abCreateRequest = null;
		_progress = 0f;
		_loadFromFile = true;

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
				if (_abCreateRequest.isDone) {
					this.Task.CacheBundle = this._abCreateRequest.assetBundle;
					if (this.Task.CacheBundle.IsNull ()) {
						this.LoadFailure ("AssetBundle Load Failure");
					} else {
						this.LoadSucccess(this.Task.CacheBundle);
					}
					return;
				}
				this.LoadProgress (this._abCreateRequest.progress);
			} else {
				if (_unityRequest.isError) {
					this.LoadFailure (_unityRequest.error);
					return;
				}
				else if(_unityRequest.isDone){
					this.Task.CacheBundle = DownloadHandlerAssetBundle.GetContent (_unityRequest);
					this.LoadSucccess (this.Task.CacheBundle);
					return;
				}
				this.LoadProgress (this._unityRequest.downloadProgress);
			}
			this.WaitedTime += dt;
			if (this.WaitedTime >= this.Task.TimeOut) {
				this.LoadFailure("AssetBundle Task " + this.Task.ID + " TimeOut");
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
			OnAssetBundleStartCallback (new AssetBundleEvent.StartEventArgs (this.Task.ID, this.Task.BundleName, this.Task.LoadFromFile));
		}

		Load ();
		this.WaitedTime = 0f;
		this._progress = 0f;
	}

	void Load (){
		if (this._loadFromFile) {
			this._abCreateRequest = AssetBundle.LoadFromFileAsync (this.Task.BundlePath);
		} else {
			this._unityRequest = UnityWebRequest.GetAssetBundle (Task.BundlePath);
			this._unityRequest.Send ();
		}
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
		if (!_abCreateRequest.IsNull ()) {
			_abCreateRequest = null;
		}
	}

	void LoadProgress (float pr){
		if (this.Task.Status == TaskStatus.TS_DOING) {
			this.Progress = pr;
		} else if (this.Task.Status == TaskStatus.TS_DONE) {
			this.Progress = 1f;
		} else if (this.Task.Status == TaskStatus.TS_ERROR) {
			this.Progress = 0f;
		}
	}

	void LoadFailure (string msg){
		Task.Status = TaskStatus.TS_ERROR;
		Task.Done = true;

		if (!_onAssetBundleFailureCallback.IsNull ()) {
			_onAssetBundleFailureCallback (new AssetBundleEvent.FailureEventArgs (this.Task.ID, this.Task.BundleName, this.Task.LoadFromFile, msg));
		}

		this.Dispose ();
	}

	void LoadSucccess (AssetBundle ab){
		Task.Status = TaskStatus.TS_DONE;
		Task.Done = true;

		if (!_onAssetBundleSuccessCallback.IsNull ()) {
			_onAssetBundleSuccessCallback (new AssetBundleEvent.SuccessEventArgs (this.Task.ID, this.Task.BundleName, this.Task.LoadFromFile, ab));
		}

		this.Dispose ();
	}


}
