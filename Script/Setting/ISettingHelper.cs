using System;

public interface ISettingHelper
{
	void Save();
	bool HasKey(string key);
	void RemoveKey(string key);
	void RemoveAllKeys();
	bool GetBool(string key, bool defaultValue = true);
	void SetBool(string key, bool value);
	int GetInt(string key, int defaultValue = 0);
	void SetInt(string key, int value);
	float GetFloat(string key, float defaultValue = 0f);
	void SetFloat(string key, float value);
	string GetString(string key, string defaultValue = "");
	void SetString(string key, string value);
	T GetObject<T>(string key, T defaultValue = default(T));
	void SetObject<T>(string key, T value);
}
