using System;

public class WebTask : ITask {

	public static int TaskSerialID = 0;

	public int ID{ get; private set; }
	public bool Done{ get; set; }
	public TaskStatus Status{ get; set; }

	public string WebRequestUrl{ get; private set; }
	public byte[] PostData{ get; private set; }
	public float TimeOut{ get; private set; }
	public object FormData{ get; private set; }
	public object UserData{ get; private set; }

	private Action<WebRequestEvent.SuccessEventArgs> _onWebRequestSuccessCallback;
	public event Action<WebRequestEvent.SuccessEventArgs> WebRequestSuccessHandle
	{
		add{
			_onWebRequestSuccessCallback += value;
		}
		remove{
			_onWebRequestSuccessCallback -= value;
		}
	}

	private Action<WebRequestEvent.FailureEventArgs> _onWebRequestFailureCallback;
	public event Action<WebRequestEvent.FailureEventArgs> WebRequestFailureHandle
	{
		add{
			_onWebRequestFailureCallback += value;
		}
		remove{
			_onWebRequestFailureCallback -= value;
		}
	}

	public WebTask(string url,byte[] data,float time, object formData ,object userdata){
		ID = WebTask.TaskSerialID++;
		Done = false;
		Status = TaskStatus.TS_TODO;
		WebRequestUrl = url;
		PostData = data;
		TimeOut = time;
		FormData = formData;
		UserData = userdata;

		_onWebRequestSuccessCallback = null;
		_onWebRequestFailureCallback = null;
	}

	public void OnWebRequestSuccessCallback(WebRequestEvent.SuccessEventArgs args){
		if (!_onWebRequestSuccessCallback.IsNull ()) {
			_onWebRequestSuccessCallback (args);
		}
	}

	public void OnWebRequestFailureCallback(WebRequestEvent.FailureEventArgs args){
		if (!_onWebRequestFailureCallback.IsNull ()) {
			_onWebRequestFailureCallback (args);
		}
	}

}
