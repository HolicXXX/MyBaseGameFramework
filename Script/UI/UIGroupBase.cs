using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class UIGroupBase : IUIGroup
{
	public string Name{ get; private set; }
	public int Depth{ 
		get{ return this._depth;} 
		private set{ 
			if (this._depth == value)
				return;
			this._depth = value;
			this.Helper.SetDepth (value);
			this.Refresh ();
		} 
	}
	public int UICount{ get{ return m_uiInfos.Count; } }
	public IUIBase CurrentUI{ get; private set; }
	public IUIGroupHelper Helper{ get; private set; }

	private int _depth;
	private LinkedList<IUIBaseInfo> m_uiInfos;

	public UIGroupBase(string name, int depth, IUIGroupHelper uiGroupHelper){
		if (string.IsNullOrEmpty (name)) {
			Debug.LogError ("UI Group name is invalide. ");
		}
		if (uiGroupHelper.IsNull ()) {
			Debug.LogError("UI Group Helper is invalide. ");
		}
		Name = name;
		Helper = uiGroupHelper;
		m_uiInfos = new LinkedList<IUIBaseInfo> ();
		Depth = depth;
	}

	public void Update(float dt){
		var current = m_uiInfos.First;
		while (current != null) {
			if (current.Value.Paused)
				break;
			current.Value.UIInstance.OnUpdate (dt);
			current = current.Next;
		}
	}

	public bool HasUI(int serialId){
		foreach (var info in m_uiInfos) {
			if (info.UIInstance.SerialID == serialId) {
				return true;
			}
		}
		return false;
	}

	public bool HasUI (string uiAssetName){
		if (string.IsNullOrEmpty (uiAssetName)) {
			Debug.LogError ("Invalid UI Asset Name. ");
			return false;
		}
		foreach (var info in m_uiInfos) {
			if (info.UIInstance.UIAssetName == uiAssetName) {
				return true;
			}
		}
		return false;
	}

	public IUIBase GetUI (int serialId){
		foreach (var info in m_uiInfos) {
			if (info.UIInstance.SerialID == serialId) {
				return info.UIInstance;
			}
		}
		return null;
	}

	public IUIBase GetUI (string uiAssetName){
		if (string.IsNullOrEmpty (uiAssetName)) {
			Debug.LogError ("Invalid UI Asset Name. ");
			return null;
		}
		foreach (var info in m_uiInfos) {
			if (info.UIInstance.UIAssetName == uiAssetName) {
				return info.UIInstance;
			}
		}
		return null;
	}

	public IUIBase[] GetUIs (string uiAssetName){
		List<IUIBase> ret = new List<IUIBase> ();
		foreach (var info in m_uiInfos) {
			if (info.UIInstance.UIAssetName == uiAssetName) {
				ret.Add (info.UIInstance);
			}
		}
		return ret.ToArray();
	}

	public IUIBase[] GetAllUIs(){
		List<IUIBase> ret = new List<IUIBase> ();
		foreach (var info in m_uiInfos) {
			ret.Add (info.UIInstance);
		}
		return ret.ToArray();
	}

	public void AddUI(IUIBase ui){
		IUIBaseInfo info = new IUIBaseInfo (ui);
		m_uiInfos.AddFirst (info);
	}

	public void RemoveUI(IUIBase ui){
		IUIBaseInfo info = this.GetUIInfo (ui);
		if (info == null) {
			Debug.LogErrorFormat ("Can't Find UI for SerialID:{0} UIAssetName:{1}", ui.SerialID, ui.UIAssetName);
			return;
		}
		if(!info.Covered){
			info.Covered = true;
			ui.OnCover ();
		}
		if(!info.Paused){
			info.Paused = true;
			ui.OnPause ();
		}
		m_uiInfos.Remove (info);
	}

	public void RefocusUI (IUIBase ui, object userData){
		IUIBaseInfo info = this.GetUIInfo (ui);
		if (info == null) {
			Debug.LogErrorFormat ("Can't Find UI for SerialID:{0} UIAssetName:{1}", ui.SerialID, ui.UIAssetName);
			return;
		}
		m_uiInfos.Remove (info);
		m_uiInfos.AddFirst (info);
	}

	public void Refresh (){
		LinkedListNode<IUIBaseInfo> current = m_uiInfos.First;
		bool pause = false;
		bool cover = false;
		int depth = UICount;
		while (current != null) {
			current.Value.UIInstance.OnDepthChanged (Depth, depth--);
			if (pause) {
				if (!current.Value.Covered) {
					current.Value.Covered = true;
					current.Value.UIInstance.OnCover ();
				}
				if (!current.Value.Paused) {
					current.Value.Paused = true;
					current.Value.UIInstance.OnPause ();
				}
			} else {
				if (current.Value.Paused) {
					current.Value.Paused = false;
					current.Value.UIInstance.OnResume ();
				}
				if (current.Value.UIInstance.PauseCoveredUI) {
					pause = true;
				}

				if (cover) {
					if(!current.Value.Covered){
						current.Value.Covered = true;
						current.Value.UIInstance.OnCover ();
					}
				} else {
					if (current.Value.Covered) {
						current.Value.Covered = false;
						current.Value.UIInstance.OnReveal ();
					}

					cover = true;
				}
			}

			current = current.Next;
		}
	}

	public IUIBaseInfo GetUIInfo(IUIBase ui)
	{
		if (ui == null)
		{
			Debug.LogError ("Find UI with Invalid ui. ");
			return null;
		}

		foreach (IUIBaseInfo uiInfo in m_uiInfos)
		{
			if (uiInfo.UIInstance == ui)
			{
				return uiInfo;
			}
		}

		return null;
	}
}

