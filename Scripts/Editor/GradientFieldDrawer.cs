using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[CustomPropertyDrawer(typeof(GradientField))]
public class GradientFieldDrawer : PropertyDrawer {
	const int GradientSize = 200;

	private static Texture2D m_checkerTexture;
	private static Texture2D m_inspectorBackground;

	private static Texture2D processedGradient;

	public override float GetPropertyHeight(SerializedProperty property, GUIContent label) {
		return EditorGUIUtility.singleLineHeight;
	}

	public override void OnGUI(Rect position, SerializedProperty property, GUIContent label) {
		Rect rect = EditorGUI.PrefixLabel(position, label);

		CheckTextureGradient(rect.width, property);

		GUI.color = new Color(0.4f, 0.4f, 0.4f);
		var content = new GUIContent(Texture2D.whiteTexture);

		if (GUI.Button(rect, content)) {
			OpenWindow(property);
		}

		GUI.color = Color.white;

		rect.x += 1;
		rect.y += 1;
		rect.width -= 2;
		rect.height -= 2;

		GUI.DrawTexture(rect, GetInspectorBackground());
		GUI.DrawTexture(rect, processedGradient);
	}

	void OpenWindow(SerializedProperty property) {
		GradientField gradient = (GradientField)fieldInfo.GetValue(property.serializedObject.targetObject);

		GradientFieldEditorWindow window = ScriptableObject.CreateInstance<GradientFieldEditorWindow>();
		window.Setup(property);
		window.minSize = new Vector2(550, 240);
		window.titleContent = new GUIContent("Gradient Editor");

		window.ShowAuxWindow();
	}

	#region Gradient Texture

	internal static void ResetGradient() {
		processedGradient = null;
		
	}

	private void CheckTextureGradient(float width, SerializedProperty property) {
		int textureWidth = GradientSize;

		if (processedGradient != null && processedGradient.width == textureWidth) {
			return;
		}

		if (Event.current.type != EventType.Repaint)
			return;

		UpdateTextureGradient(textureWidth, property);
	}

	private void UpdateTextureGradient(int width, SerializedProperty property) {
		if (processedGradient == null || processedGradient.width != width)
			processedGradient = new Texture2D(width, 1);

		var gradient = GenerateGradient(property);

		SetGradientToTexture(gradient, processedGradient);
	}

	public static void SetGradientToTexture(GradientField gradient, Texture2D targetTexture) {
		int width = targetTexture.width;

		Color[] colors = new Color[width];
		for (int i = 0; i < width; i++) {
			float lerp = (float)i / (width - 1);
			colors[i] = gradient.Evaluate(lerp);
		}

		targetTexture.SetPixels(colors);
		targetTexture.Apply();
	}

	public static GradientField GenerateGradient(SerializedProperty property) {
		var gradient = new GradientField();

		gradient.mode = (GradientMode)property.FindPropertyRelative("m_mode").intValue;
		var colorProps = property.FindPropertyRelative("m_serializedColorKeys");
		var alphaProps = property.FindPropertyRelative("m_serializedAlphaKeys");


		GradientColorKey[] colorKeys;
		GradientAlphaKey[] alphaKeys;

		SerializedProperty prop;

		colorKeys = new GradientColorKey[colorProps.arraySize];
		for (int i = 0; i < colorKeys.Length; i++) {
			prop = colorProps.GetArrayElementAtIndex(i);
			colorKeys[i] = new GradientColorKey(
				prop.FindPropertyRelative("color").colorValue,
				prop.FindPropertyRelative("time").floatValue / 100f
			);
		}

		alphaKeys = new GradientAlphaKey[alphaProps.arraySize];
		for(int i = 0; i < alphaKeys.Length; i++) {
			prop = alphaProps.GetArrayElementAtIndex(i);
			alphaKeys[i] = new GradientAlphaKey(
				prop.FindPropertyRelative("alpha").floatValue,
				prop.FindPropertyRelative("time").floatValue / 100f
			);
		}

		gradient.alphaKeys = alphaKeys;
		gradient.colorKeys = colorKeys;

		return gradient;
	}
	#endregion

	#region Support Textures

	public static Texture2D GetCheckerTexture() {
		if (m_checkerTexture == null) {
			int size = Mathf.RoundToInt(EditorGUIUtility.singleLineHeight);
			m_checkerTexture = new Texture2D(size, size);

			int stepSize = size / 4;

			Color[] checkerColors = new Color[] {
				Color.white,
				new Color(0.6f, 0.6f, 0.6f)
			};

			for(int x = 0; x < size; x++) {
				for(int y = 0; y < size; y++) {
					int xVal = x / stepSize;
					int yVal = y / stepSize;

					Color c = checkerColors[(xVal + yVal) % 2];
					m_checkerTexture.SetPixel(x, y, c);
				}
			}
			m_checkerTexture.Apply();
		}

		return m_checkerTexture;
	}

	private Texture2D GetInspectorBackground() {
		if (m_inspectorBackground != null)
			return m_inspectorBackground;

		m_inspectorBackground = GenerateInspectorBackground(8, 1);

		return m_inspectorBackground;
	}

	public static Texture2D GenerateInspectorBackground(int tileX, int tileY) {

		var checkerTexture = GetCheckerTexture();

		Vector2Int checkerSize = new Vector2Int(checkerTexture.width, checkerTexture.height);

		int width = checkerSize.x * tileX;
		int height = checkerSize.y * tileY;

		var checkerPixels = checkerTexture.GetPixels();

		var texture = new Texture2D(width, height);

		for (int x = 0; x < tileX; x++) {
			for(int y = 0; y < tileY; y++) {
				texture.SetPixels(x * checkerSize.x, y * checkerSize.y, checkerSize.x, checkerSize.y, checkerPixels);
			}
		}

		texture.Apply();

		return texture;
	}

	#endregion

}
