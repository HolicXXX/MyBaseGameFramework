using UnityEngine;
using UnityEditor;
using System.IO;
using System.Xml;
using System.Collections;

public class BuildABTargetInfo
{
	public static string AB_Root_Path = null;

	public BuildTarget Target{ get; }
	public string BuildPath{ get; }
	public AssetBundleManifest Manifest{ get; set; }

	public BuildABTargetInfo (BuildTarget target) {
		Target = target;
		BuildPath = GetBuildPath ();
	}

	private string GetBuildPath(){
		if (AB_Root_Path == null) {
			AB_Root_Path = Application.streamingAssetsPath;
		}
		string combineDir = null;
		switch (Target) {
		case BuildTarget.iOS:
			combineDir = "iOS/";
			break;
		case BuildTarget.Android:
			combineDir = "Android/";
			break;
		case BuildTarget.StandaloneOSXUniversal:
			combineDir = "OSX/";
			break;
		case BuildTarget.StandaloneWindows:
			combineDir = "Windows/";
			break;
		default:
			combineDir = Target.ToString() + "/";
			break;
		}
		combineDir = Path.Combine (AB_Root_Path, combineDir);
		if (!Directory.Exists (combineDir)) {
			Directory.CreateDirectory (combineDir);
		}
		return combineDir;
	}
}

