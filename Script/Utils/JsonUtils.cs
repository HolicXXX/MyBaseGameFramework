using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using LitJson;

public static class JsonUtils {

	public static JsonData ReadJsonString(string jsonstring)
	{
		JsonData ret = null;
		if (jsonstring.Length != 0) {
			ret = JsonMapper.ToObject (jsonstring);
		}
		return ret;
	}

	public static string ReadJsonData(JsonData data)
	{
		string ret = "";
		if (data != null) {
			ret = JsonMapper.ToJson (data);
		}
		return ret;
	}
}
