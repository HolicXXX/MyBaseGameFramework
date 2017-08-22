using System;

public class UIEvent
{
	public class OpenSuccessEventArgs : IEventBase
	{
		public override int ID { get { return (int)EventID.UI_Open_Success; } }
		public OpenSuccessEventArgs(IUIBase ui, float duration, object userData){
			UI = ui;
			Duration = duration;
			UserData = userData;
		}
		public IUIBase UI { get; set; }
		public float Duration { get; set; }
		public object UserData { get; set; }
	}

	public class OpenProgressEventArgs : IEventBase
	{
		public override int ID { get { return (int)EventID.UI_Open_Progress; } }
		public OpenProgressEventArgs(int serialId, string uiAssetName, string uiGroupName, bool pauseCoveredUI, float progress, object userData){
			SerialID = serialId;
			UIAssetName = uiAssetName;
			UIGroupName = uiGroupName;
			PauseCoveredUI = pauseCoveredUI;
			Progress = progress;
			UserData = userData;
		}
		public int SerialID{ get; set; }
		public string UIAssetName{ get; set; }
		public string UIGroupName{ get; set; }
		public bool PauseCoveredUI{ get; set; }
		public float Progress{ get; set; }
		public object UserData { get; set; }
	}

	public class OpenFailureEventArgs : IEventBase
	{
		public override int ID { get { return (int)EventID.UI_Open_Failure; } }
		public OpenFailureEventArgs(int serialId, string uiAssetName, string uiGroupName, bool pauseCoveredUI, string msg, object userData){
			SerialID = serialId;
			UIAssetName = uiAssetName;
			UIGroupName = uiGroupName;
			PauseCoveredUI = pauseCoveredUI;
			ErrorMessage = msg;
			UserData = userData;
		}
		public int SerialID{ get; set; }
		public string UIAssetName{ get; set; }
		public string UIGroupName{ get; set; }
		public bool PauseCoveredUI{ get; set; }
		public string ErrorMessage{ get; set; }
		public object UserData { get; set; }
	}

	public class CloseCompleteEventArgs : IEventBase
	{
		public override int ID { get { return (int)EventID.UI_Close_Complete; } }
		public CloseCompleteEventArgs(int serialId, string uiAssetName, IUIGroup uiGroup, object userData){
			SerialID = serialId;
			UIAssetName = uiAssetName;
			UIGroup = uiGroup;
			UserData = userData;
		}
		public int SerialID{ get; set; }
		public string UIAssetName{ get; set; }
		public IUIGroup UIGroup{ get; set; }
		public object UserData { get; set; }
	}

}

