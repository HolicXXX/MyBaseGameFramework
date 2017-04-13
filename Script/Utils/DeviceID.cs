using UnityEngine;
using System.Collections;
using System.Runtime.InteropServices;

public class DeviceID{
    [DllImport("__Internal")]
    private static extern string GetIphoneADID();

    public static string Get() {
		#if UNITY_ANDROID || UNITY_EDITOR
//        AndroidJavaClass unityA = new AndroidJavaClass("com.unity3d.player.UnityPlayer");
//        AndroidJavaObject curA = unityA.GetStatic<AndroidJavaObject>("currentActivity");
//        AndroidJavaClass AndroidID = new AndroidJavaClass("com.yang.GetAID.AndroidID");
//        string aID = AndroidID.CallStatic<string>("GetID",curA);
		string aID = SystemInfo.deviceUniqueIdentifier;
        return "ANDROID-" + aID;
		#elif UNITY_IPHONE  
        string iID = GetIphoneADID();
        return "IOS-" + iID;
		#endif

//        return "isnot in mobilePhone";
    }
}
