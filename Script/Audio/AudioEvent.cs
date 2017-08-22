using System;
using UnityEngine;

public class AudioEvent
{
	public class PlaySuccessEventArgs : IEventBase
	{
		public override int ID { get { return (int)EventID.Audio_Play_Success; } }
		public PlaySuccessEventArgs(PlayAudioInfo info, string audioAssetName, AudioAgent audioAgent, float duration){
			PlayAudioInfo = info;
			AudioAssetName = audioAssetName;
			AudioAgent = audioAgent;
			Duration = duration;
		}
		public PlayAudioInfo PlayAudioInfo{ get; set; }
		public string AudioAssetName{ get; set; }
		public AudioAgent AudioAgent{ get; set; }
		public float Duration{ get; set; }
	}

	public class PlayFailureEventArgs : IEventBase
	{
		public override int ID { get { return (int)EventID.Audio_Play_Failure; } }
		public PlayFailureEventArgs(PlayAudioInfo info, string audioAssetName, PlayAudioErrorCode errorCode, string msg){
			PlayAudioInfo = info;
			AudioAssetName = audioAssetName;
			ErrorCode = errorCode;
			Message = msg;
		}
		public PlayAudioInfo PlayAudioInfo{ get; set; }
		public string AudioAssetName{ get; set; }
		public PlayAudioErrorCode ErrorCode{ get; set; }
		public string Message{ get; set; }
	}
}

