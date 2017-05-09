using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AudioManager : Singleton<AudioManager> {

	float _audioEffectVolume;
	public float AudioEffectVolume {
		get{ return _audioEffectVolume;}
		set{ 
			_audioEffectVolume = value;
			if (m_bgSource) {
				m_bgSource.volume = _audioEffectVolume;
			}
			if (m_effectSource) {
				m_effectSource.volume = _audioEffectVolume;
			}
		}
	}

	Dictionary<string,AudioClip> m_dictAudios;

	AudioSource m_bgSource;
	AudioSource m_effectSource;

	/// <summary>
	/// Loads the audio,use Assetbundle,called by procedure
	/// </summary>
	/// <param name="config">Bundle name.</param>
	public void LoadAudio(string config){
		//TODO:LoadAudio
	}

	/// <summary>
	/// Unloads the audio,called by procedure
	/// </summary>
	/// <param name="config">Bundle name.</param>
	public void UnloadAudio(string config){
		//TODO:OnLoadAudio
	}

	void Awake(){
		m_dictAudios = new Dictionary<string, AudioClip> ();

		gameObject.AddComponent<AudioSource> ();
		gameObject.AddComponent<AudioSource> ();

		var sources = GetComponents<AudioSource> ();
		if (sources.Length > 1) {
			m_bgSource = sources [0];
			m_bgSource.playOnAwake = false;
			m_effectSource = sources [1];
			m_effectSource.playOnAwake = false;
		}
		AudioEffectVolume = 1f;//playerprefs
	}

	void Start () {
	}

	void Update () {
	}

	public void PlayAudioEffect(string audioDefine)
	{
		if (m_dictAudios.Count == 0){
			Debug.Log ("No Audios in AudioManager");
			return;
		}

		if (string.IsNullOrEmpty(audioDefine)){
			Debug.Log ("Audio name is Empty");
			return;
		}

		AudioClip audio = null;
		m_dictAudios.TryGetValue (audioDefine, out audio);
		if (audio == null){
			Debug.Log ("Name: " + audioDefine + " doesn't exists");
			return;
		}
		m_effectSource.PlayOneShot (audio,AudioEffectVolume);
	}

	public void PlayAudioBackground(string audioDefine)
	{
		if (m_dictAudios.Count == 0){
			Debug.Log ("No Audios in AudioManager");
			return;
		}

		if (string.IsNullOrEmpty(audioDefine)){
			Debug.Log ("Audio name is Empty");
			return;
		}

		AudioClip audio = null;
		m_dictAudios.TryGetValue (audioDefine, out audio);
		if (audio == null){
			Debug.Log ("Name: " + audioDefine + " doesn't exists");
			return;
		}
		m_bgSource.clip = audio;
		m_bgSource.Play ();
	}

	public void StopAudioBackground()
	{
		if (m_bgSource.isPlaying) {
			m_bgSource.Stop ();
		}
	}

	public void PauseAudioBackground()
	{
		if (m_bgSource.isPlaying) {
			m_bgSource.Pause ();
		}
	}

	public void ResumeAudioBackground()
	{
		m_bgSource.UnPause ();
	}

}
