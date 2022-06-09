using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public static class Extend
{
	/// <summary>
	/// Vector2のxとyを入れ替える(y,x)
	/// </summary>
	/// <param name="vector">Vector2</param>
	/// <returns></returns>
	public static Vector2Int Flip(this Vector2Int vector)
	{
		return new Vector2Int(vector.y, vector.x);
	}

	/// <summary>
	/// Vector3をfloat配列に変える
	/// </summary>
	/// <param name="v">Vector3</param>
	/// <returns></returns>
	public static float[] Array(this Vector3 v)
	{
		float[] f = new float[3];
		f[0] = v.x;
		f[1] = v.y;
		f[2] = v.z;

		return f;
	}

	/// <summary>
	/// Vector3の中身が-1の部分を0,90,180,270に変更
	/// </summary>
	/// <param name="v">Vector3</param>
	/// <returns></returns>
	public static Vector3 AddOffset(this Vector3 v)
	{
		float[] f = Array(v);
		for (int i = 0; i < 3; i++)
		{
			if (f[i] == -1)
			{
				f[i] = IRandom(i == 1 ? 45 : 0);
			}
		}

		return new Vector3(f[0], f[1], f[2]);
	}

	/// <summary>
	/// 0,90,180,270のいずれかを返す
	/// </summary>
	/// <param name="offset">追加角度</param>
	/// <returns></returns>
	public static float IRandom(float offset)
	{
		float n = Random.Range(0, 4) * 90 + offset;
		return n;
	}

	/// <summary>
	/// bが最大値のaが現在値の時の0～1の割合を返す
	/// </summary>
	/// <param name="a">現在値</param>
	/// <param name="b">最大値</param>
	/// <returns></returns>
	public static float TwoRatio(float a,float b)
	{
		if (a == 0 || b == 0) return 0;
		float n = a / b;
		if (n > 1) return 1;
		else if (n < 0) return 0;
		return n;
	}

	public static Color ColorMultiple(this Color a, Color b)
	{
		a.r = Mathf.Clamp(a.r * b.r, 0.0f, 1.0f);
		a.g = Mathf.Clamp(a.g * b.g, 0.0f, 1.0f);
		a.b = Mathf.Clamp(a.b * b.b, 0.0f, 1.0f);
		a.a = Mathf.Clamp(a.a * b.a, 0.0f, 1.0f);

		return a;
	}

	public static Color ColorA(this Color color, float a)
	{
		color.a = a;
		return color;
	}

	public static Vector3 RectToWorld(this RectTransform rect)
	{
		Vector3 screenPos = RectTransformUtility.WorldToScreenPoint(Camera.main, rect.position);
		Vector3 result = Vector3.zero;
		RectTransformUtility.ScreenPointToWorldPointInRectangle(rect, screenPos, Camera.main, out result);

		return result;
	}

	public static float Value2decibel(this float value)
	{
		float v = Mathf.Clamp(value, 0.0001f, 1.0f);
		float v2 = (float)(20.0d * Mathf.Log10(v));
		float v3 = Mathf.Clamp(v2, -80.0f, 0.0f);
		return v3;
	}
}