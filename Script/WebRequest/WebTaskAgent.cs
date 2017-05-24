
using System;
using UnityEngine;
using UnityEngine.Networking;

public class WebTaskAgent : ITaskAgent<WebTask> {
	public WebTask Task{ get; private set; }
	public float WaitedTime{ get; private set; }
	private UnityWebRequest _unityWebRequest;

	public Action<int> OnWebRequestStartCallback;
	private Action<byte[]> _onWebRequestSuccessCallback;
	private Action<string> _onWebRequestFailureCallback;

	public event Action<byte[]> WebRequestSuccessHandle{
		add{
			_onWebRequestSuccessCallback += value;
		}
		remove{
			_onWebRequestSuccessCallback -= value;
		}
	}

	public event Action<string> WebRequestFailureHandle{
		add{
			_onWebRequestFailureCallback += value;
		}
		remove{
			_onWebRequestFailureCallback -= value;
		}
	}

	public WebTaskAgent(){
		this.Task = null;
		this.WaitedTime = 0f;
		_unityWebRequest = null;

		OnWebRequestStartCallback = null;
		_onWebRequestSuccessCallback = null;
		_onWebRequestFailureCallback = null;
	}
		
	public void Update(float dt){
		if (this.Task.IsNull ())
			return;
		
		if (this.Task.Status == TaskStatus.TS_DOING && !_unityWebRequest.IsNull()) {
			if (_unityWebRequest.isError) {
				this.WebRequestFailure (_unityWebRequest.error);
			}
			else if(_unityWebRequest.isDone){
				this.WebRequestSuccess (_unityWebRequest.downloadHandler.data);
			}
			this.WaitedTime += dt;
			if (this.WaitedTime >= this.Task.TimeOut) {
				_onWebRequestFailureCallback("Task " + this.Task.ID + " TimeOut");
			}
		}
	}

	public void Start (WebTask task){
		
		this.Task = task;
		this.Task.Status = TaskStatus.TS_DOING;

		_onWebRequestSuccessCallback += this.Task.OnWebRequestSuccessCallback;
		_onWebRequestFailureCallback += this.Task.OnWebRequestFailureCallback;

		if (!OnWebRequestStartCallback.IsNull ()) {
			OnWebRequestStartCallback (task.ID);
		}
		Request ();
		this.WaitedTime = 0f;
	}

	void Request(){
		if (!this.Task.UserData.IsNull ()) {
			_unityWebRequest = UnityWebRequest.Post (this.Task.WebRequestUrl, Utility.Converter.GetStringFromBytes (this.Task.PostData));
		} else if (!this.Task.PostData.IsNull ()) {
			_unityWebRequest = UnityWebRequest.Post (this.Task.WebRequestUrl, this.Task.UserData as WWWForm);
		} else {
			_unityWebRequest = UnityWebRequest.Get (this.Task.WebRequestUrl);
		}
		_unityWebRequest.Send ();
	}

	public void Reset(){
		if (!this.Task.IsNull ()) {
			_onWebRequestSuccessCallback -= this.Task.OnWebRequestSuccessCallback;
			_onWebRequestFailureCallback -= this.Task.OnWebRequestFailureCallback;
		}

		this.Task = null;
		this.WaitedTime = 0f;
	}

	public void Dispose(){
		if (!_unityWebRequest.IsNull ()) {
			_unityWebRequest.Dispose ();
			_unityWebRequest = null;
		}
	}

	public void Close(){
		Reset ();
		Dispose ();
	}

	void WebRequestSuccess(byte[] data){
		this.Task.Status = TaskStatus.TS_DONE;
		this.Task.Done = true;
		if(!_onWebRequestSuccessCallback.IsNull()){
			_onWebRequestSuccessCallback (data);
		}
		this.Dispose ();
	}

	void WebRequestFailure(string message){
		this.Task.Status = TaskStatus.TS_ERROR;
		this.Task.Done = true;
		if(!_onWebRequestFailureCallback.IsNull()){
			_onWebRequestFailureCallback (message);
		}
		this.Dispose ();
	}
}
