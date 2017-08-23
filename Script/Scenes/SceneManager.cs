using UnityEngine;
using UnityEngine.SceneManagement;
using System;
using System.IO;
using System.Collections;
using System.Collections.Generic;

public class SceneLoadManager : Singleton<SceneLoadManager>
{
	private Dictionary<string, SceneInfo> m_sceneInfoDict;
	private List<string> m_sceneLoaded;
	private List<string> m_sceneLoading;
	private List<string> m_sceneUnloading;
	private SceneInfo m_currentScene;

	public bool ConfigPreloaded{ get; set; }

	public event Action<SceneEvent.LoadSuccessEventArgs> LoadSceneSuccessHandler;
	public event Action<SceneEvent.LoadUpdateEventArgs> LoadSceneUpdateHandler;
	public event Action<SceneEvent.LoadFailureEventArgs> LoadSceneFailureHandler;
	public event Action<SceneEvent.UnloadSuccessEventArgs> UnloadSceneSuccessHandler;
	public event Action<SceneEvent.UnloadFailureEventArgs> UnloadSceneFailureHandler;

	void Awake() {
		m_sceneInfoDict = new Dictionary<string, SceneInfo> ();
		m_sceneLoaded = new List<string> ();
		m_sceneLoading = new List<string> ();
		m_sceneUnloading = new List<string> ();
		m_currentScene = null;

		ConfigPreloaded = false;

		LoadSceneSuccessHandler = null;
		LoadSceneUpdateHandler = null;
		LoadSceneFailureHandler = null;
		UnloadSceneSuccessHandler = null;
		UnloadSceneFailureHandler = null;
	}

	void Start () {
		CoroutineUtils.WaitUntil (() => AssetBundleManager.Instance.ConfigPreLoaded, () => {
			string[] bundles = AssetBundleManager.Instance.GetScenesAssetBundleName();
			InitDictionary(bundles);
		});
	}

	private void InitDictionary(string[] bundles){
		for (int i = 0; i < bundles.Length; ++i) {
			string bundle = bundles [i];
			string[] scenes = AssetBundleManager.Instance.GetAssetListByBundleNameAndVariant (bundle);
			for (int j = 0; j < scenes.Length; ++j) {
				SceneInfo info = new SceneInfo (scenes [j], bundle);
				m_sceneInfoDict.Add (scenes [j], info);
			}
		}
		ConfigPreloaded = true;
	}

	public void ShutDown() {
		foreach (string loaded in m_sceneLoaded) {
			if (IsSceneUnloading (loaded)) {
				continue;
			}
			UnloadScene (loaded, null);
		}
		m_sceneLoaded.Clear ();
		m_sceneLoading.Clear ();
		m_sceneUnloading.Clear ();
	}

	void Update () {
	}

	protected override void OnDestroy() {
		base.OnDestroy ();
	}

	public bool IsSceneLoaded(string sceneAssetName){
		return !string.IsNullOrEmpty (sceneAssetName) && m_sceneLoaded.Contains (sceneAssetName);
	}

	public string[] GetLoadedSceneAssetName() {
		return m_sceneLoaded.ToArray ();
	}

	public bool IsSceneLoading(string sceneAsstName){
		return !string.IsNullOrEmpty (sceneAsstName) && m_sceneLoading.Contains (sceneAsstName);
	}

	public string[] GetLoadingSceneAssetName() {
		return m_sceneLoading.ToArray ();
	}

	public bool IsSceneUnloading(string sceneAssetName){
		return !string.IsNullOrEmpty (sceneAssetName) && m_sceneUnloading.Contains (sceneAssetName);
	}

	public string[] GetUnloadingSceneAssetName() {
		return m_sceneUnloading.ToArray ();
	}

	public void LoadScene(string sceneAssetName, object userData){
		SceneInfo info = m_sceneInfoDict [sceneAssetName];
		float duration = 0f;
		m_sceneLoading.Add (sceneAssetName);
		AssetBundleManager.Instance.AddFromFileTask (info.SceneBundleName, args => {
			duration += Time.deltaTime;
			LoadUpdateCallback(info.SceneAssetName,args.Progress / 2f,userData);
		}, args => {
			AssetBundleManager.Instance.AddAssetTask(info.SceneBundleName,info.SceneAssetName,sargs=>{
				duration += Time.deltaTime;
				LoadUpdateCallback(info.SceneAssetName,sargs.Progress / 2f + 0.5f,userData);
			},sargs=>{
				LoadSuccessCallback(info.SceneAssetName,duration,userData);
			},sargs=>{
				LoadFailureCallback(info.SceneAssetName,sargs.Message,userData);
			});
		}, args => {
			LoadFailureCallback(info.SceneAssetName,args.Message,userData);
		});
	}

	public void UnloadScene(string sceneAssetName, object userData){
		SceneInfo info = m_sceneInfoDict [sceneAssetName];
		string[] depToRelease = info.SceneDependency;
		string[] depNotRelease = m_currentScene.SceneDependency;
		string[] releases = Extensions.ArrayMinus (depToRelease, depNotRelease);
		for (int i = 0; i < releases.Length; ++i) {
			AssetBundleManager.Instance.UnloadAssetBundle (releases [i], true);
		}
		AssetBundleManager.Instance.UnloadAssetBundle (info.SceneBundleName, true);
		m_sceneUnloading.Add (sceneAssetName);
		CoroutineManager.Instance.StartNewCoroutineTask (UnloadLastScene (info, userData));
	}

	private IEnumerator UnloadLastScene(SceneInfo info, object userData){
		var asyncOpe = SceneManager.UnloadSceneAsync (info.SceneName);
		if (asyncOpe.IsNull ()) {
			UnloadFailureCallback (info.SceneAssetName, "UnloadSceneAsyncOperation is Null", userData);
			yield break;
		}
		yield return asyncOpe;
		UnloadSuccessCallback (info.SceneAssetName, userData);
	}

	private void LoadSuccessCallback(string sceneAssetName, float duration, object userData){
		
		Scene nextScene = SceneManager.GetSceneByName (Path.GetFileNameWithoutExtension (sceneAssetName));
		m_sceneLoading.Remove (sceneAssetName);
		if (nextScene.IsNull () || !nextScene.IsValid()) {
			Debug.LogError ("Scene " + sceneAssetName + " is invalid");
			return;
		}
		m_sceneLoaded.Add (sceneAssetName);
		SceneManager.SetActiveScene (nextScene);

		var eventArgs = new SceneEvent.LoadSuccessEventArgs (sceneAssetName, duration, userData);
		if (!LoadSceneSuccessHandler.IsNull ()) {
			LoadSceneSuccessHandler (eventArgs);
		}
		EventPoolManager.Instance.TriggerEvent (this, eventArgs);
	}

	private void LoadUpdateCallback(string sceneAssetName, float progress, object userData){
		var eventArgs = new SceneEvent.LoadUpdateEventArgs (sceneAssetName, progress, userData);
		if (!LoadSceneUpdateHandler.IsNull ()) {
			LoadSceneUpdateHandler (eventArgs);
		}
		EventPoolManager.Instance.TriggerEvent (this, eventArgs);
	}

	private void LoadFailureCallback(string sceneAssetName, string errorMessage, object userData){
		m_sceneLoading.Remove (sceneAssetName);
		var eventArgs = new SceneEvent.LoadFailureEventArgs (sceneAssetName, errorMessage, userData);
		if (!LoadSceneFailureHandler.IsNull ()) {
			LoadSceneFailureHandler (eventArgs);
		}
		EventPoolManager.Instance.TriggerEvent (this, eventArgs);
	}

	private void UnloadSuccessCallback(string sceneAssetName, object userData){
		m_sceneLoaded.Remove (sceneAssetName);
		m_sceneUnloading.Remove (sceneAssetName);
		var eventArgs = new SceneEvent.UnloadSuccessEventArgs (sceneAssetName, userData);
		if (!UnloadSceneSuccessHandler.IsNull ()) {
			UnloadSceneSuccessHandler (eventArgs);
		}
		EventPoolManager.Instance.TriggerEvent (this, eventArgs);
	}

	private void UnloadFailureCallback(string sceneAssetName, string errorMessage, object userData){
		m_sceneUnloading.Remove (sceneAssetName);
		var eventArgs = new SceneEvent.UnloadFailureEventArgs (sceneAssetName, errorMessage, userData);
		if (!UnloadSceneFailureHandler.IsNull ()) {
			UnloadSceneFailureHandler (eventArgs);
		}
		EventPoolManager.Instance.TriggerEvent (this, eventArgs);
	}

}

