using UnityEngine;
using System;
using System.IO;
using System.Collections;

public static class AssetBundleUtility
{
	public static string GetCurrentBundlePath () {
		string combine = null;
		switch (Application.platform) {
		case RuntimePlatform.IPhonePlayer:
			combine = "iOS/";
			break;
		case RuntimePlatform.OSXPlayer:
			combine = "OSX/";
			break;
		case RuntimePlatform.Android:
			combine = "Android/";
			break;
		case RuntimePlatform.WindowsPlayer:
			combine = "Windows/";
			break;
		default:
			#if DEBUG
			combine = "OSX/";
			#else
			combine = Application.platform.ToString() + "/";
			#endif
			break;
		}
		return Path.Combine (Application.streamingAssetsPath, combine);
	}

	public static string GetCurrentManifestBundleName () {
		switch (Application.platform) {
		case RuntimePlatform.IPhonePlayer:
			return "iOS";
		case RuntimePlatform.OSXPlayer:
			return "OSX";
		case RuntimePlatform.Android:
			return "Android";
		case RuntimePlatform.WindowsPlayer:
			return "Windows";
		default:
			#if DEBUG
			return "OSX";
			#else
			return Application.platform.ToString ();
			#endif
		}
	}
}

