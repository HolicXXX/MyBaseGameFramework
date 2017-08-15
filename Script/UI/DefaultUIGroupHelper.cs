using UnityEngine;
using UnityEngine.UI;

public class DefaultUIGroupHelper : IUIGroupHelper
{
	public const int DepthFactor = 10000;
	private int m_depth = 0;
	private Canvas m_cachedCanvas;

	public override void SetDepth (int depth){
		m_depth = depth;

		m_cachedCanvas.overrideSorting = true;
		m_cachedCanvas.sortingOrder = DepthFactor * depth;
	}

	void Awake(){
		m_cachedCanvas = gameObject.GetOrAddComponent<Canvas> ();
		gameObject.GetOrAddComponent<GraphicRaycaster> ();
	}

	void Start(){
		m_cachedCanvas.renderMode = RenderMode.ScreenSpaceOverlay;
		m_cachedCanvas.overrideSorting = true;
		m_cachedCanvas.sortingOrder = DepthFactor * m_depth;

		RectTransform rectTransform = GetComponent<RectTransform> ();
		rectTransform.anchorMin = Vector2.zero;
		rectTransform.anchorMax = Vector2.one;
		rectTransform.anchoredPosition = Vector2.zero;
		rectTransform.sizeDelta = Vector2.zero;
	}
}

