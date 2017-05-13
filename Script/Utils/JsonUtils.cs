
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
		if (!data.IsNull()) {
			ret = JsonMapper.ToJson (data);
		}
		return ret;
	}

	public static T ParseJsonToObject<T>(string js){
		return JsonUtility.FromJson<T> (js);;
	}

	public static string ParseObjectToJson(object obj){
		string ret = "";
		if (!obj.IsNull()) {
			ret = JsonUtility.ToJson (obj);
		}
		return ret;
	}

}
