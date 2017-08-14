
public interface IUIGroup {
	string Name{ get; }
	int Depth{ get; }
	int UICount{ get; }
	IUIBase CurrentUI{ get; }
	IUIGroupHelper Helper{ get; }
	void Update(float dt);
	bool HasUI(int serialId);
	bool HasUI (string uiAssetName);
	IUIBase GetUI (int serialId);
	IUIBase GetUI (string uiAssetName);
	IUIBase[] GetUIs (string uiAssetName);
	IUIBase[] GetAllUIs();
	void AddUI(IUIBase ui);
	void RemoveUI (IUIBase ui);
	void RefocusUI (IUIBase ui, object userData);
	void Refresh();
	IUIBaseInfo GetUIInfo (IUIBase ui);
}
