using System;
using UnityEngine;

public class WebRequestManager : Singleton<WebRequestManager> {

	private TaskPool<WebTask> _taskPool;
	public float TimeOut{ get; set; }

	public int TotalAgentCount{ get { return _taskPool.TotalAgentCount; } }
	public int FreeAgentCount{ get { return _taskPool.FreeAgentCount; } }
	public int WorkingAgentCount{ get { return _taskPool.WorkingAgentCount; } }
	public int WaitingTaskCount{ get { return _taskPool.WaitingTaskCount; } }

	private Action<int> _webRequestStartCallback;
	private Action<byte[]> _webRequestSuccessCallback;
	private Action<string> _webRequestFailureCallback;

	public event Action<int> WebRequestStartHandle {add{ _webRequestStartCallback += value;}remove{ _webRequestStartCallback -= value;}}
	public event Action<byte[]> WebRequestSuccessHandle {add{ _webRequestSuccessCallback += value;}remove{ _webRequestSuccessCallback -= value;}}
	public event Action<string> WebRequestFailureHandle {add{ _webRequestFailureCallback += value;}remove{ _webRequestFailureCallback -= value;}}

	void Awake() {
		_taskPool = new TaskPool<WebTask> ();
		TimeOut = 15f;
		_webRequestStartCallback = (id) => {
			Debug.LogWarning("Task " + id + " Start");	
		};
		_webRequestSuccessCallback = (data) => {
			Debug.LogWarning("Received Length: " + data.Length);
			Debug.LogWarning("Receive Data " + Utility.Converter.GetStringFromBytes(data));
		};
		_webRequestFailureCallback = (message) => {
			Debug.LogError("Task Error: " + message);	
		};
	}

	void Start() {
	}

	void Update () {
		_taskPool.Update (Time.fixedUnscaledDeltaTime);
	}

	public void AddAgent(Action<int> startCallback){
		var agent = new WebTaskAgent ();
		agent.OnWebRequestStartCallback = startCallback;
		agent.OnWebRequestStartCallback += OnWebRequestStart;
		agent.WebRequestSuccessHandle += OnWebRequestSuccess;
		agent.WebRequestFailureHandle += OnWebRequestFailure;

		_taskPool.AddAgent (agent);
	}

	public int AddWebRequest(string url,byte[] data,object userdata,Action<byte[]> succCallback,Action<string> failCallback){
		if (TotalAgentCount <= 0) {
			Debug.LogError ("Add an agent first");
		}
		WebTask task = new WebTask (url, data, TimeOut, userdata);
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

	public void OnWebRequestStart(int id){
		if (!_webRequestStartCallback.IsNull ()) {
			_webRequestStartCallback (id);
		}
	}

	public void OnWebRequestSuccess(byte[] data){
		if (!_webRequestSuccessCallback.IsNull ()) {
			_webRequestSuccessCallback (data);
		}
	}

	public void OnWebRequestFailure(string message){
		if (!_webRequestFailureCallback.IsNull ()) {
			_webRequestFailureCallback (message);
		}
	}

	public void CloseManager(){
		_taskPool.ClearPool ();
	}

	protected override void OnDestroy(){
		this.CloseManager ();
		base.OnDestroy ();
	}
}
