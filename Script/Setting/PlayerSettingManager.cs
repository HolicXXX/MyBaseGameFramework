using UnityEngine;
using System.Collections;

public class PlayerSettingManager : Singleton<PlayerSettingManager>
{

	public ISettingHelper SettingHelper{ get; set; }

	public void Save(){
		SettingHelper.Save ();
	}

	public bool HasKey(string key){
		return SettingHelper.HasKey (key);
	}

	public void RemoveKey(string key){
		SettingHelper.RemoveKey (key);
	}

	public void RemoveAllKeys(){
		SettingHelper.RemoveAllKeys ();
	}

	public bool GetBool(string key, bool defaultValue = true){
		return SettingHelper.GetBool (key, defaultValue);
	}

	public void SetBool(string key, bool value){
		SettingHelper.SetBool (key, value);
	}

	public int GetInt(string key, int defaultValue = 0){
		return SettingHelper.GetInt (key, defaultValue);
	}

	public void SetInt(string key, int value){
		SettingHelper.SetInt (key, value);
	}

	public float GetFloat(string key, float defaultValue = 0f){
		return SettingHelper.GetFloat (key, defaultValue);
	}

	public void SetFloat(string key, float value){
		SettingHelper.SetFloat (key, value);
	}

	public string GetString(string key, string defaultValue = ""){
		return SettingHelper.GetString (key, defaultValue);
	}

	public void SetString(string key, string value){
		SettingHelper.SetString (key, value);
	}

	public T GetObject<T>(string key, T defaultValue = default(T)){
		return SettingHelper.GetObject (key, defaultValue);
	}

	public void SetObject<T>(string key, T value){
		SettingHelper.SetObject (key, value);
	}

	protected override void OnDestroy(){
		SettingHelper.Save ();
		base.OnDestroy ();
	}

}

