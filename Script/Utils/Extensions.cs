using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public static class Extensions {

	#region Base

	public static bool IsNull(this object obj){
		return obj == null;
	}

	public static T[] ArrayMinus<T>(T[] arr1, T[] arr2) where T : IComparable
	{
		List<T> ret = new List<T> ();
		for (int i = 0; i < arr1.Length; ++i) {
			T item = arr1 [i];
			bool find = false;
			for (int j = 0; j < arr2.Length; ++j) {
				if (item.CompareTo (arr2 [j]) == 0) {
					find = true;
					break;
				}
			}
			if (!find)
				ret.Add (item);
		}
		return ret.ToArray ();
	}

	#endregion

	public static T GetOrAddComponent<T>(this GameObject gameobject) where T :Component
	{
		T component = gameobject.GetComponent<T> ();
		if (component.IsNull ()) {
			component = gameobject.AddComponent<T> ();
		}
		return component;
	}

	public static Component GetOrAddComponent(this GameObject gameObject, Type type)
	{
		Component component = gameObject.GetComponent(type);
		if (component == null)
		{
			component = gameObject.AddComponent(type);
		}

		return component;
	}

	public static bool InScene(this GameObject gameObject)
	{
		return gameObject.scene.name != null;
	}

	public static Vector2 ToVector2(this Vector3 vector3)
	{
		return new Vector2(vector3.x, vector3.z);
	}

	public static Vector3 ToVector3(this Vector2 vector2)
	{
		return new Vector3(vector2.x, 0f, vector2.y);
	}

	public static Vector3 ToVector3(this Vector2 vector2, float y)
	{
		return new Vector3(vector2.x, y, vector2.y);
	}

	#region Transform

	/// <summary>
	/// 设置绝对位置的 x 坐标。
	/// </summary>
	/// <param name="transform"><see cref="UnityEngine.Transform" /> 对象。</param>
	/// <param name="newValue">x 坐标值。</param>
	public static void SetPositionX(this Transform transform, float newValue)
	{
		Vector3 v = transform.position;
		v.x = newValue;
		transform.position = v;
	}

	/// <summary>
	/// 设置绝对位置的 y 坐标。
	/// </summary>
	/// <param name="transform"><see cref="UnityEngine.Transform" /> 对象。</param>
	/// <param name="newValue">y 坐标值。</param>
	public static void SetPositionY(this Transform transform, float newValue)
	{
		Vector3 v = transform.position;
		v.y = newValue;
		transform.position = v;
	}

	/// <summary>
	/// 设置绝对位置的 z 坐标。
	/// </summary>
	/// <param name="transform"><see cref="UnityEngine.Transform" /> 对象。</param>
	/// <param name="newValue">z 坐标值。</param>
	public static void SetPositionZ(this Transform transform, float newValue)
	{
		Vector3 v = transform.position;
		v.z = newValue;
		transform.position = v;
	}

	/// <summary>
	/// 增加绝对位置的 x 坐标。
	/// </summary>
	/// <param name="transform"><see cref="UnityEngine.Transform" /> 对象。</param>
	/// <param name="deltaValue">x 坐标值增量。</param>
	public static void AddPositionX(this Transform transform, float deltaValue)
	{
		Vector3 v = transform.position;
		v.x += deltaValue;
		transform.position = v;
	}

	/// <summary>
	/// 增加绝对位置的 y 坐标。
	/// </summary>
	/// <param name="transform"><see cref="UnityEngine.Transform" /> 对象。</param>
	/// <param name="deltaValue">y 坐标值增量。</param>
	public static void AddPositionY(this Transform transform, float deltaValue)
	{
		Vector3 v = transform.position;
		v.y += deltaValue;
		transform.position = v;
	}

	/// <summary>
	/// 增加绝对位置的 z 坐标。
	/// </summary>
	/// <param name="transform"><see cref="UnityEngine.Transform" /> 对象。</param>
	/// <param name="deltaValue">z 坐标值增量。</param>
	public static void AddPositionZ(this Transform transform, float deltaValue)
	{
		Vector3 v = transform.position;
		v.z += deltaValue;
		transform.position = v;
	}

	/// <summary>
	/// 设置相对位置的 x 坐标。
	/// </summary>
	/// <param name="transform"><see cref="UnityEngine.Transform" /> 对象。</param>
	/// <param name="newValue">x 坐标值。</param>
	public static void SetLocalPositionX(this Transform transform, float newValue)
	{
		Vector3 v = transform.localPosition;
		v.x = newValue;
		transform.localPosition = v;
	}

	/// <summary>
	/// 设置相对位置的 y 坐标。
	/// </summary>
	/// <param name="transform"><see cref="UnityEngine.Transform" /> 对象。</param>
	/// <param name="newValue">y 坐标值。</param>
	public static void SetLocalPositionY(this Transform transform, float newValue)
	{
		Vector3 v = transform.localPosition;
		v.y = newValue;
		transform.localPosition = v;
	}

	/// <summary>
	/// 设置相对位置的 z 坐标。
	/// </summary>
	/// <param name="transform"><see cref="UnityEngine.Transform" /> 对象。</param>
	/// <param name="newValue">z 坐标值。</param>
	public static void SetLocalPositionZ(this Transform transform, float newValue)
	{
		Vector3 v = transform.localPosition;
		v.z = newValue;
		transform.localPosition = v;
	}

	/// <summary>
	/// 增加相对位置的 x 坐标。
	/// </summary>
	/// <param name="transform"><see cref="UnityEngine.Transform" /> 对象。</param>
	/// <param name="deltaValue">x 坐标值。</param>
	public static void AddLocalPositionX(this Transform transform, float deltaValue)
	{
		Vector3 v = transform.localPosition;
		v.x += deltaValue;
		transform.localPosition = v;
	}

	/// <summary>
	/// 增加相对位置的 y 坐标。
	/// </summary>
	/// <param name="transform"><see cref="UnityEngine.Transform" /> 对象。</param>
	/// <param name="deltaValue">y 坐标值。</param>
	public static void AddLocalPositionY(this Transform transform, float deltaValue)
	{
		Vector3 v = transform.localPosition;
		v.y += deltaValue;
		transform.localPosition = v;
	}

	/// <summary>
	/// 增加相对位置的 z 坐标。
	/// </summary>
	/// <param name="transform"><see cref="UnityEngine.Transform" /> 对象。</param>
	/// <param name="deltaValue">z 坐标值。</param>
	public static void AddLocalPositionZ(this Transform transform, float deltaValue)
	{
		Vector3 v = transform.localPosition;
		v.z += deltaValue;
		transform.localPosition = v;
	}

	/// <summary>
	/// 设置相对尺寸的 x 分量。
	/// </summary>
	/// <param name="transform"><see cref="UnityEngine.Transform" /> 对象。</param>
	/// <param name="newValue">x 分量值。</param>
	public static void SetLocalScaleX(this Transform transform, float newValue)
	{
		Vector3 v = transform.localScale;
		v.x = newValue;
		transform.localScale = v;
	}

	/// <summary>
	/// 设置相对尺寸的 y 分量。
	/// </summary>
	/// <param name="transform"><see cref="UnityEngine.Transform" /> 对象。</param>
	/// <param name="newValue">y 分量值。</param>
	public static void SetLocalScaleY(this Transform transform, float newValue)
	{
		Vector3 v = transform.localScale;
		v.y = newValue;
		transform.localScale = v;
	}

	/// <summary>
	/// 设置相对尺寸的 z 分量。
	/// </summary>
	/// <param name="transform"><see cref="UnityEngine.Transform" /> 对象。</param>
	/// <param name="newValue">z 分量值。</param>
	public static void SetLocalScaleZ(this Transform transform, float newValue)
	{
		Vector3 v = transform.localScale;
		v.z = newValue;
		transform.localScale = v;
	}

	/// <summary>
	/// 增加相对尺寸的 x 分量。
	/// </summary>
	/// <param name="transform"><see cref="UnityEngine.Transform" /> 对象。</param>
	/// <param name="deltaValue">x 分量增量。</param>
	public static void AddLocalScaleX(this Transform transform, float deltaValue)
	{
		Vector3 v = transform.localScale;
		v.x += deltaValue;
		transform.localScale = v;
	}

	/// <summary>
	/// 增加相对尺寸的 y 分量。
	/// </summary>
	/// <param name="transform"><see cref="UnityEngine.Transform" /> 对象。</param>
	/// <param name="deltaValue">y 分量增量。</param>
	public static void AddLocalScaleY(this Transform transform, float deltaValue)
	{
		Vector3 v = transform.localScale;
		v.y += deltaValue;
		transform.localScale = v;
	}

	/// <summary>
	/// 增加相对尺寸的 z 分量。
	/// </summary>
	/// <param name="transform"><see cref="UnityEngine.Transform" /> 对象。</param>
	/// <param name="deltaValue">z 分量增量。</param>
	public static void AddLocalScaleZ(this Transform transform, float deltaValue)
	{
		Vector3 v = transform.localScale;
		v.z += deltaValue;
		transform.localScale = v;
	}

	/// <summary>
	/// 二维空间下使 <see cref="UnityEngine.Transform" /> 指向指向目标点的算法，使用世界坐标。
	/// </summary>
	/// <param name="transform"><see cref="UnityEngine.Transform" /> 对象。</param>
	/// <param name="lookAtPoint2D">要朝向的二维坐标点。</param>
	/// <remarks>假定其 forward 向量为 <see cref="UnityEngine.Vector3.up" />。</remarks>
	public static void LookAt2D(this Transform transform, Vector2 lookAtPoint2D)
	{
		Vector3 vector = lookAtPoint2D.ToVector3() - transform.position;
		vector.y = 0f;

		if (vector.magnitude > 0f)
		{
			transform.rotation = Quaternion.LookRotation(vector.normalized, Vector3.up);
		}
	}

	/// <summary>
	/// 递归设置游戏对象的层次。
	/// </summary>
	/// <param name="transform"><see cref="UnityEngine.Transform" /> 对象。</param>
	/// <param name="layer">目标层次的编号。</param>
	public static void SetLayerRecursively(this Transform transform, int layer)
	{
		Transform[] transforms = transform.GetComponentsInChildren<Transform>(true);
		for (int i = 0; i < transforms.Length; i++)
		{
			transforms[i].gameObject.layer = layer;
		}
	}

	#endregion Transform

	#region UI
	public static IEnumerator FadeToAlpha(this CanvasGroup canvasGroup, float alpha, float duration)
	{
		float time = 0f;
		float originalAlpha = canvasGroup.alpha;
		while (time < duration)
		{
			time += Time.deltaTime;
			canvasGroup.alpha = Mathf.Lerp(originalAlpha, alpha, time / duration);
			yield return new WaitForEndOfFrame();
		}

		canvasGroup.alpha = alpha;
	}

	public static IEnumerator SmoothValue(this Slider slider, float value, float duration)
	{
		float time = 0f;
		float originalValue = slider.value;
		while (time < duration)
		{
			time += Time.deltaTime;
			slider.value = Mathf.Lerp(originalValue, value, time / duration);
			yield return new WaitForEndOfFrame();
		}

		slider.value = value;
	}

	#endregion

	#region Audio
	public static IEnumerator FadeToVolume(this AudioSource audioSource, float volume, float duration)
	{
		float time = 0f;
		float originalVolume = audioSource.volume;
		while (time < duration)
		{
			time += UnityEngine.Time.deltaTime;
			audioSource.volume = Mathf.Lerp(originalVolume, volume, time / duration);
			yield return new WaitForEndOfFrame();
		}

		audioSource.volume = volume;
	}
	#endregion
}
