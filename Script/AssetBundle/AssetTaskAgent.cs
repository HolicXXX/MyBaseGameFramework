using System;
using System.IO;
using UnityEngine;
using UnityEngine.SceneManagement;

public class AssetTaskAgent : ITaskAgent<AssetTask>
{
	public AssetTask Task{ get; private set; }
	public float WaitedTime{ get; private set; }

	private AssetBundleRequest _request;
	private bool _isScene;
	private AsyncOperation _sceneLoader;

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
		_isScene = false;
		_sceneLoader = null;

		OnAssetStartCallback = null;
		_onAssetProgressCallback = null;
		_onAssetFailureCallback = null;
		_onAssetSuccessCallback = null;
	}

	public void Update(float dt){
		if (this.Task.IsNull () || (_request.IsNull () && _sceneLoader.IsNull ())) {
			return;
		}

		if (this.Task.Status == TaskStatus.TS_DOING) {
			if (_isScene) {
				if (_sceneLoader.isDone) {
					this.LoadSucccess (SceneManager.GetSceneByName (Path.GetFileNameWithoutExtension (Task.AssetName)));
				} else {
					this.LoadProgress (this._sceneLoader.progress);
				}
			} else {
				if (_request.isDone) {
					if (_request.asset.IsNull ()) {
						this.LoadFailure ("Asset Load Failure , Asset Name: " + Task.AssetName);
					} else {
						this.LoadSucccess (_request.asset);
					}
				} else {
					this.LoadProgress (this._request.progress);
				}
			}
			this.WaitedTime += dt;
			if (this.WaitedTime >= this.Task.TimeOut) {
				this.LoadFailure("Asset Task " + this.Task.ID + " TimeOut");
			}
		}
	}

	public void Start (AssetTask task){
		this.Task = task;
		this.Task.Status = TaskStatus.TS_DOING;
		this._isScene = this.Task.IsScene;

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
		if (_isScene) {
			this._sceneLoader = SceneManager.LoadSceneAsync (Path.GetFileNameWithoutExtension (Task.AssetName));
		} else {
			this._request = Task.CachedAssetBundle.LoadAssetAsync (Task.AssetName);
		}
	}

	public void Reset (){
		if (!this.Task.IsNull ()) {
			_onAssetProgressCallback -= this.Task.OnAssetProgressCallback;
			_onAssetFailureCallback -= this.Task.OnAssetFailureCallback;
			_onAssetSuccessCallback -= this.Task.OnAssetSuccessCallback;
			this.Task = null;
		}
		this.WaitedTime = 0f;
		this._isScene = false;
	}

	public void Close (){
		Reset ();
		Dispose ();
	}

	public void Dispose (){
		if (!_request.IsNull ()) {
			_request = null;
		}
		if (!_sceneLoader.IsNull ()) {
			_sceneLoader = null;
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

	void LoadSucccess (object asset){
		Task.Status = TaskStatus.TS_DONE;
		Task.Done = true;

		if (!_onAssetSuccessCallback.IsNull ()) {
			_onAssetSuccessCallback (new AssetEvent.SuccessEventArgs (this.Task.ID, this.Task.AssetName, asset));
		}

		this.Dispose ();
	}
}

