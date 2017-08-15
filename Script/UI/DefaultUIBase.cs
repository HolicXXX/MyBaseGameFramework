using UnityEngine;
using UnityEngine.UI;
using System;
using System.Collections;

public class DefaultUIBase : UIBase
{
	public const int DepthFactor = 100;
	private const float FadeTime = 0.3f;
	private static Font s_MainFont = null;
	public static void SetMainFont(Font mainFont){
		if (mainFont.IsNull ()) {
			Debug.LogError ("Font is invalid!");
			return;
		}
		DefaultUIBase.s_MainFont = mainFont;
	}

	#region COSTUM

	private Canvas m_CachedCanvas = null;
	private CanvasGroup m_CanvasGroup = null;

	public bool IsAvailable{ get { return gameObject.activeSelf; } }
	public Transform CachedTransform{ get; private set; }
	public int OriginDepth{ get; private set; }
	public int SortingDepth{ get { return m_CachedCanvas.sortingOrder; } }

	public virtual void Close(bool ignoreAnimation){
		StopAllCoroutines ();

		if (ignoreAnimation) {
			UIManager.Instance.CloseUI (SerialID, null);	
		} else {
			this.PlayExitAnimation ();
		}
	}

	public virtual void PlayUISound(int uiSoundId){
//		AudioManager.Instance
	}

	private IEnumerator FadeAnimation(float sAlpha, float eAlpha, float duration, Action callback = null){
		m_CanvasGroup.alpha = sAlpha;
		yield return m_CanvasGroup.FadeToAlpha (eAlpha, duration);
		if (!callback.IsNull ()) {
			callback ();
		}
	}

	public virtual void PlayEnterAnimation(){
		StopAllCoroutines ();
		FadeAnimation (0f, 1f, FadeTime);
	}

	public virtual void PlayExitAnimation(){
		StopAllCoroutines ();
		FadeAnimation (1f, 0f, FadeTime);
	}

	#endregion

	#region OVERRIDE

	public override void OnInit(int serialId, string uiAssetName, IUIGroup uiGroup, bool pauseCoveredUI, bool isNewInstance,object userData){
		base.OnInit (serialId, uiAssetName, uiGroup, pauseCoveredUI, isNewInstance, userData);
		if (CachedTransform.IsNull ()) {
			CachedTransform = gameObject.transform;
		}

		m_CachedCanvas = gameObject.GetOrAddComponent<Canvas> ();
		m_CachedCanvas.overrideSorting = true;
		OriginDepth = m_CachedCanvas.sortingOrder;

		m_CanvasGroup = gameObject.GetOrAddComponent<CanvasGroup> ();

		RectTransform transform = GetComponent<RectTransform>();
		transform.anchorMin = Vector2.zero;
		transform.anchorMax = Vector2.one;
		transform.anchoredPosition = Vector2.zero;
		transform.sizeDelta = Vector2.zero;

		gameObject.GetOrAddComponent<GraphicRaycaster> ();

		Text[] texts = GetComponentsInChildren<Text>(true);
		for (int i = 0; i < texts.Length; i++)
		{
			texts[i].font = DefaultUIBase.s_MainFont;
			if (!string.IsNullOrEmpty(texts[i].text))
			{
				//TODO:Localization
				//texts[i].text = GameEntry.Localization.GetString(texts[i].text);
			}
		}
	}

	public override void OnEnter(object userData){
		base.OnEnter (userData);

		this.PlayEnterAnimation ();
	}

	public override void OnExit(object userData){
		base.OnExit (userData);
	}

	public override void OnPause(){
		base.OnPause ();
	}

	public override void OnResume(){
		base.OnResume ();
	}

	public override void OnCover(){
		base.OnCover ();
	}

	public override void OnReveal(){
		base.OnReveal ();
	}

	public override void OnFocus(object userData){
		base.OnFocus (userData);
	}

	public override void OnUpdate(float dt){
		base.OnUpdate (dt);
	}

	public override void OnDepthChanged(int uiGroupDepth, int depthInUIGroup){
		int oldDepth = SortingDepth;
		base.OnDepthChanged (uiGroupDepth, depthInUIGroup);
		int deltaDepth = DefaultUIGroupHelper.DepthFactor * uiGroupDepth + DepthFactor * depthInUIGroup - oldDepth + OriginDepth;
		Canvas[] canvases = GetComponentsInChildren<Canvas>(true);
		for (int i = 0; i < canvases.Length; i++)
		{
			canvases[i].sortingOrder += deltaDepth;
		}
	}

	#endregion
}

