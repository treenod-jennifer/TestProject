using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
using System.Collections;
using System.Collections.Generic;
[CustomPropertyDrawer(typeof(EnumNamedArrayAttribute))]
public class DrawerEnumNamedArray : PropertyDrawer
{
    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        EnumNamedArrayAttribute enumNames = attribute as EnumNamedArrayAttribute;
        //propertyPath returns something like component_hp_max.Array.data[4]
        //so get the index from there
        int index = System.Convert.ToInt32(property.propertyPath.Substring(property.propertyPath.IndexOf("[")).Replace("[", "").Replace("]", ""));

        if ( index < 0)
            return;

        if( enumNames.names.Length <= index )
        {
            label.text = $"[{index}] Invalid Index";
            //draw field
            EditorGUI.PropertyField(position, property, label, true);
            return;
        }
            
        //change the label
        label.text = $"[{index}] " + enumNames.names[index];
        //draw field
        EditorGUI.PropertyField(position, property, label, true);
    }
}
#endif