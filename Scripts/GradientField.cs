using System;
using System.Collections.Generic;
using UnityEngine;

using System.Linq;

[Serializable]
public class GradientField
{
	private delegate float GetFloat();
	public enum GradientRepeatMode {
		Clamped,
		Repeat,
		PingPong
	}

	#region Getters and Setters
	public GradientMode gradientMode { get; set; }

	public GradientColorKey[] colorKeys {
		get {
			return m_colorKeys;
		}
		set {
			if(value == null || value.Length == 0) {
				return;
			}

			if(value.Length == 1) {
				m_colorKeys = new GradientColorKey[] {
					new GradientColorKey(value[0].color, 0),
					new GradientColorKey(value[0].color, 1)
				};
			} else {
				m_colorKeys = value;
			}
		}
	}
	public GradientAlphaKey[] alphaKeys {
		get {
			return m_alphaKeys;
		}
		set {
			if (value == null || value.Length == 0) {
				return;
			}

			if (value.Length == 1) {
				m_alphaKeys = new GradientAlphaKey[] {
					new GradientAlphaKey(value[0].alpha, 0),
					new GradientAlphaKey(value[0].alpha, 1)
				};
			} else {
				m_alphaKeys = value;
			}
		}
	}
	#endregion

	[SerializeField, HideInInspector]
	public GradientColorKey[] m_colorKeys = new GradientColorKey[2] {
		new GradientColorKey(Color.white, 0),
		new GradientColorKey(Color.white, 1)
	};

	[SerializeField, HideInInspector]
	public GradientAlphaKey[] m_alphaKeys = new GradientAlphaKey[2] {
		new GradientAlphaKey(1,0),
		new GradientAlphaKey(1,1)
	};
	
	public Color Evaluate(float time, GradientRepeatMode repeatMode = GradientRepeatMode.Clamped) {
		time = NormalizeTime(time, repeatMode);

		Color result = GetColor(time);
		result.a = GetAlpha(time);

		return result;
	}

	private Color GetColor(float time) {
		GradientColorKey prev = GetPreviousKey(m_colorKeys, time, c => c.time);
		GradientColorKey next = GetNextKey(m_colorKeys, time, c => c.time);

		return Color.Lerp(prev.color, next.color, Mathf.InverseLerp(prev.time, next.time, time));
	}
	private float GetAlpha(float time) {
		GradientAlphaKey prev = GetPreviousKey(m_alphaKeys, time, c => c.time);
		GradientAlphaKey next = GetNextKey(m_alphaKeys, time, c => c.time);

		return Mathf.Lerp(prev.alpha, next.alpha, Mathf.InverseLerp(prev.time, next.time, time));
	}

	private T GetPreviousKey<T>(T[] array, float time, Func<T, float> getTime) {
		int index = -1;
		for(int i = 0; i < array.Length; i++) {
			float val = getTime(array[i]);
			if (val > time) {
				if(index >= 0) {
					return array[index];
				} else {
					return array[i];
				}
			} else {
				index = i;
			}
		}
		return array[index];
	}

	private T GetNextKey<T>(T[] array, float time, Func<T, float> getTime) {
		int index = -1;
		for (int i = array.Length - 1; i >= 0; i--) {
			float val = getTime(array[i]);
			if (val < time) {
				if (index >= 0) {
					return array[index];
				} else {
					return array[i];
				}
			} else {
				index = i;
			}
		}
		return array[index];
	}

	private float NormalizeTime(float time, GradientRepeatMode repeatMode) {
		switch (repeatMode) {
			case GradientRepeatMode.Clamped:
				return Mathf.Clamp01(time);
			case GradientRepeatMode.Repeat:
				return Mathf.Repeat(time, 1);
			case GradientRepeatMode.PingPong:
				return Mathf.PingPong(time, 1);
		}
		return 0;
	}
}

