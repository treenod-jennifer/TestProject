using UnityEngine;
using UnityEditor;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

public static class HierarchyHelper
{
    public class SettingsNode
    {
        public RemotePackageSettings self;
        public ICollection<SettingsNode> children;

        public SettingsNode( RemotePackageSettings settings )
        {
            self = settings;
            children = new List<SettingsNode>();
        }
    }

    public static List<SettingsNode> GetSettingsTree( IEnumerable<RemotePackageSettings> allSetings )
    {
        Dictionary<RemotePackageSettings, SettingsNode> nodeMap = new Dictionary<RemotePackageSettings, SettingsNode>();
        List<SettingsNode> roots = new List<SettingsNode>();

        List<RemotePackageSettings> settingsList = allSetings.ToList();

        while( settingsList.Count > 0 )
        {
            for( int i = 0; i < settingsList.Count; )
            {
                RemotePackageSettings settings = settingsList[i];
                RemotePackageSettings parentSettings = settings.parent;

                SettingsNode node = null;
                SettingsNode parentNode = null;

                if( parentSettings == null )
                {
                    node = new SettingsNode( settings );
                    nodeMap.Add( settings, node );
                    settingsList.RemoveAt( i );

                    roots.Add( node );
                    continue;
                }
                else if( nodeMap.TryGetValue( parentSettings, out parentNode ) )
                {
                    node = new SettingsNode( settings );
                    nodeMap.Add( settings, node );
                    settingsList.RemoveAt( i );

                    parentNode.children.Add( node );
                    continue;
                }
                else
                {
                    i++;
                }
            }
        }

        return roots;
    }

    public static void DrawHierarchyView()
    {
        scroll = EditorGUILayout.BeginScrollView( scroll );

        foreach( SettingsNode root in GetSettingsTree( PackageSettingsHelper.AllPackageSettings ) )
        {
            ShowSettingsNode( root );
        }

        Rect rootDropPosition = GUILayoutUtility.GetRect( 0.0f, 0.0f, GUILayout.ExpandWidth( true ), GUILayout.ExpandHeight( true ) );
        DragDropHelper.DragAndDropArea( rootDropPosition, null );

        EditorGUILayout.EndScrollView();
    }

    private static void ShowSettingsNode( SettingsNode node )
    {
        if( node.children.Count > 0 )
        {
            Rect position = EditorHelper.Rect( EditorStyles.label );
            DragDropHelper.DragAndDropArea( position, node.self );

            EditorGUI.BeginChangeCheck();
            node.self.isExpanded = EditorGUI.Foldout( position, node.self.isExpanded, node.self.name );
            if( EditorGUI.EndChangeCheck() )
            {
                EditorUtility.SetDirty( node.self );
                AssetDatabase.SaveAssets();
            }

            if( node.self.isExpanded )
            {
                EditorGUI.indentLevel += 2;
                foreach( SettingsNode child in node.children )
                {
                    ShowSettingsNode( child );
                }
                EditorGUI.indentLevel -= 2;
            }
        }
        else
        {
            Rect position = EditorHelper.Rect( EditorStyles.label );
            DragDropHelper.DragAndDropArea( position, node.self );

            EditorGUI.LabelField( position, node.self.name );
        }
    }

    private static Vector2 scroll = Vector2.zero;
}
