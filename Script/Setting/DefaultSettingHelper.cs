using UnityEngine;

public class DefaultSettingHelper : ISettingHelper{

	public virtual void Save(){
		PlayerPrefs.Save ();
	}

	public virtual bool HasKey(string key){
		return PlayerPrefs.HasKey (key);
	} 

	public virtual void RemoveKey(string key){
		PlayerPrefs.DeleteKey (key);
	}

	public virtual void RemoveAllKeys(){
		PlayerPrefs.DeleteAll ();
	}

	public virtual bool GetBool(string key, bool defaultValue = true){
		return PlayerPrefs.GetInt (key, defaultValue ? 1 : 0) != 0;
	}

	public virtual void SetBool(string key, bool value){
		PlayerPrefs.SetInt (key, value ? 1 : 0);
	}

	public virtual int GetInt(string key, int defaultValue = 0){
		return PlayerPrefs.GetInt (key, defaultValue);
	}

	public virtual void SetInt(string key, int value){
		PlayerPrefs.SetInt (key, value);
	}

	public virtual float GetFloat(string key, float defaultValue = 0f){
		return PlayerPrefs.GetFloat (key, defaultValue);
	}

	public virtual void SetFloat(string key, float value){
		PlayerPrefs.SetFloat (key, value);
	}

	public virtual string GetString(string key, string defaultValue = ""){
		return PlayerPrefs.GetString (key, defaultValue);
	}

	public virtual void SetString(string key, string value){
		PlayerPrefs.SetString (key, value);
	}

	public virtual T GetObject<T>(string key, T defaultValue = default(T)){
		string json = PlayerPrefs.GetString (key, "");
		if (string.IsNullOrEmpty (json)) {
			return defaultValue;
		}
		return JsonUtils.ParseJsonToObject<T> (json);
	}

	public virtual void SetObject<T>(string key, T value){
		PlayerPrefs.SetString (key, JsonUtils.ParseObjectToJson (value));
	}

}
