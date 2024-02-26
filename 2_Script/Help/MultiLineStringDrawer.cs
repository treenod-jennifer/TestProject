using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

#if UNITY_EDITOR
[CustomPropertyDrawer(typeof(MultiLineStringAttribute))]
public class MultiLineStringDrawer : PropertyDrawer
{
    public override void OnGUI(Rect rect, SerializedProperty prop, GUIContent label)
    {
        MultiLineStringAttribute mlAtt = (MultiLineStringAttribute)attribute;

        Rect textRect   = rect;
        textRect.height = mlAtt.height;

        EditorGUI.BeginProperty(textRect, label, prop);
        GUIStyle style = new GUIStyle(EditorStyles.textField);
        style.wordWrap = true;
        style.fixedHeight = mlAtt.height;
        EditorGUI.BeginChangeCheck();
        string input = EditorGUI.TextArea(textRect, prop.stringValue, style);
        if (EditorGUI.EndChangeCheck())
            prop.stringValue = input;
        EditorGUI.EndProperty();
    }

    public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
    {
        MultiLineStringAttribute mlAtt = (MultiLineStringAttribute)attribute;
        return mlAtt.height;
    }
}
#endif