using UnityEngine;
using UnityEditor;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

#if !UNITY_4
public class PackageSelectorNode
{
    public string name = "";
    public string path = null;
    public bool foldout = false;
    public string sizeString;
    public List<PackageSelectorNode> children = new List<PackageSelectorNode>();

    public PackageSelectorNode GetChild( string childName )
    {
        PackageSelectorNode child = children.FirstOrDefault( c => c.name == childName );
        if( child == null )
        {
            child = new PackageSelectorNode();
            child.name = childName;
            children.Add( child );
        }

        return child;
    }

    public static PackageSelectorNode BuildHierarchy( string[] allAssetBundles )
    {
        PackageSelectorNode root = new PackageSelectorNode();

        foreach( string assetBundle in allAssetBundles )
        {
            PackageSelectorNode node = root;
            foreach( string part in assetBundle.Split( '/' ) )
            {
                node = node.GetChild( part );
            }

            node.path = assetBundle;

            long? size = PackageSettingsHelper.GetMeanPackageSize( assetBundle );
            node.sizeString = size.HasValue ? FileHelper.GetSizeString( size.Value ) : "Not built.";
        }

        return root;
    }

    public static void Show( PackageSelector selector, PackageSelectorNode node, int depth )
    {
        float indentSize = 16.0f;
        float foldoutSize = 12.0f;
        float toggleSize = indentSize + 8.0f;
        EditorGUILayout.BeginHorizontal();

        /*
        if( string.IsNullOrEmpty( node.path ) )
        {
            GUILayout.Space( toggleSize );
        }
        else
        {
            selector[node.path] = EditorGUILayout.Toggle( selector[node.path], GUILayout.Width( indentSize ) );
        }
        */
        if( node.children.Count > 0 )
        {
            GUILayout.Space( indentSize * depth );
            node.foldout = EditorGUILayout.Foldout( node.foldout, node.name );
            GUILayout.Space(toggleSize);
            if (GUILayout.Button(EditorHelper.Label("All Set"), EditorStyles.miniButton, GUILayout.Width(60.0f)))
            {
                for (int i = 0; i < node.children.Count; i++)
                {
                    if (!string.IsNullOrEmpty(node.children[i].path))
                    {
                        selector[node.children[i].path] = true;
                    }
                }
            }
            if (GUILayout.Button(EditorHelper.Label("All UnSet"), EditorStyles.miniButton, GUILayout.Width(60.0f)))
            {
                for (int i = 0; i < node.children.Count; i++)
                {
                    if (!string.IsNullOrEmpty(node.children[i].path))
                    {
                        selector[node.children[i].path] = false;
                    }
                }
            }
        }
        else
        {
//            GUILayout.Space( foldoutSize + indentSize * depth );
            GUILayout.Space(indentSize * depth);
            selector[node.path] = EditorGUILayout.Toggle(selector[node.path], GUILayout.Width(indentSize));
            EditorGUILayout.LabelField( node.name );
        }

        if( !string.IsNullOrEmpty( node.path ) )
        {
            GUILayout.FlexibleSpace();

            EditorHelper.BeginChangeLabelWidth( 1.0f );
            EditorGUILayout.LabelField( node.sizeString, EditorCompatibilityHelper.CenteredGreyMiniLabel() );
            EditorHelper.EndChangeLabelWidth();

            AssetBundleHelper.FilterButton( node.path, EditorStyles.miniButton, GUILayout.Width( 40.0f ) );
        }

        EditorGUILayout.EndHorizontal();

        if( node.children.Count > 0 && node.foldout )
        {
            foreach( PackageSelectorNode child in node.children )
            {
                Show( selector, child, depth + 1 );
            }
        }
    }
}
#endif
