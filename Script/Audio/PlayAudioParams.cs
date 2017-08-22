using UnityEngine;
using System.Collections;

public static class ConstantParams
{
	public const float DefaultTime = 0f;
	public const bool DefaultMute = false;
	public const bool DefaultLoop = false;
	public const int DefaultPriority = 0;
	public const float DefaultVolume = 1f;
	public const float DefaultFadeInSeconds = 0f;
	public const float DefaultFadeOutSeconds = 0f;
	public const float DefaultPitch = 1f;
	public const float DefaultPanStereo = 0f;
	public const float DefaultSpatialBlend = 0f;
	public const float DefaultMaxDistance = 100f;
}

public class PlayAudioParams
{
	public float Time{ get; set; }
	public bool MuteInAudioGroup{ get; set; }
	public bool Loop{ get; set; }
	public int Priority{ get; set; }
	public float VolumeInAudioGroup{ get; set; }
	public float FadeInSeconds{ get; set; }
	public float FadeOutSeconds{ get; set; }
	public float Pitch{ get; set; }
	public float PanStereo{ get; set; }
	public float SpatialBlend{ get; set; }
	public float MaxDistance{ get; set; }

	public PlayAudioParams(){
		Time = ConstantParams.DefaultTime;
		MuteInAudioGroup = ConstantParams.DefaultMute;
		Loop = ConstantParams.DefaultLoop;
		Priority = ConstantParams.DefaultPriority;
		VolumeInAudioGroup = ConstantParams.DefaultVolume;
		FadeInSeconds = ConstantParams.DefaultFadeInSeconds;
		FadeOutSeconds = ConstantParams.DefaultFadeOutSeconds;
		Pitch = ConstantParams.DefaultPitch;
		PanStereo = ConstantParams.DefaultPanStereo;
		SpatialBlend = ConstantParams.DefaultSpatialBlend;
		MaxDistance = ConstantParams.DefaultMaxDistance;
	}
}

