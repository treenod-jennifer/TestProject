﻿using UnityEngine;
using UnityEditor;
using System.Linq;
using System.Collections.Generic;

[CustomPropertyDrawer( typeof( PackageSelectorAttribute ) )]
public class PackageSelectorDrawer : PropertyDrawer
{
    public override void OnGUI( Rect position, SerializedProperty prop, GUIContent label )
    {
#if !UNITY_4
        string[] paths = AssetBundleHelper.AllAssetBundleNames;
        int selectedIndex = System.Array.IndexOf( paths, prop.stringValue );
#else
        List<string> paths = BuilderHelper.GetAllPackagePaths();
        int selectedIndex = paths.IndexOf( prop.stringValue );
#endif

        EditorGUI.BeginChangeCheck();
        selectedIndex = EditorGUI.Popup( position, label, selectedIndex, paths.Select( p => new GUIContent( p ) ).ToArray() );
        if( EditorGUI.EndChangeCheck() )
        {
            prop.stringValue = paths[selectedIndex];
        }
    }
}
