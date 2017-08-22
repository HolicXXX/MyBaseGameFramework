using System;
using UnityEngine;

public class AssetBundleEvent
{
	public class StartEventArgs : IEventBase
	{
		public override int ID{ get { return (int)EventID.Load_AssetBundle_Start; } }

		public StartEventArgs(int serialId, string bname, bool fromFile){
			SerialID = serialId;
			BundleName = bname;
			LoadFromFile = fromFile;
		}

		public int SerialID{ get; set; }
		public string BundleName{ get; set; }
		public bool LoadFromFile{ get; set; }
	}

	public class SuccessEventArgs : IEventBase
	{
		public override int ID{ get { return (int)EventID.Load_AssetBundle_Success; } }

		public SuccessEventArgs(int serialId, string bname, bool fromFile, AssetBundle ab){
			SerialID = serialId;
			BundleName = bname;
			LoadFromFile = fromFile;
			CachedBundle = ab;
		}

		public int SerialID{ get; set; }
		public string BundleName{ get; set; }
		public bool LoadFromFile{ get; set; }
		public AssetBundle CachedBundle{ get; set; }
	}

	public class ProgressEventArgs : IEventBase
	{
		public override int ID{ get { return (int)EventID.Load_AssetBundle_Progress; } }

		public ProgressEventArgs(int serialId, string bname, bool fromFile, float progress){
			SerialID = serialId;
			BundleName = bname;
			LoadFromFile = fromFile;
			Progress = progress;
		}

		public int SerialID{ get; set; }
		public string BundleName{ get; set; }
		public bool LoadFromFile{ get; set; }
		public float Progress{ get; set; }
	}

	public class FailureEventArgs : IEventBase
	{
		public override int ID{ get { return (int)EventID.Load_AssetBundle_Failure; } }

		public FailureEventArgs(int serialId, string bname, bool fromFile, string msg){
			SerialID = serialId;
			BundleName = bname;
			LoadFromFile = fromFile;
			Message = msg;
		}

		public int SerialID{ get; set; }
		public string BundleName{ get; set; }
		public bool LoadFromFile{ get; set; }
		public string Message{ get; set; }
	}
}

