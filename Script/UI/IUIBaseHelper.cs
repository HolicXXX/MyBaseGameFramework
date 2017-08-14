
public interface IUIBaseHelper {
	object InstantiateUI (object uiAsset);
	IUIBase CreateUI(object uiInstance,IUIGroup uiGroup,object userData);
	void ReleaseUI(object uiAsset,object uiInstance);
}
