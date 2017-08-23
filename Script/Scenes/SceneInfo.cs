using UnityEngine;
using System.IO;
using System.Collections;

public class SceneInfo
{
	public string SceneName{ get; private set; }
	public string SceneBundleName{ get; private set; }
	public string SceneAssetName{ get; private set; }
	public string[] SceneDependency{ get; private set; }

	public SceneInfo(string sceneAssetName, string bundleName){
		SceneBundleName = bundleName;
		SceneAssetName = sceneAssetName;
		SceneName = Path.GetFileNameWithoutExtension (sceneAssetName);
		SceneDependency = AssetBundleManager.Instance.GetBundleDependency (bundleName);
	}
}

