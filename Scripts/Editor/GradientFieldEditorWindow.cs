using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class GradientFieldEditorWindow : EditorWindow
{
	private enum EditMode {
		color,
		alpha
	}

	private Texture2D gradientTexture;
	private Texture2D bgTexture;

	private SerializedProperty m_property;

	private SerializedProperty modeProperty;
	private SerializedProperty colorArrayProperty;
	private SerializedProperty alphaArrayProperty;

	private int selectedItem = 0;

	private EditMode editMode = EditMode.color;
	private SerializedProperty selectedProperty;

	public void Setup(SerializedProperty property) {
		m_property = property;

		modeProperty = property.FindPropertyRelative("m_mode");
		colorArrayProperty = property.FindPropertyRelative("m_serializedColorKeys");
		alphaArrayProperty = property.FindPropertyRelative("m_serializedAlphaKeys");


		editMode = EditMode.color;
		selectedItem = 0;

		selectedProperty = colorArrayProperty.GetArrayElementAtIndex(0);

		gradientTexture = new Texture2D(400, 1);
		gradientTexture.wrapMode = TextureWrapMode.Clamp;

		GradientFieldDrawer.SetGradientToTexture(GradientFieldDrawer.GenerateGradient(property), gradientTexture);
		bgTexture = GradientFieldDrawer.GenerateInspectorBackground(30, 2);
	}

	private void OnGUI() {
		EditorGUI.BeginChangeCheck();

		EditorGUILayout.PropertyField(modeProperty);

		GUILayout.Space(20);

		Rect gradientArea = EditorGUILayout.GetControlRect(GUILayout.Height(50));

		GUI.color = Color.gray;
		GUI.DrawTexture(gradientArea, Texture2D.whiteTexture);
		gradientArea.x += 1;
		gradientArea.y += 1;
		gradientArea.width -= 2;
		gradientArea.height -= 2;

		GUI.color = Color.white;

		GUI.DrawTexture(gradientArea, bgTexture);

		GUI.DrawTexture(gradientArea, gradientTexture);
		

		GUILayout.Space(20);
		GUI.color = Color.white;

		EditorGUILayout.BeginHorizontal();

		EditorGUIUtility.labelWidth = 80;

		EditorGUILayout.PropertyField(selectedProperty.FindPropertyRelative(editMode == EditMode.alpha ? "alpha" : "color"), GUILayout.Width( Screen.width - 170));

		GUILayout.Space(20);
		EditorGUIUtility.labelWidth = 60;

		EditorGUILayout.PropertyField(selectedProperty.FindPropertyRelative("time"), new GUIContent("Location"), GUILayout.Width(120));

		GUILayout.Label("%");
		GUILayout.Space(20);
		EditorGUILayout.EndHorizontal();

		EditorGUIUtility.labelWidth = 0;


		if (EditorGUI.EndChangeCheck()) {
			GradientFieldDrawer.ResetGradient();

			m_property.serializedObject.ApplyModifiedProperties();

			GradientFieldDrawer.SetGradientToTexture(GradientFieldDrawer.GenerateGradient(m_property), gradientTexture);

			Repaint();
		}
	}
}
