using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[CanEditMultipleObjects]
[CustomEditor(typeof(UIItemTab))]
public class UIItemTabEditor : Editor 
{
    private const string SET_GROUP_BUTTON_TEXT = "Set Group";

    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();

        if (GUILayout.Button(SET_GROUP_BUTTON_TEXT))
        {
            if(targets.Length > 1)
            {
                foreach (var t in targets)
                {
                    SetGroup(t, FindTab(t));
                }
            }
            else
            {
                EditorUtility.DisplayDialog(
                    "메시지", 
                    $"두개 이상의 Tab을 선택한 뒤 [{SET_GROUP_BUTTON_TEXT}]버튼을 눌러주세요.", 
                    "확인");
            }
        }
    }

    private void SetGroup(Object target, UIItemTab[] tabs)
    {
        SerializedObject serializedObject = new SerializedObject(target);

        serializedObject.Update();

        SerializedProperty groupProperty = serializedObject.FindProperty("group");

        groupProperty.ClearArray();

        foreach (var tab in tabs)
        {
            groupProperty.InsertArrayElementAtIndex(groupProperty.arraySize);
            SerializedProperty tabProperty = groupProperty.GetArrayElementAtIndex(groupProperty.arraySize - 1);
            tabProperty.objectReferenceValue = tab;
        }

        serializedObject.ApplyModifiedProperties();
    }

    private UIItemTab[] FindTab(Object target)
    {
        List<UIItemTab> tabs = new List<UIItemTab>();

        foreach(var t in targets)
        {
            if (t == target) continue;

            UIItemTab tab = t as UIItemTab;
            tabs.Add(tab);
        }

        return tabs.ToArray();
    }
}
