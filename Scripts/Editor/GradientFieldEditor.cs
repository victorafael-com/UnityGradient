using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[CustomPropertyDrawer(typeof(GradientField))]
public class GradientFieldEditor : PropertyDrawer {
	const float textureSizeMultiplier = 0.5f;

	private static Texture2D m_checkerTexture;
	private static Texture2D m_inspectorBackground;

	private Texture2D processedGradient;

	public override float GetPropertyHeight(SerializedProperty property, GUIContent label) {
		return EditorGUIUtility.singleLineHeight;
	}

	public override void OnGUI(Rect position, SerializedProperty property, GUIContent label) {
		Rect rect = EditorGUI.PrefixLabel(position, label);

		CheckTextureGradient(rect.width, property);

		GUI.color = new Color(0.4f, 0.4f, 0.4f);
		GUI.DrawTexture(rect, Texture2D.whiteTexture);
		GUI.color = Color.white;

		rect.x += 1;
		rect.y += 1;
		rect.width -= 2;
		rect.height -= 2;

		GUI.DrawTexture(rect, GetInspectorBackground());
		GUI.DrawTexture(rect, processedGradient);
	}

	private void CheckTextureGradient(float width, SerializedProperty property) {
		int textureWidth = Mathf.Abs(Mathf.FloorToInt(width * textureSizeMultiplier));

		if (processedGradient != null && processedGradient.width == textureWidth) {
			return;
		}

		if (Event.current.type != EventType.Repaint)
			return;

		UpdateTextureGradient(textureWidth, property);
	}

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

	public static Texture2D GetInspectorBackground() {
		if (m_inspectorBackground != null)
			return m_inspectorBackground;

		var checkerTexture = GetCheckerTexture();

		int horizontalRepeats = 8;

		int width = checkerTexture.width * horizontalRepeats;


		var checkerPixels = checkerTexture.GetPixels();

		Vector2Int checkerSize = new Vector2Int(checkerTexture.width, checkerTexture.height);

		m_inspectorBackground = new Texture2D(width, checkerSize.y);

		for (int i = 0; i < horizontalRepeats; i++) {
			m_inspectorBackground.SetPixels(i * checkerSize.x, 0, checkerSize.x, checkerSize.y, checkerPixels);
		}

		m_inspectorBackground.Apply();

		return m_inspectorBackground;
	}

	private void UpdateTextureGradient(int width, SerializedProperty property) {
		if (processedGradient == null || processedGradient.width != width)
			processedGradient = new Texture2D(width, 1);

		GradientField gradient = (GradientField) fieldInfo.GetValue(property.serializedObject.targetObject);

		Color[] colors = new Color[width];
		for (int i = 0; i < width; i++) {
			float lerp = (float)i / (width - 1);
			colors[i] = gradient.Evaluate(lerp);
		}

		processedGradient.SetPixels(colors);
		processedGradient.Apply();
	}
}
