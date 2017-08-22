using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;

public class AudioGroup
{
	public string Name{ get; private set; }
	public int AudioAgentCount{ get { return m_audioAgents.Count; } }
	public bool AvoidBeingReplaceBySamePriority{ get; set; }
	public IAudioGroupHelper AudioGroupHelper{ get; set; }
	private List<AudioAgent> m_audioAgents;

	private bool m_mute;
	public bool Mute
	{
		get{ 
			return m_mute;
		}
		set{ 
			m_mute = value;
			foreach (AudioAgent ag in m_audioAgents) {
				ag.RefreshMute ();
			}
		}
	}
	private float m_volume;
	public float Volume
	{
		get{ 
			return m_volume;
		}
		set{ 
			m_volume = value;
			foreach (AudioAgent ag in m_audioAgents) {
				ag.RefreshVolume ();
			}
		}
	}

	public AudioGroup(string audioGroupName, IAudioGroupHelper groupHelper){
		Name = audioGroupName;
		AudioGroupHelper = groupHelper;
		m_audioAgents = new List<AudioAgent> ();
	}

	public void AddAudioAgentHelper(IAudioHelper audioHelper, IAudioAgentHelper agentHelper){
		m_audioAgents.Add (new AudioAgent (this, audioHelper, agentHelper));
	}

	public AudioAgent PlayAudio(int serialId, object audioAsset, PlayAudioParams playParams, out PlayAudioErrorCode? errorCode){
		errorCode = null;
		AudioAgent agent = null;
		foreach (AudioAgent aa in m_audioAgents) {
			if (!aa.IsPlaying) {
				agent = aa;
				break;
			}
			if (aa.Priority < playParams.Priority) {
				if (agent.IsNull () || aa.Priority < agent.Priority) {
					agent = aa;
				}
			} else if (!AvoidBeingReplaceBySamePriority && aa.Priority == playParams.Priority) {
				if (agent.IsNull () || aa.SetAudioAssetTime < agent.SetAudioAssetTime) {
					agent = aa;
				}
			}
		}

		if (agent.IsNull ()) {
			errorCode = PlayAudioErrorCode.PAEC_IGNORED_DUE_TO_LOW_PRIORITY;
			return agent;
		}

		if (!agent.SetAudioAsset (audioAsset)) {
			errorCode = PlayAudioErrorCode.PAEC_SET_AUDIO_ASSET_FAILURE;
			return null;
		}

		agent.SerialID = serialId;
		agent.Time = playParams.Time;
		agent.MuteInAudioGroup = playParams.MuteInAudioGroup;
		agent.Loop = playParams.Loop;
		agent.Priority = playParams.Priority;
		agent.VolumeInAudioGroup = playParams.VolumeInAudioGroup;
		agent.Pitch = playParams.Pitch;
		agent.PanStereo = playParams.PanStereo;
		agent.SpatialBlend = playParams.SpatialBlend;
		agent.MaxDistance = playParams.MaxDistance;
		agent.Play (playParams.FadeInSeconds);

		return agent;
	}

	public bool StopAudio(int serialId, float fadeOutSeconds){
		foreach (AudioAgent agent in m_audioAgents) {
			if (agent.SerialID == serialId) {
				agent.Stop (fadeOutSeconds);
				return true;
			}
		}
		return false;
	}

	public bool PauseAudio(int serialId, float fadeOutSeconds){
		foreach (AudioAgent agent in m_audioAgents) {
			if (agent.SerialID == serialId) {
				agent.Pause (fadeOutSeconds);
				return true;
			}
		}
		return false;
	}

	public bool ResumeAudio(int serialId, float fadeInSeconds){
		foreach (AudioAgent agent in m_audioAgents) {
			if (agent.SerialID == serialId) {
				agent.Resume (fadeInSeconds);
				return true;
			}
		}
		return false;
	}

	public void StopAllLoadedAudio(float fadeOutSceonds = 0f){
		foreach (AudioAgent agent in m_audioAgents) {
			if (agent.IsPlaying) {
				agent.Stop (fadeOutSceonds);
			}
		}
	}
}

