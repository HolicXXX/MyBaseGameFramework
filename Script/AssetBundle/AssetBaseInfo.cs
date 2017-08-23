using UnityEngine;
using System.Collections;

public class AssetBaseInfo
{
	public string AssetName{ get; private set; }
	public string BundleName{ get; private set; }
	public bool IsScene{ get; private set; }
	public AssetBaseInfo(string assetName, string bundleName){
		AssetName = assetName;
		BundleName = bundleName;
		IsScene = assetName.EndsWith (".unity");
	}
}

