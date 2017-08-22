using System;
using UnityEngine;
using UnityEngine.Networking;

public class WebRequestManager : Singleton<WebRequestManager> {

	private TaskPool<WebTask> _taskPool;
	public float TimeOut{ get; set; }

	public int TotalAgentCount{ get { return _taskPool.TotalAgentCount; } }
	public int FreeAgentCount{ get { return _taskPool.FreeAgentCount; } }
	public int WorkingAgentCount{ get { return _taskPool.WorkingAgentCount; } }
	public int WaitingTaskCount{ get { return _taskPool.WaitingTaskCount; } }

	private Action<WebRequestEvent.StartEventArgs> _webRequestStartCallback;
	private Action<WebRequestEvent.SuccessEventArgs> _webRequestSuccessCallback;
	private Action<WebRequestEvent.FailureEventArgs> _webRequestFailureCallback;

	public event Action<WebRequestEvent.StartEventArgs> WebRequestStartHandle {add{ _webRequestStartCallback += value;}remove{ _webRequestStartCallback -= value;}}
	public event Action<WebRequestEvent.SuccessEventArgs> WebRequestSuccessHandle {add{ _webRequestSuccessCallback += value;}remove{ _webRequestSuccessCallback -= value;}}
	public event Action<WebRequestEvent.FailureEventArgs> WebRequestFailureHandle {add{ _webRequestFailureCallback += value;}remove{ _webRequestFailureCallback -= value;}}

	void Awake() {
		_taskPool = new TaskPool<WebTask> ();
		TimeOut = 15f;
		_webRequestStartCallback = args => {
			Debug.LogWarning("WebRequest ID " + args.SerialID.ToString() + ", RequestUrl: " + args.RequestUrl + ", PostData: " + (args.FormData.IsNull() ? args.PostData : args.FormData) + " Start.");
		};
		_webRequestSuccessCallback = args => {
			Debug.LogWarning("WebRequest ID: " + args.SerialID.ToString() + ", RequestUrl: " + args.RequestUrl + " Success, Receive Data " + Utility.Converter.GetStringFromBytes(args.ResponseData));
		};
		_webRequestFailureCallback = args => {
			Debug.LogError("WebRequest ID: " + args.SerialID.ToString() + ", RequestUrl: " + args.RequestUrl + " Failed, Error: " + args.Message);	
		};
	}

	void Start() {
	}

	void Update () {
		_taskPool.Update (Time.fixedUnscaledDeltaTime);
	}

	public void AddAgent(Action<WebRequestEvent.StartEventArgs> startCallback){
		var agent = new WebTaskAgent ();
		agent.OnWebRequestStartCallback = startCallback;
		agent.OnWebRequestStartCallback += OnWebRequestStart;
		agent.WebRequestSuccessHandle += OnWebRequestSuccess;
		agent.WebRequestFailureHandle += OnWebRequestFailure;

		_taskPool.AddAgent (agent);
	}

	public int AddWebRequest(string url,byte[] data, WWWForm formData,object userdata,Action<WebRequestEvent.SuccessEventArgs> succCallback,Action<WebRequestEvent.FailureEventArgs> failCallback){
		if (TotalAgentCount <= 0) {
			Debug.LogError ("Add an agent first");
		}
		WebTask task = new WebTask (url, data, TimeOut, formData, userdata);
		task.WebRequestSuccessHandle += succCallback;
		task.WebRequestFailureHandle += failCallback;
		_taskPool.AddTask (task);

		return task.ID;
	}

	public bool RemoveTask(int serialID){
		return !_taskPool.RemoveTask (serialID).IsNull ();
	}

	public void RemoveAllTasks(){
		_taskPool.RemoveAllTasks ();
	}

	public void OnWebRequestStart(WebRequestEvent.StartEventArgs args){
		if (!_webRequestStartCallback.IsNull ()) {
			_webRequestStartCallback (args);
		}
		EventPoolManager.Instance.TriggerEvent (this, args);
	}

	public void OnWebRequestSuccess(WebRequestEvent.SuccessEventArgs args){
		if (!_webRequestSuccessCallback.IsNull ()) {
			_webRequestSuccessCallback (args);
		}
		EventPoolManager.Instance.TriggerEvent (this, args);
	}

	public void OnWebRequestFailure(WebRequestEvent.FailureEventArgs args){
		if (!_webRequestFailureCallback.IsNull ()) {
			_webRequestFailureCallback (args);
		}
		EventPoolManager.Instance.TriggerEvent (this, args);
	}

	public void CloseManager(){
		_taskPool.ClearPool ();
	}

	protected override void OnDestroy(){
		this.CloseManager ();
		base.OnDestroy ();
	}
}
