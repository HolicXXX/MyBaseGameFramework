using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.SceneManagement;

public class AudioManager : Singleton<AudioManager> {

	private AudioListener m_audioListener = null;

	private Dictionary<string,AudioGroup> m_audioGroupDict;
	private List<int> m_audioLoading;
	private HashSet<int> m_audioLoadingToRelease;
	[SerializeField]
	private Transform m_InstanceRoot;

	public Type AudioGroupHelperType{ get; set; }
	public Type AudioAgentHelperType{ get; set; }
	public IAudioHelper AudioHelper{ get ; private set; }
	private int m_serialId;

	public AudioMixer AudioMixer{ get; private set; }
	public int AudioGroupCount { get { return m_audioGroupDict.Count; } }
	private Action<AudioEvent.PlaySuccessEventArgs> _playAudioSuccessCallback;
	public event Action<AudioEvent.PlaySuccessEventArgs> PlayAudioSuccessHandler
	{
		add{
			_playAudioSuccessCallback += value;
		}
		remove{
			_playAudioSuccessCallback -= value;
		}
	}
	private Action<AudioEvent.PlayFailureEventArgs> _playAudioFailureCallback;
	public event Action<AudioEvent.PlayFailureEventArgs> PlayAudioFailureHandler
	{
		add{
			_playAudioFailureCallback += value;
		}
		remove{
			_playAudioFailureCallback -= value;
		}
	}

	void Awake(){
		m_audioGroupDict = new Dictionary<string, AudioGroup> ();
		m_audioLoading = new List<int> ();
		m_audioLoadingToRelease = new HashSet<int> ();
		m_serialId = 0;
		m_InstanceRoot = null;
		AudioHelper = null;

		m_audioListener = gameObject.GetOrAddComponent<AudioListener> ();
		SceneManager.sceneLoaded += OnSceneLoaded;
		SceneManager.sceneUnloaded += OnSceneUnloaded;

		AudioGroupHelperType = typeof(AudioGroupHelperBase);
		AudioAgentHelperType = typeof(AudioAgentHelperBase);
	}

	void Start(){
		if (m_InstanceRoot.IsNull ()) {
			m_InstanceRoot = new GameObject ("Audio Instances Root").transform;
			m_InstanceRoot.SetParent (gameObject.transform);
			m_InstanceRoot.localScale = Vector3.one;
		}
		if (AudioHelper.IsNull()) {
			GameObject helper = new GameObject ("Audio Helper");
			AudioHelper = helper.AddComponent<AudioHelperBase> ();
			Transform trans = helper.transform;
			trans.SetParent (this.transform);
			trans.localScale = Vector3.one;
		}
		InitGroupConfig ();
	}

	protected override void OnDestroy(){
		SceneManager.sceneLoaded -= OnSceneLoaded;
		SceneManager.sceneUnloaded -= OnSceneUnloaded;
		base.OnDestroy ();
	}

	private void InitGroupConfig(){
		//TODO: init with config file or set in prefab,Use AudioGroup[]
		AddAudioGroup ("Music", true, false, 1f, 2);
		AddAudioGroup ("Sound", false, false, 1f, 5);
		AddAudioGroup ("UISound", false, false, 1f, 5);
	}

	public bool HasAudioGroup(string groupName){
		return !string.IsNullOrEmpty (groupName) && m_audioGroupDict.ContainsKey (groupName);
	}

	public AudioGroup GetAudioGroup(string groupName){
		AudioGroup ret = null;
		m_audioGroupDict.TryGetValue (groupName, out ret);
		return ret;
	}

	public AudioGroup[] GetaAllAudioGroups(){
		int index = 0;
		AudioGroup[] ret = new AudioGroup[m_audioGroupDict.Count];
		foreach (var pair in m_audioGroupDict) {
			ret [index++] = pair.Value;
		}
		return ret;
	}

	public bool AddAudioGroup(string audioGroupName, bool avoidBeingReplaceBySamePriority, bool audioGroupMute, float audioGroupVolume, int audioGroupAgentCount){
		if (HasAudioGroup (audioGroupName)) {
			return false;
		}

		AudioGroupHelperBase helper = new GameObject ("Audio Group " + audioGroupName).AddComponent (AudioGroupHelperType) as AudioGroupHelperBase;
		if (helper.IsNull ()) {
			Debug.LogError ("Create Audio Group Helper Failed.");
			return false;
		}
		Transform trans = helper.transform;
		trans.SetParent (m_InstanceRoot.transform);
		trans.localScale = Vector3.one;

		if (AudioMixer != null) {
			AudioMixerGroup[] groups = AudioMixer.FindMatchingGroups ("Master/" + audioGroupName);
			if (groups.Length > 0) {
				helper.AudioMixerGroup = groups [0];
			} else {
				helper.AudioMixerGroup = AudioMixer.FindMatchingGroups ("Master") [0];
			}
		}

		AudioGroup audioGroup = new AudioGroup (audioGroupName, helper);
		audioGroup.AvoidBeingReplaceBySamePriority = avoidBeingReplaceBySamePriority;
		audioGroup.Mute = audioGroupMute;
		audioGroup.Volume = audioGroupVolume;
		m_audioGroupDict.Add (audioGroupName, audioGroup);

		for (int i = 0; i < audioGroupAgentCount; ++i) {
			if (!AddAudioAgentHelper (audioGroupName, helper, i)) {
				return false;
			}
		}

		return true;
	}

	private bool AddAudioAgentHelper(string groupName, AudioGroupHelperBase groupHelper, int index){
		AudioAgentHelperBase helper = new GameObject ("Audio Agent Helper " + groupName + " " + index.ToString ()).AddComponent(AudioAgentHelperType) as AudioAgentHelperBase;
		if (helper.IsNull ()) {
			Debug.LogError ("Create Agent Helper Failed.");
			return false;
		}
		Transform trans = helper.transform;
		trans.SetParent (groupHelper.transform);
		trans.localScale = Vector3.one;
		if (AudioMixer != null)
		{
			AudioMixerGroup[] audioMixerGroups = AudioMixer.FindMatchingGroups(string.Format("Master/{0}/{1}", groupName, index.ToString()));
			if (audioMixerGroups.Length > 0)
			{
				helper.AudioMixerGroup = audioMixerGroups[0];
			}
			else
			{
				helper.AudioMixerGroup = groupHelper.AudioMixerGroup;
			}
		}

		AudioGroup audioGroup = GetAudioGroup (groupName);
		audioGroup.AddAudioAgentHelper (AudioHelper, helper);
		return true;
	}

	public int[] GetAllLoadingAudioSerialIDs(){
		return m_audioLoading.ToArray ();
	}

	public bool IsLoadingAudio(int id){
		return m_audioLoading.Contains (id);
	}

	public int PlayAudio (string audioAssetName, string audioGroupName, PlayAudioParams audioParams, MonoBehaviour entity, object userData){
		return PlayAudio (audioAssetName, audioGroupName, audioParams, entity, Vector3.zero, userData);
	}

	public int PlayAudio (string audioAssetName, string audioGroupName, PlayAudioParams audioParams, Vector3 worldPos, object userData){
		return PlayAudio (audioAssetName, audioGroupName, audioParams, null, worldPos, userData);
	}

	private int PlayAudio (string audioAssetName, string audioGroupName, PlayAudioParams audioParams, MonoBehaviour entity, Vector3 worldPos, object userData){
		if (audioParams.IsNull ()) {
			audioParams = new PlayAudioParams ();
		}
		int serialId = m_serialId++;
		PlayAudioErrorCode? code = null;
		string errorMessage = null;
		AudioGroup audioGroup = GetAudioGroup (audioGroupName);
		if (audioGroup.IsNull ()) {
			code = PlayAudioErrorCode.PAEC_AUDIO_GROUP_NOT_EXIST;
			errorMessage = "Audio Group " + audioGroupName + " Not Exists.";
		} else if (audioGroup.AudioAgentCount <= 0) {
			code = PlayAudioErrorCode.PAEC_AUDIO_GROUP_HAS_NO_AGENT;
			errorMessage = "Audio Group " + audioGroupName + " Has No Audio Agent.";
		}

		PlayAudioInfo info = new PlayAudioInfo (serialId, audioGroup, audioParams, entity, worldPos, userData);
		if (code.HasValue) {
			AudioEvent.PlayFailureEventArgs args = new AudioEvent.PlayFailureEventArgs (info, audioAssetName, code.Value, errorMessage);
			if (!_playAudioFailureCallback.IsNull ()) {
				_playAudioFailureCallback (args);
			}
			EventPoolManager.Instance.TriggerEvent (this, args);
			return serialId;
		}

		m_audioLoading.Add (serialId);
		float? duration = null;
		AssetBundleManager.Instance.AddAssetTask (AssetBundleManager.Instance.GetBundleNameByAssetName (audioAssetName), audioAssetName,
			args => {
				if(!duration.HasValue){
					duration = Time.deltaTime;
				}else{
					duration += Time.deltaTime;
				}
			},
			args => {
				LoadAudioSuccessCallback(info, args.AssetName,args.Asset, duration.Value);
			},
			args => {
				LoadAudioFailureCallback(info,args.AssetName,args.Message);
			});
		return serialId;
	}

	public void StopAudio(int serialId, float fadeOutSeconds = 0f){
		if (IsLoadingAudio (serialId)) {
			m_audioLoadingToRelease.Add (serialId);
			return;
		}

		foreach (var pair in m_audioGroupDict) {
			if (pair.Value.StopAudio (serialId, fadeOutSeconds)) {
				break;
			}
		}
	}

	public void StopAllLoadingAudio(){
		foreach (var id in m_audioLoading) {
			m_audioLoadingToRelease.Add (id);
		}
	}

	public void StopAllLoadedAudio(float fadeOutSeconds = 0f){
		foreach (var pair in m_audioGroupDict) {
			pair.Value.StopAllLoadedAudio (fadeOutSeconds);
		}
	}

	public void PauseAudio(int serialId, float fadeOutSeconds = 0f){
		foreach (var pair in m_audioGroupDict) {
			if (pair.Value.PauseAudio (serialId, fadeOutSeconds)) {
				break;
			}
		}
	}

	public void ResumeAudio(int serialId, float fadeInSeconds = 0f){
		foreach (var pair in m_audioGroupDict) {
			if (pair.Value.ResumeAudio (serialId, fadeInSeconds)) {
				break;
			}
		}
	}

	private void LoadAudioSuccessCallback(PlayAudioInfo info, string assetName, object asset, float duration){
		m_audioLoading.Remove (info.SerialID);
		if (m_audioLoadingToRelease.Contains (info.SerialID)) {
			Debug.LogWarning ("Release Audio" + info.SerialID.ToString () + " on Loading Success");
			m_audioLoadingToRelease.Remove (info.SerialID);
			AudioHelper.ReleaseAudioAsset (assetName, asset);
			return;
		}

		PlayAudioErrorCode? code = null;
		AudioAgent agent = info.CachedAudioGroup.PlayAudio (info.SerialID, asset, info.PlayParams, out code);
		if (!agent.IsNull ()) {
			AudioAgentHelperBase agentHelper = agent.AudioAgentHelper as AudioAgentHelperBase;
			if (!info.BindingEntity.IsNull ()) {
				agentHelper.SetBindingEntity (info.BindingEntity);
			} else {
				agentHelper.SetWorldPosition (info.WorldPosition);
			}
			AudioEvent.PlaySuccessEventArgs args = new AudioEvent.PlaySuccessEventArgs (info, assetName, agent, duration);
			if (!_playAudioSuccessCallback.IsNull ()) {
				_playAudioSuccessCallback (args);
			}
			EventPoolManager.Instance.TriggerEvent (this, args);
		} else {
			AudioHelper.ReleaseAudioAsset (assetName, asset);
			string errorMessage = string.Format ("Audio Group {0} Play Audio {1} Failure.", info.CachedAudioGroup.Name, assetName);
			AudioEvent.PlayFailureEventArgs args = new AudioEvent.PlayFailureEventArgs (info, assetName, code.Value, errorMessage);
			if (!_playAudioFailureCallback.IsNull ()) {
				_playAudioFailureCallback (args);
			}
			EventPoolManager.Instance.TriggerEvent (this, args);
		}
	}

	private void LoadAudioFailureCallback(PlayAudioInfo info, string assetName, string errorMessage){
		m_audioLoading.Remove (info.SerialID);
		m_audioLoadingToRelease.Remove (info.SerialID);
		string errmsg = string.Format ("Load Audio Failure, Asset Name: {0}, error message: {1}", assetName, errorMessage);
		AudioEvent.PlayFailureEventArgs args = new AudioEvent.PlayFailureEventArgs (info, assetName, PlayAudioErrorCode.PAEC_LOAD_ASSET_FAILURE, errmsg);
		if (!_playAudioFailureCallback.IsNull ()) {
			_playAudioFailureCallback (args);
		}
		EventPoolManager.Instance.TriggerEvent (this, args);
	}

	private void OnSceneLoaded(Scene scene, LoadSceneMode loadSceneMode){
		RefreshAudioListener ();
	}

	private void OnSceneUnloaded(Scene scene){
		RefreshAudioListener ();
	}

	private void RefreshAudioListener()
	{
		m_audioListener.enabled = FindObjectsOfType<AudioListener>().Length <= 1;
	}
}
