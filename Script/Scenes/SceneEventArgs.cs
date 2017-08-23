using UnityEngine;
using System.Collections;

public class SceneEvent
{
	public class LoadSuccessEventArgs : IEventBase
	{
		public override int ID{ get{ return (int)EventID.Scene_Load_Success;} }
		public LoadSuccessEventArgs (string sceneAssetName, float duration, object userData){
			SceneAssetName = sceneAssetName;
			Duration = duration;
			UserData = userData;
		}
		public string SceneAssetName{ get; set; }
		public float Duration{ get; set; }
		public object UserData{ get; set; }
	}

	public class LoadUpdateEventArgs : IEventBase
	{
		public override int ID{ get{ return (int)EventID.Scene_Load_Update;} }
		public LoadUpdateEventArgs(string sceneAssetName, float progress, object userData){
			SceneAssetName = sceneAssetName;
			Progress = progress;
			UserData = userData;
		}
		public string SceneAssetName{ get; set; }
		public float Progress{ get; set; }
		public object UserData{ get; set; }
	}

	public class LoadFailureEventArgs : IEventBase
	{
		public override int ID{get{ return (int)EventID.Scene_Load_Failure; } }
		public LoadFailureEventArgs(string sceneAssetName, string errorMessage, object userData){
			SceneAssetName = sceneAssetName;
			Message = errorMessage;
			UserData = userData;
		}
		public string SceneAssetName{ get; set; }
		public string Message{ get; set; }
		public object UserData{ get; set; }
	}

	public class UnloadSuccessEventArgs : IEventBase
	{
		public override int ID{ get{ return (int)EventID.Scene_Unload_Success; } }
		public UnloadSuccessEventArgs(string sceneAssetName, object userData){
			SceneAssetName = sceneAssetName;
			UserData = userData;
		}
		public string SceneAssetName{ get; set; }
		public object UserData{ get; set; }
	}

	public class UnloadFailureEventArgs : IEventBase
	{
		public override int ID{ get{ return (int)EventID.Scene_Unload_Failure; } }
		public UnloadFailureEventArgs(string sceneAssetName, string errorMessage, object userData){
			SceneAssetName = sceneAssetName;
			Message = errorMessage;
			UserData = userData;
		}
		public string SceneAssetName{ get; set; }
		public string Message{ get; set; }
		public object UserData{ get; set; }
	}

}

