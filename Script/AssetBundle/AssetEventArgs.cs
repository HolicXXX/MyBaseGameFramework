using UnityEngine;

public class AssetEvent
{
	public class StartEventArgs : IEventBase
	{
		public override int ID{ get{ return (int)EventID.Load_Asset_Start; } }

		public StartEventArgs(int serialId, string name){
			SerialID = serialId;
			AssetName = name;
		}

		public int SerialID{ get; set; }
		public string AssetName{ get; set; }
	}

	public class ProgressEventArgs : IEventBase
	{
		public override int ID{ get{ return (int)EventID.Load_Asset_Progress; } }

		public ProgressEventArgs(int serialId, string name, float progress){
			SerialID = serialId;
			AssetName = name;
			Progress = progress;
		}

		public int SerialID{ get; set; }
		public string AssetName{ get; set; }
		public float Progress{ get; set; }
	}

	public class SuccessEventArgs : IEventBase
	{
		public override int ID{ get{ return (int)EventID.Load_Asset_Success; } }

		public SuccessEventArgs(int serialId, string name, Object asset){
			SerialID = serialId;
			AssetName = name;
			Asset = asset;
		}

		public int SerialID{ get; set; }
		public string AssetName{ get; set; }
		public Object Asset{ get; set; }
	}

	public class FailureEventArgs : IEventBase
	{
		public override int ID{ get{ return (int)EventID.Load_Asset_Failure; } }

		public FailureEventArgs(int serialId, string name, string msg){
			SerialID = serialId;
			AssetName = name;
			Message = msg;
		}

		public int SerialID{ get; set; }
		public string AssetName{ get; set; }
		public string Message{ get; set; }
	}
}
