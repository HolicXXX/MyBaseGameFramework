using UnityEngine;

/// <summary>
/// Entity info.
/// </summary>
public class EntityInfo {
	public string Name{ get; private set; }
	public string BundleName{ get; private set; }
	public GameObject Entity{ get; private set; }
	public EntityInfo(string name,string bn,GameObject e){
		Name = name;
		BundleName = bn;
		Entity = e;
	}

	public void ReleaseInfo(){
		Name = BundleName = "";
		Entity = null;
	}
}
