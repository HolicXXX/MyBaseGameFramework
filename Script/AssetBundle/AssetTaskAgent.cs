using System;
using UnityEngine;

public class AssetTaskAgent : ITaskAgent<AssetTask>
{
	public AssetTask Task{ get; private set; }
	public float WaitedTime{ get; private set; }

	private AssetBundleRequest _request;

	public Action<AssetEvent.StartEventArgs> OnAssetStartCallback;
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

	public AssetTaskAgent ()
	{
		Task = null;
		WaitedTime = 0f;
		_request = null;

		OnAssetStartCallback = null;
		_onAssetProgressCallback = null;
		_onAssetFailureCallback = null;
		_onAssetSuccessCallback = null;
	}

	public void Update(float dt){
		if (this.Task.IsNull () || this._request.IsNull()) {
			return;
		}

		if (this.Task.Status == TaskStatus.TS_DOING) {
			if (_request.isDone) {
				if (_request.asset.IsNull ()) {
					this.LoadFailure ("Asset Load Failure , Asset Name: " + Task.AssetName);
				} else {
					this.LoadSucccess (_request.asset);
				}
			} else {
				this.LoadProgress (this._request.progress);
			}
			//				not sure if it's necessary
			//				this.WaitedTime += dt;
			//				if (this.WaitedTime >= this.Task.TimeOut) {
			//					this.LoadFailure("Task " + this.Task.ID + " TimeOut");
			//				}
		}
	}

	public void Start (AssetTask task){
		this.Task = task;
		this.Task.Status = TaskStatus.TS_DOING;

		_onAssetProgressCallback += this.Task.OnAssetProgressCallback;
		_onAssetFailureCallback += this.Task.OnAssetFailureCallback;
		_onAssetSuccessCallback += this.Task.OnAssetSuccessCallback;

		if (!OnAssetStartCallback.IsNull ()) {
			OnAssetStartCallback (new AssetEvent.StartEventArgs (this.Task.ID, this.Task.AssetName));
		}

		Load ();
		this.WaitedTime = 0f;
	}

	void Load (){
		this._request = Task.CachedAssetBundle.LoadAssetAsync (Task.AssetName);
	}

	public void Reset (){
		if (!this.Task.IsNull ()) {
			_onAssetProgressCallback -= this.Task.OnAssetProgressCallback;
			_onAssetFailureCallback -= this.Task.OnAssetFailureCallback;
			_onAssetSuccessCallback -= this.Task.OnAssetSuccessCallback;
			this.Task = null;
		}
		this.WaitedTime = 0f;
	}

	public void Close (){
		Reset ();
		Dispose ();
	}

	public void Dispose (){
		if (!_request.IsNull ()) {
			_request = null;
		}
	}

	void LoadProgress (float pr){
		if (!_onAssetProgressCallback.IsNull ()) {
			_onAssetProgressCallback (new AssetEvent.ProgressEventArgs (this.Task.ID, this.Task.AssetName, pr));
		}
	}

	void LoadFailure (string msg){
		Task.Status = TaskStatus.TS_ERROR;
		Task.Done = true;

		if (!_onAssetFailureCallback.IsNull ()) {
			_onAssetFailureCallback (new AssetEvent.FailureEventArgs (this.Task.ID, this.Task.AssetName, msg));
		}

		this.Dispose ();
	}

	void LoadSucccess (UnityEngine.Object asset){
		Task.Status = TaskStatus.TS_DONE;
		Task.Done = true;

		if (!_onAssetSuccessCallback.IsNull ()) {
			_onAssetSuccessCallback (new AssetEvent.SuccessEventArgs (this.Task.ID, this.Task.AssetName, asset));
		}

		this.Dispose ();
	}
}

