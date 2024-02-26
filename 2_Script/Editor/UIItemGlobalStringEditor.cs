using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[CanEditMultipleObjects]
[CustomEditor(typeof(UIItemGlobalString))]
public class UIItemGlobalStringEditor : Editor {

    private void OnEnable()
    {
        foreach(var t in targets)
        {
            InitTarget(t);
        }
    }

    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();

        bool check = false;
        foreach(var t in targets)
        {
            check = check ? true : ResetCheck(t);
        }

        GUI.enabled = check;
        if (GUILayout.Button("Reset"))
        {
            foreach (var t in targets)
            {
                if (ResetCheck(t))
                {
                    InitTarget(t, true);
                }
            }
        }
    }

    private void InitTarget(UnityEngine.Object target, bool reset = false)
    {
        SerializedObject serializedObject = new SerializedObject(target);

        serializedObject.Update();

        UILabel label = (target as UIItemGlobalString).GetComponent<UILabel>();

        if (label == null)
        {
            return;
        }

        SerializedProperty stringProperty = serializedObject.FindProperty("stringLabel");

        if (reset == true || stringProperty.objectReferenceValue == null)
        {
            stringProperty.objectReferenceValue = label;
        }

        serializedObject.ApplyModifiedProperties();
    }

    private bool ResetCheck(UnityEngine.Object target)
    {
        UILabel label = (target as UIItemGlobalString).GetComponent<UILabel>();

        if (label == null)
            return false;

        SerializedObject serializedObject = new SerializedObject(target);

        serializedObject.Update();

        SerializedProperty stringProperty = serializedObject.FindProperty("stringLabel");

        if (stringProperty.objectReferenceValue as UILabel != label)
            return true;
        else
            return false;
    }
}
