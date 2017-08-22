using UnityEngine;
using System;
using System.Collections;

public class AudioAgent
{
	public AudioGroup CachedAudioGroup{ get; private set; }
	public IAudioAgentHelper AudioAgentHelper{ get; private set; }
	public int SerialID{ get; set; }
	public bool IsPlaying{ get { return AudioAgentHelper.IsPlaying; } }
	public float Time{ get { return AudioAgentHelper.Time; } set { AudioAgentHelper.Time = value; } }
	public bool Mute{ get { return AudioAgentHelper.Mute; } }
	private bool _mudeInAudioGroup;
	public bool MuteInAudioGroup {
		get { return _mudeInAudioGroup; }
		set {
			_mudeInAudioGroup = value;
			RefreshMute ();
		}
	}
	public bool Loop{ get { return AudioAgentHelper.Loop; } set { AudioAgentHelper.Loop = value; } }
	public int Priority{ get { return AudioAgentHelper.Priority; } set { AudioAgentHelper.Priority = value; } }
	public float Volume{ get { return AudioAgentHelper.Volume; } }
	private float _volumeInAudioGroup;
	public float VolumeInAudioGroup {
		get{ return _volumeInAudioGroup; }
		set {
			_volumeInAudioGroup = value;
			RefreshVolume ();
		}
	}
	public float Pitch{ get { return AudioAgentHelper.Pitch; } set { AudioAgentHelper.Pitch = value; } }
	public float PanStereo{ get { return AudioAgentHelper.PanStereo; } set { AudioAgentHelper.PanStereo = value; } }
	public float SpatialBlend{ get { return AudioAgentHelper.SpatialBlend; } set { AudioAgentHelper.SpatialBlend = value; } }
	public float MaxDistance{ get { return AudioAgentHelper.MaxDistance; } set { AudioAgentHelper.MaxDistance = value; } }
	public DateTime SetAudioAssetTime{ get; private set; }

	private object m_audioAsset;
	private IAudioHelper m_audioHelper;

	public AudioAgent(AudioGroup audioGroup, IAudioHelper audioHelper, IAudioAgentHelper agentHelper){
		if (audioGroup.IsNull () || audioHelper.IsNull () || agentHelper.IsNull ()) {
			Debug.LogError ("Construct AudioAgent failed for invalid params");
		} else {
			CachedAudioGroup = audioGroup;
			m_audioHelper = audioHelper;
			AudioAgentHelper = agentHelper;
			AudioAgentHelper.ResetAudioAgentHandler += OnResetAudioAgent;
			SerialID = 0;
			m_audioAsset = null;
			Reset ();
		}
	}

	public void Play(float fadeInSeconds = 0f){
		AudioAgentHelper.Play (fadeInSeconds);
	}

	public void Stop(float fadeOutSeconds = 0f){
		AudioAgentHelper.Stop (fadeOutSeconds);
	}

	public void Pause(float fadeOutSeconds = 0f){
		AudioAgentHelper.Stop (fadeOutSeconds);
	}

	public void Resume(float fadeInSeconds = 0f){
		AudioAgentHelper.Stop (fadeInSeconds);
	}

	public void Reset(){
		if (!m_audioAsset.IsNull ()) {
			m_audioHelper.ReleaseAudioAsset (null, m_audioAsset);
			m_audioAsset = null;
		}

		SetAudioAssetTime = DateTime.MinValue;
		Time = ConstantParams.DefaultTime;
		MuteInAudioGroup = ConstantParams.DefaultMute;
		Loop = ConstantParams.DefaultLoop;
		Priority = ConstantParams.DefaultPriority;
		VolumeInAudioGroup = ConstantParams.DefaultVolume;
		Pitch = ConstantParams.DefaultPitch;
		PanStereo = ConstantParams.DefaultPanStereo;
		SpatialBlend = ConstantParams.DefaultSpatialBlend;
		MaxDistance = ConstantParams.DefaultMaxDistance;

		AudioAgentHelper.Reset();
	}

	public bool SetAudioAsset(object audioAsset){
		Reset ();

		m_audioAsset = audioAsset;
		SetAudioAssetTime = DateTime.Now;

		return AudioAgentHelper.SetAudioAsset (audioAsset);
	}

	public void RefreshMute(){
		AudioAgentHelper.Mute = Mute || _mudeInAudioGroup;
	}

	public void RefreshVolume(){
		AudioAgentHelper.Volume = CachedAudioGroup.Volume * _volumeInAudioGroup;
	}

	private void OnResetAudioAgent(object sender, object args){
		Reset ();
	}
}

