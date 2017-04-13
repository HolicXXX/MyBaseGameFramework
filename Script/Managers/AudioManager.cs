using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(AudioSource))]
public class AudioManager : Singleton<AudioManager> {

	public float AudioEffectVolume {
		get;
		set;
	}

	private Dictionary<string,AudioClip> m_dictAudios;

	private AudioSource m_audioSource;

	public void LoadAudio(){
		//TODO:LoadAudio
	}

	void Awake(){
		AudioEffectVolume = 1f;//playerprefs
		m_dictAudios = new Dictionary<string, AudioClip> ();
		m_audioSource = GetComponent<AudioSource> ();
		m_audioSource.playOnAwake = false;
		if (m_audioSource == null)
			return;
	}

	void Start () {
	}

	void Update () {
	}

	public void PlayAudioEffect(string audioDefine)
	{
		if (m_dictAudios.Count == 0) {
			Debug.Log ("No Audios in AudioManager");
			return;
		}

		if (audioDefine.Length == 0) {
			Debug.Log ("Audio name is Empty");
			return;
		}

		AudioClip audio = null;
		m_dictAudios.TryGetValue (audioDefine, out audio);
		if (audio == null) {
			Debug.Log ("Name: " + audioDefine + " doesn't exists");
			return;
		}
		m_audioSource.PlayOneShot (audio,AudioEffectVolume);
	}

	public void PlayAudioBackground(string audioDefine)
	{
		if (m_dictAudios.Count == 0) {
			Debug.Log ("No Audios in AudioManager");
			return;
		}

		if (audioDefine.Length == 0) {
			Debug.Log ("Audio name is Empty");
			return;
		}

		AudioClip audio = null;
		m_dictAudios.TryGetValue (audioDefine, out audio);
		if (audio == null) {
			Debug.Log ("Name: " + audioDefine + " doesn't exists");
			return;
		}
		m_audioSource.clip = audio;
		m_audioSource.Play ();
	}

	public void StopAudioBackground()
	{
		if (m_audioSource.isPlaying) {
			m_audioSource.Stop ();
		}
	}

	public void PauseAudioBackground()
	{
		if (m_audioSource.isPlaying) {
			m_audioSource.Pause ();
		}
	}

	public void ResumeAudioBackground()
	{
		m_audioSource.UnPause ();
	}

}
