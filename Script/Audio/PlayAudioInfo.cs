using UnityEngine;
using System.Collections;

public class PlayAudioInfo
{
	public int SerialID{ get; }
	public AudioGroup CachedAudioGroup{ get; }
	public PlayAudioParams PlayParams{ get; }
	public MonoBehaviour BindingEntity{ get; }
	public Vector3 WorldPosition{ get; }
	public object UserData{ get; }
	public PlayAudioInfo(int serialId, AudioGroup audioGroup, PlayAudioParams playParams, MonoBehaviour bindEntity, Vector3 wPos, object userData){
		SerialID = serialId;
		CachedAudioGroup = audioGroup;
		PlayParams = playParams;
		BindingEntity = bindEntity;
		WorldPosition = wPos;
		UserData = userData;
	}
}

