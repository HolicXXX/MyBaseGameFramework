using System;

public class WebRequestEvent
{
	public class StartEventArgs : IEventBase
	{
		public override int ID { get { return (int)EventID.WebRequest_Start; } }
		public StartEventArgs(int serialId, object formData, byte[] postData, string url, object userData){
			SerialID = serialId;
			FormData = formData;
			PostData = postData;
			RequestUrl = url;
			UserData = userData;
		}
		public int SerialID{ get; set; }
		public object FormData { get; set; }
		public byte[] PostData { get; set; }
		public string RequestUrl { get; set; }
		public object UserData { get; set; }
	}

	public class SuccessEventArgs : IEventBase
	{
		public override int ID { get { return (int)EventID.WebRequest_Success; } }
		public SuccessEventArgs(int serialId, object formData, byte[] postData, string url, byte[] responseBytes, object userData){
			SerialID = serialId;
			FormData = formData;
			PostData = postData;
			RequestUrl = url;
			ResponseData = responseBytes;
			UserData = userData;
		}
		public int SerialID{ get; set; }
		public object FormData { get; set; }
		public byte[] PostData { get; set; }
		public string RequestUrl { get; set; }
		public byte[] ResponseData{ get; set; }
		public object UserData { get; set; }
	}

	public class FailureEventArgs : IEventBase
	{
		public override int ID { get { return (int)EventID.WebRequest_Failure; } }
		public FailureEventArgs(int serialId, object formData, byte[] postData, string url, string msg, object userData){
			SerialID = serialId;
			FormData = formData;
			PostData = postData;
			RequestUrl = url;
			Message = msg;
			UserData = userData;
		}
		public int SerialID{ get; set; }
		public object FormData { get; set; }
		public byte[] PostData { get; set; }
		public string RequestUrl { get; set; }
		public string Message { get ; set; }
		public object UserData { get; set; }
	}
}

