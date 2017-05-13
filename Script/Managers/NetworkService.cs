using UnityEngine;
using System.Collections;
using System;
using LitJson;

public class NetworkService {
	//some url to send messages
	private const string MsgUrl = "https://api.shaojishiduo.com/GameScream/";

	private string deviceID = "";

	private string getDeviceID(){
		if (deviceID == "") {
			deviceID = StringUtil.Base64Encode(DeviceID.Get());
		}
		Debug.Log("deviceId:" + deviceID);
		return deviceID;
	}

	private bool IsResponseValid(WWW www) {
		if (!www.error.IsNull()) {
			Debug.Log("bad connection");
			return false;
		}
		else if (string.IsNullOrEmpty(www.text)) {
			Debug.Log("bad data");
			return false;
		}
		else {	// all good
			return true;
		}
	}

	public IEnumerator ReqJsonWithMsg(string msgName, Hashtable args, Action<JsonData> callback) {
		WWWForm form = new WWWForm();
		//this can change
		if (!args.IsNull()) {
			form.AddField ("skey", getDeviceID());
			form.AddField ("timestamp", DateTime.UtcNow.Ticks.ToString());
			foreach(DictionaryEntry arg in args) {
				form.AddField(arg.Key.ToString(), arg.Value.ToString());
			}
		}
		WWW www = new WWW(MsgUrl + msgName, form);

		yield return www;
		
		if (!IsResponseValid(www))
			yield break;
		
		Debug.Log ("WWW: " + www.text);
		JsonData jsdArray = JsonUtils.ReadJsonString(www.text);
		if (!callback.IsNull()) {
			callback (jsdArray);
		}
	}

	public IEnumerator ReqJsonWithUrl(string url, Hashtable args, Action<JsonData> callback) {
		WWWForm form = new WWWForm();
		if (!args.IsNull()) {
			form.AddField ("skey", getDeviceID());
			form.AddField ("timestamp", DateTime.UtcNow.Ticks.ToString());
			foreach(DictionaryEntry arg in args) {
				form.AddField(arg.Key.ToString(), arg.Value.ToString());
			}
		}
		WWW www = new WWW(url, form);

		yield return www;

		if (!IsResponseValid(www))
			yield break;

		Debug.Log ("WWW: " + www.text);
		JsonData jsdArray = JsonUtils.ReadJsonString(www.text);
		if (!callback.IsNull()) {
			callback (jsdArray);
		}
	}

}
