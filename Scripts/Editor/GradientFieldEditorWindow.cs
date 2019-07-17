using System;
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

	private Texture2D alphaHandle;
	private Texture2D colorHandle;

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

		
		alphaHandle = LoadIconFromRelativePath("/Resources/GradientAlphaKey.png");
		colorHandle = LoadIconFromRelativePath("/Resources/GradientColorKey.png");
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

		DrawKeyLine(gradientArea, gradientArea.yMin - 16, 14, EditMode.alpha, alphaArrayProperty);
		DrawKeyLine(gradientArea, gradientArea.yMax + 2, 14, EditMode.color, colorArrayProperty);

		GUI.color = Color.white;

		GUILayout.Space(20);

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
	void DrawKeyLine(Rect bounds, float y, float height, EditMode targetEditMode, SerializedProperty array) {
		int arraySize = array.arraySize;
		SerializedProperty prop;
		Rect r;
		for(int i = 0; i < arraySize; i++) {
			prop = array.GetArrayElementAtIndex(i);
			float x = Mathf.Lerp(bounds.xMin, bounds.xMax, prop.FindPropertyRelative("time").floatValue / 100);
			x -= 3;

			Color col;
			if (targetEditMode == EditMode.alpha) {
				float a = prop.FindPropertyRelative("alpha").floatValue;
				col = new Color(a, a, a);
			} else {
				col = prop.FindPropertyRelative("color").colorValue;
			}

			GUI.color = col;
			r = new Rect(x, y, 9, height);
			GUI.DrawTexture(r, targetEditMode == EditMode.alpha ? alphaHandle : colorHandle);
		}
	}

	//From: https://forum.unity.com/threads/editorguiutility-load-unity-documentation-inconsistency.561262/
	private Texture2D LoadIconFromRelativePath(string relativePath) {
		MonoScript asset = MonoScript.FromScriptableObject(this);
		string myPath = AssetDatabase.GetAssetPath(asset);
		string[] myPathArray = myPath.Split('/');
		myPath = String.Join("/", myPathArray, 0, myPathArray.Length - 1);
		myPath += relativePath;
		return EditorGUIUtility.Load(myPath) as Texture2D;
	}

}
