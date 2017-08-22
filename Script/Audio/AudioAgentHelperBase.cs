using System;
using UnityEngine;
using UnityEngine.Audio;
using System.Collections;

public class AudioAgentHelperBase : MonoBehaviour, IAudioAgentHelper
{
	private Transform m_cachedTransform = null;
	private AudioSource m_audioSource = null;
	private MonoBehaviour m_entity = null;
	private float m_volumeWhenPause = 0f;

	public virtual bool IsPlaying{ get { return m_audioSource.isPlaying; } }
	public virtual float Time{ get { return m_audioSource.time; } set { m_audioSource.time = value; } }
	public virtual bool Mute{ get { return m_audioSource.mute; } set { m_audioSource.mute = value; } }
	public virtual bool Loop{ get { return m_audioSource.loop; } set { m_audioSource.loop = value; } }
	public virtual int Priority{ get { return 128 - m_audioSource.priority; } set { m_audioSource.priority = 128 - value; } }//AudioSource default priority is 128
	public virtual float Volume{ get{ return m_audioSource.volume; } set { m_audioSource.volume = value; } }
	public virtual float Pitch{ get { return m_audioSource.pitch; } set { m_audioSource.pitch = value; } }
	/// <summary>
	/// 获取或设置声音立体声声相。
	/// </summary>
	public float PanStereo
	{
		get{
			return m_audioSource.panStereo;
		}
		set{
			m_audioSource.panStereo = value;
		}
	}
	/// <summary>
	/// 获取或设置声音空间混合量。
	/// </summary>
	public float SpatialBlend
	{
		get{
			return m_audioSource.spatialBlend;
		}
		set{
			m_audioSource.spatialBlend = value;
		}
	}
	/// <summary>
	/// 获取或设置声音最大距离。
	/// </summary>
	public float MaxDistance
	{
		get{
			return m_audioSource.maxDistance;
		}
		set{
			m_audioSource.maxDistance = value;
		}
	}

	public virtual AudioMixerGroup AudioMixerGroup{ get { return m_audioSource.outputAudioMixerGroup; } set { m_audioSource.outputAudioMixerGroup = value; } }

	private Action<object,object> _resetAudioAgentCallback = null;
	public virtual event Action<object,object> ResetAudioAgentHandler
	{
		add{
			_resetAudioAgentCallback += value;
		}
		remove{
			_resetAudioAgentCallback -= value;
		}
	}

	public virtual void Play(float fadeInSeconds){
		StopAllCoroutines ();

		m_audioSource.Play ();
		if (fadeInSeconds > 0f) {
			float volume = m_audioSource.volume;
			m_audioSource.volume = 0f;
			StartCoroutine (m_audioSource.FadeToVolume (volume, fadeInSeconds));
		}
	}

	public virtual void Stop(float fadeOutSeconds){
		StopAllCoroutines ();

		if (fadeOutSeconds > 0f) {
			StartCoroutine (StopCo (fadeOutSeconds));
		} else {
			m_audioSource.Stop ();
		}
	}

	public virtual void Pause(float fadeOutSeconds){
		StopAllCoroutines ();

		m_volumeWhenPause = m_audioSource.volume;
		if (fadeOutSeconds > 0f) {
			StartCoroutine (PauseCo (fadeOutSeconds));
		} else {
			m_audioSource.Pause ();
		}
	}

	public virtual void Resume(float fadeInSeconds){
		StopAllCoroutines ();

		m_audioSource.UnPause ();
		if (fadeInSeconds > 0f) {
			StartCoroutine (m_audioSource.FadeToVolume (m_volumeWhenPause, fadeInSeconds));
		} else {
			m_audioSource.volume = m_volumeWhenPause;
		}
	}

	public virtual void Reset(){
		m_cachedTransform.localPosition = Vector3.zero;
		m_audioSource.clip = null;
		m_entity = null;
		m_volumeWhenPause = 0f;
	}

	public virtual bool SetAudioAsset(object audioAsset){
		AudioClip clip = audioAsset as AudioClip;
		if (!clip.IsNull ()) {
			m_audioSource.clip = clip;
			return true;
		}
		return false;
	}

	public virtual void SetBindingEntity(MonoBehaviour bindingEntity){
		m_entity = bindingEntity;
		if (!bindingEntity.IsNull ()) {
			UpdateAgentPosition ();
			return;
		}

		if (_resetAudioAgentCallback != null)
		{
			_resetAudioAgentCallback(this, m_entity);
		}
	}

	public virtual void SetWorldPosition(Vector3 worldPos){
		m_cachedTransform.position = worldPos;
	}

	private void UpdateAgentPosition()
	{
		if (m_entity.gameObject.activeSelf)
		{
			m_cachedTransform.position = m_entity.gameObject.transform.position;
			return;
		}

		if (_resetAudioAgentCallback != null)
		{
			_resetAudioAgentCallback(this, m_entity);
		}
	}

	void Awake(){
		m_cachedTransform = transform;
		m_audioSource = gameObject.GetOrAddComponent<AudioSource> ();
		m_audioSource.playOnAwake = false;
		m_audioSource.rolloffMode = AudioRolloffMode.Custom;
	}

	void Update(){
		if (!IsPlaying && !m_audioSource.clip.IsNull () && !_resetAudioAgentCallback.IsNull ()) {
			_resetAudioAgentCallback (this, null);
			return;
		}
		if (!m_entity.IsNull ()) {
			UpdateAgentPosition ();
		}
	}

	private IEnumerator StopCo(float fadeOutSeconds){
		yield return m_audioSource.FadeToVolume (0f, fadeOutSeconds);
		m_audioSource.Stop ();
	}
	private IEnumerator PauseCo(float fadeOutSeconds){
		yield return m_audioSource.FadeToVolume (0f, fadeOutSeconds);
		m_audioSource.Pause ();
	}

}

