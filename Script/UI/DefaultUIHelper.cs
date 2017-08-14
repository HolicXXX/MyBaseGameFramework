using UnityEngine;

public class DefaultUIHelper : IUIBaseHelper
{
	public object InstantiateUI (object uiAsset){
		return Object.Instantiate ((Object)uiAsset);
	}

	public IUIBase CreateUI(object uiInstance,IUIGroup uiGroup,object userData){
		GameObject obj = uiInstance as GameObject;
		if (obj.IsNull ()) {
			Debug.LogError ("UI Instance is invalid");
			return null;
		}
		Transform trans = obj.transform;
		trans.SetParent (uiGroup.Helper.transform);
		trans.localScale = Vector3.one;

		return obj.GetOrAddComponent<UIBase> ();
	}

	public void ReleaseUI(object uiAsset,object uiInstance){
		var ui = uiAsset as IUIBase;
		AssetBundleManager.Instance.UnloadAsset (ui.UIAssetName);
		if (!uiInstance.IsNull ()) {
			Object.DestroyObject ((Object)uiInstance);	
		}
	}
}

