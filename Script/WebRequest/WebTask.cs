using System;

public class WebTask : ITask {

	public static int TaskSerialID = 0;

	public int ID{ get; private set; }
	public bool Done{ get; set; }
	public TaskStatus Status{ get; set; }

	public string WebRequestUrl{ get; private set; }
	public byte[] PostData{ get; private set; }
	public float TimeOut{ get; private set; }
	public object UserData{ get; private set; }

	private Action<byte[]> _onWebRequestSuccessCallback;
	public event Action<byte[]> WebRequestSuccessHandle
	{
		add{
			_onWebRequestSuccessCallback += value;
		}
		remove{
			_onWebRequestSuccessCallback -= value;
		}
	}

	private Action<string> _onWebRequestFailureCallback;
	public event Action<string> WebRequestFailureHandle
	{
		add{
			_onWebRequestFailureCallback += value;
		}
		remove{
			_onWebRequestFailureCallback -= value;
		}
	}

	public WebTask(string url,byte[] data,float time,object udata){
		ID = WebTask.TaskSerialID++;
		Done = false;
		Status = TaskStatus.TS_TODO;
		WebRequestUrl = url;
		PostData = data;
		TimeOut = time;
		UserData = udata;

		_onWebRequestSuccessCallback = null;
		_onWebRequestFailureCallback = null;
	}

	public void OnWebRequestSuccessCallback(byte[] data){
		if (!_onWebRequestSuccessCallback.IsNull ()) {
			_onWebRequestSuccessCallback (data);
		}
	}

	public void OnWebRequestFailureCallback(string message){
		if (!_onWebRequestFailureCallback.IsNull ()) {
			_onWebRequestFailureCallback (message);
		}
	}

}
