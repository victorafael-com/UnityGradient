using System;
using System.Collections.Generic;
using UnityEngine;

using System.Linq;

[Serializable]
public class GradientField
{
	#region Serializable Classes

	//I Don't know how Unity serializes the color and gradients. so I'm making those complementary classes to store the data.

	[System.Serializable]
	public class SerializableColorKey {
		public Color color;
		public float time;

		public SerializableColorKey(Color c, float t) {
			color = c;
			time = t;
		}

		public static GradientColorKey[] GetKeys(SerializableColorKey[] serialized) {
			GradientColorKey[] result = new GradientColorKey[serialized.Length];
			for (int i = 0; i < serialized.Length; i++) {
				result[i] = new GradientColorKey(serialized[i].color, serialized[i].time / 100);
			}
			return result;
		}
	}
	[System.Serializable]
	public class SerializableAlphaKey {
		[Range(0,1)]
		public float alpha;
		public float time;

		public SerializableAlphaKey(float a, float t) {
			alpha = a;
			time = t;
		}

		public static GradientAlphaKey[] GetKeys(SerializableAlphaKey[] serialized) {
			GradientAlphaKey[] result = new GradientAlphaKey[serialized.Length];
			for (int i = 0; i < serialized.Length; i++) {
				result[i] = new GradientAlphaKey(serialized[i].alpha, serialized[i].time / 100);
			}
			return result;
		}
	}
	#endregion
	
	#region Getters and Setters
	public GradientMode mode {
		get => m_mode;
		set => m_mode = value;
	}
	public GradientColorKey[] colorKeys {
		get {
			if(m_colorKeys == null) {
				m_colorKeys = SerializableColorKey.GetKeys(m_serializedColorKeys);
			}
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
			if(m_alphaKeys == null) {
				m_alphaKeys = SerializableAlphaKey.GetKeys(m_serializedAlphaKeys);
			}
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

	#region Variables
	[SerializeField, HideInInspector]
	private GradientMode m_mode;

	private GradientColorKey[] m_colorKeys;
	private GradientAlphaKey[] m_alphaKeys;

	[SerializeField, HideInInspector]
	private SerializableColorKey[] m_serializedColorKeys = new SerializableColorKey[2] {
		new SerializableColorKey(Color.white, 0),
		new SerializableColorKey(Color.white, 100)
	};

	[SerializeField, HideInInspector]
	private SerializableAlphaKey[] m_serializedAlphaKeys = new SerializableAlphaKey[2] {
		new SerializableAlphaKey(1,0),
		new SerializableAlphaKey(1,100)
	};
	#endregion

	public Color Evaluate(float time, GradientRepeatMode repeatMode = GradientRepeatMode.Clamped) {
		time = NormalizeTime(time, repeatMode);

		Color result = GetColor(time);
		result.a = GetAlpha(time);

		return result;
	}

	private Color GetColor(float time) {
		GradientColorKey next = GetNextKey(colorKeys, time, c => c.time);

		if (mode == GradientMode.Fixed)
			return next.color;

		GradientColorKey prev = GetPreviousKey(colorKeys, time, c => c.time);

		return Color.Lerp(prev.color, next.color, Mathf.InverseLerp(prev.time, next.time, time));
	}

	private float GetAlpha(float time) {
		GradientAlphaKey next = GetNextKey(alphaKeys, time, c => c.time);

		if (mode == GradientMode.Fixed)
			return next.alpha;
		
		GradientAlphaKey prev = GetPreviousKey(alphaKeys, time, c => c.time);

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

