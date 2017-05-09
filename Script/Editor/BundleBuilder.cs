using UnityEditor;
using UnityEngine;
using System.Collections.Generic;
using System.IO;
using UnityEngine.Windows;

public class BundleBuilder {
	
	[MenuItem ("Custom Menu/Build Assetbundle")]
	static private void BuildAssetBundle() {
		BuildPipeline.BuildAssetBundles (Application.streamingAssetsPath, BuildAssetBundleOptions.None, GetBuildTarget ());
	}

	static private BuildTarget GetBuildTarget() {
		BuildTarget target;
		#if UNITY_IPHONE
		target = BuildTarget.iOS;
		#elif UNITY_ANDROID
		target = BuildTarget.Android;
		#elif UNITY_STANDALONE_OSX
		target = BuildTarget.StandaloneOSXUniversal;
		#elif UNITY_STANDALONE
		target = BuildTarget.StandaloneWindows;
		#else
		tartet = BuildTarget.NoTarget;
		#endif
		return target;
	}
}