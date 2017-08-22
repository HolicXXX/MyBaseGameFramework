using System;

public interface IAudioAgentHelper
{
	bool IsPlaying{ get; }
	float Time{ get; set; }
	bool Mute{ get; set; }
	bool Loop{ get; set; }
	int Priority{ get; set; }
	float Volume{ get; set; }
	float Pitch{ get; set; }

	/// <summary>
	/// 获取或设置声音立体声声相。
	/// </summary>
	float PanStereo
	{
		get;
		set;
	}
	/// <summary>
	/// 获取或设置声音空间混合量。
	/// </summary>
	float SpatialBlend
	{
		get;
		set;
	}
	/// <summary>
	/// 获取或设置声音最大距离。
	/// </summary>
	float MaxDistance
	{
		get;
		set;
	}

	event Action<object,object> ResetAudioAgentHandler;

	void Play(float fadeInSeconds);
	void Stop(float fadeOutSeconds);
	void Pause(float fadeOutSeconds);
	void Resume(float fadeInSeconds);
	void Reset();
	bool SetAudioAsset(object audioAsset);
}

