using UnityEngine;

public class AudioHelperBase : MonoBehaviour, IAudioHelper
{
	public virtual void ReleaseAudioAsset (string assetName, object audioAsset){
		if (string.IsNullOrEmpty (assetName)) {
			AssetBundleManager.Instance.UnloadAsset (audioAsset);
		} else {
			AssetBundleManager.Instance.UnloadAsset (assetName);
		}
	}
}

