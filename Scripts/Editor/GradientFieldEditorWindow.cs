using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEditorInternal;

public class GradientFieldEditorWindow : EditorWindow {
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
	private Texture2D alphaHandleHighlight;
	private Texture2D colorHandleHighlight;

	private bool isDragging;
	private bool isDraggingToRemove;

	private GradientField currentGradient;

	private DateTime lastMouseUp;

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

		currentGradient = GradientFieldDrawer.GenerateGradient(property);
		GradientFieldDrawer.SetGradientToTexture(currentGradient, gradientTexture);
		bgTexture = GradientFieldDrawer.GenerateInspectorBackground(30, 2);


		alphaHandle = LoadIconFromRelativePath("/Resources/GradientAlphaKey.png");
		colorHandle = LoadIconFromRelativePath("/Resources/GradientColorKey.png");
		
		alphaHandleHighlight = LoadIconFromRelativePath("/Resources/GradientAlphaKey.Highlight.png");
		colorHandleHighlight = LoadIconFromRelativePath("/Resources/GradientColorKey.Highlight.png");

	}

	private void OnGUI() {
		EditorGUI.BeginChangeCheck();

		GUILayout.Space(10);

		EditorGUILayout.PropertyField(modeProperty);

		GUILayout.Space(20);

		Rect gradientArea = EditorGUILayout.GetControlRect(GUILayout.Height(50)); //Gray outline

		GUI.color = Color.gray;
		GUI.DrawTexture(gradientArea, Texture2D.whiteTexture);

		gradientArea = ExpandRect(gradientArea, -1, -1);

		GUI.color = Color.white;

		GUI.DrawTexture(gradientArea, bgTexture); //Gradient Background

		GUI.DrawTexture(gradientArea, gradientTexture); //Actual Gradient

		DrawKeyLine(gradientArea, gradientArea.yMin - 16, 14, EditMode.alpha, alphaArrayProperty); //Draw alpha keys
		DrawKeyLine(gradientArea, gradientArea.yMax + 2, 14, EditMode.color, colorArrayProperty); //Draw color keys

		GUI.color = Color.white;

		GUILayout.Space(20);


		//Draw selected item line

		EditorGUILayout.BeginHorizontal();

		EditorGUIUtility.labelWidth = 80;

		EditorGUILayout.PropertyField(selectedProperty.FindPropertyRelative(editMode == EditMode.alpha ? "alpha" : "color"), GUILayout.Width(Screen.width - 170));

		GUILayout.Space(20);
		EditorGUIUtility.labelWidth = 60;

		EditorGUILayout.PropertyField(selectedProperty.FindPropertyRelative("time"), new GUIContent("Location"), GUILayout.Width(120));

		GUILayout.Label("%");
		GUILayout.Space(20);
		EditorGUILayout.EndHorizontal();

		EditorGUIUtility.labelWidth = 0;

		bool needUpdate = false;

		needUpdate = CheckMouse(gradientArea);

		needUpdate = CheckKeyboard() || needUpdate;

		if (EditorGUI.EndChangeCheck() || needUpdate) {
			Save();
		}
	}

	bool CheckMouse(Rect gradientArea) {
		bool draggedHandle = false;

		if (isDragging && selectedProperty != null) {

			var array = (editMode == EditMode.alpha ? alphaArrayProperty : colorArrayProperty);

			switch (Event.current.type) {
				case EventType.MouseDrag:
					float targetTime = Mathf.InverseLerp(gradientArea.xMin, gradientArea.xMax, Event.current.mousePosition.x);

					selectedProperty.FindPropertyRelative("time").floatValue = targetTime * 100;
					draggedHandle = true;

					float handleCenter = editMode == EditMode.alpha ? gradientArea.yMin - 9 : gradientArea.yMax + 9;

					isDraggingToRemove = Mathf.Abs(handleCenter - Event.current.mousePosition.y) > 20 && array.arraySize > 1;

					break;
				case EventType.MouseUp:

					if (isDragging) {
						DateTime time = DateTime.Now;

						if(lastMouseUp != null && (lastMouseUp - time).TotalMilliseconds < 500) {
							/*
							 * Being unable to double click the selected handler and open the color picker
							 */

						}

						lastMouseUp = time;

						if (isDraggingToRemove) {
							DeleteSelectedItem();

							isDraggingToRemove = false;

							draggedHandle = true;
						}

						isDragging = false;
					}

					break;
			}
		}
		return draggedHandle;
	}

	bool CheckKeyboard() {
		if(Event.current.type == EventType.KeyUp) {
			if(Event.current.keyCode == KeyCode.Delete) {
				DeleteSelectedItem();
				Event.current.Use();
				return true;
			}
		}
		return false;
	}

	void DeleteSelectedItem() {
		var array = (editMode == EditMode.alpha ? alphaArrayProperty : colorArrayProperty);

		if (array.arraySize == 1) return;
		array.DeleteArrayElementAtIndex(selectedItem);

		selectedProperty = array.GetArrayElementAtIndex(0);
		selectedItem = 0;
	}

	// Draws the gradient key line
	void DrawKeyLine(Rect bounds, float y, float height, EditMode targetEditMode, SerializedProperty array) {
		bool isPointerDown = false;

		if(Event.current.type == EventType.MouseDown) {
			Rect area = new Rect(bounds.x - 20, y, bounds.width + 40, height); //Removes EditorGUILayout padding
			if (area.Contains(Event.current.mousePosition)) {
				isPointerDown = true;
				selectedProperty = null;
			}
		}

		int arraySize = array.arraySize;
		SerializedProperty prop;
		Rect currentRect;
		for(int i = 0; i < arraySize; i++) {
			prop = array.GetArrayElementAtIndex(i);

			bool isDrawingSelectedItem = targetEditMode == editMode && selectedItem == i;

			float x = Mathf.Lerp(bounds.xMin, bounds.xMax, prop.FindPropertyRelative("time").floatValue / 100);

			x -= 3;

			Color color;
			if (targetEditMode == EditMode.alpha) {
				float a = prop.FindPropertyRelative("alpha").floatValue;
				color = new Color(a, a, a);
			} else {
				color = prop.FindPropertyRelative("color").colorValue;
				color.a = 1;
			}

			if(isDrawingSelectedItem && isDraggingToRemove) {
				color = Color.clear;
			}

			GUI.color = color;
			currentRect = new Rect(x, y, 9, height);

			GUI.DrawTexture(currentRect, targetEditMode == EditMode.alpha ? alphaHandle : colorHandle);

			if (isDrawingSelectedItem) {
				GUI.color = Color.white;
				GUI.DrawTexture(currentRect, targetEditMode == EditMode.alpha ? alphaHandleHighlight : colorHandleHighlight);
			}

			if (isPointerDown) {
				currentRect = ExpandRect(currentRect, 5, 2); //Expands rect to make it easier to click
				if (currentRect.Contains(Event.current.mousePosition)) {
					selectedProperty = prop;
					editMode = targetEditMode;
					isDragging = true;
					selectedItem = i;
				}
			}
		}

		if(isPointerDown){
			if (selectedProperty == null) {
				array.InsertArrayElementAtIndex(0);
				selectedProperty = array.GetArrayElementAtIndex(0);
				selectedItem = 0;

				float time = Mathf.InverseLerp(bounds.xMin, bounds.xMax, Event.current.mousePosition.x);

				selectedProperty.FindPropertyRelative("time").floatValue = time * 100;

				var c = currentGradient.Evaluate(time);

				if (targetEditMode == EditMode.alpha) {
					selectedProperty.FindPropertyRelative("alpha").floatValue = c.a;
				} else {
					c.a = 1;
					selectedProperty.FindPropertyRelative("color").colorValue = c;
				}

				isDragging = true;

				editMode = targetEditMode;
				Save();
			}
			Repaint();
		}
	}

	void Save() {
		GradientFieldDrawer.ResetGradient();

		m_property.serializedObject.ApplyModifiedProperties();

		int excludeAlpha = -1;
		int excludeColor = -1;

		if (isDraggingToRemove) {
			excludeAlpha = editMode == EditMode.alpha ? selectedItem : -1;
			excludeColor = editMode == EditMode.color ? selectedItem : -1;
		}

		currentGradient = GradientFieldDrawer.GenerateGradient(m_property, excludeColor, excludeAlpha);
		GradientFieldDrawer.SetGradientToTexture(currentGradient, gradientTexture);

		Repaint();
	}

	Rect ExpandRect(Rect source, float x, float y) {
		return new Rect(
			source.x - x,
			source.y - y,
			source.width + x * 2,
			source.height + y * 2
		);
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
