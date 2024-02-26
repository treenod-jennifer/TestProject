#if UNITY_4_3 || UNITY_4_5 || UNITY_4_6
#define OVERRIDE_DEFAULT_MARGINS
#endif

using UnityEngine;
using UnityEditor;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

[CustomEditor( typeof( RemotePackageSettings ) )]
public class RemotePackageSettingsEditor : Editor
{
#if !UNITY_4
    public override void OnInspectorGUI()
    {
        StringBuilder message = new StringBuilder();
        message.AppendLine( "In Unity 5, the old package settings are not used!" );
        message.AppendLine();
        message.AppendLine( "After you upgrade, you can delete this and start creating" );
        message.AppendLine( "Asset Bundles using the new enhanced workflow." );
        EditorGUILayout.HelpBox( message.ToString(), MessageType.Warning );

        if( GUILayout.Button( "More info on the new workflow (web link)" ) )
        {
            Application.OpenURL( "http://docs.unity3d.com/Manual/EnhancedAssetBundle.html" );
        }

        if( GUILayout.Button( "Upgrade to the new workflow" ) )
        {
            LegacyUpgradeHelper.Upgrade();
        }
    }
#else

#if OVERRIDE_DEFAULT_MARGINS
    public override bool UseDefaultMargins()
    {
        return false;
    }
#endif

    public override void OnInspectorGUI()
    {
        EditorGUILayout.BeginHorizontal( EditorStyles.toolbar );

        if( GUILayout.Button( "Build", EditorStyles.toolbarButton ) )
        {
            RemotePackageManagerWindow.Window.ForEachSelectedBuildTarget( Settings.BuildAssetBundle );
        }

        GUILayout.FlexibleSpace();

        if( GUILayout.Button( "Open Package Manager", EditorStyles.toolbarButton ) )
        {
            RemotePackageManagerWindow.OpenWindow();
        }

        EditorGUILayout.EndHorizontal();

#if OVERRIDE_DEFAULT_MARGINS
        EditorGUILayout.BeginVertical( EditorStyles.inspectorDefaultMargins );
#endif

        string packageName = Path.GetFileName( Path.GetDirectoryName( AssetDatabase.GetAssetPath( Settings ) ) );

        EditorGUILayout.LabelField( "Package", packageName, EditorStyles.boldLabel );
        EditorGUILayout.LabelField( "Uri", Settings.GetPackageUri() );

        ShowFileSize();
        ShowVersion();
        ShowParentSettings();
        ShowParentUri();

        EditorGUILayout.Space();

        scroll = EditorGUILayout.BeginScrollView( scroll );
        EditorGUI.indentLevel++;

        ShowAssets();
        ShowIncludedAssets();

        EditorGUI.indentLevel--;
        EditorGUILayout.EndScrollView();

#if OVERRIDE_DEFAULT_MARGINS
        EditorGUILayout.EndVertical();
#endif
    }

    private void ShowFileSize()
    {
        EditorGUILayout.LabelField( "File Size", packageFileSizeString );
    }

    private void ShowVersion()
    {
        EditorGUI.BeginChangeCheck();
        int version = EditorGUILayout.IntField( "Version", Settings.version );
        EditorGUILayout.HelpBox( "Don't change the package version unless you know what you're doing!", MessageType.Warning, false );
        if( EditorGUI.EndChangeCheck() )
        {
            Settings.version = version;
            EditorUtility.SetDirty( Settings );
        }
    }

    private void ShowParentSettings()
    {
        EditorGUI.BeginChangeCheck();
        Object obj = EditorGUILayout.ObjectField( "Depends On", Settings.parent, typeof( RemotePackageSettings ), false );
        RemotePackageSettings parent = obj as RemotePackageSettings;
        if( EditorGUI.EndChangeCheck() && Settings.CheckParentCicle( parent ) )
        {
            Settings.parent = parent;
            EditorUtility.SetDirty( Settings );
        }
    }

    private void ShowParentUri()
    {
        if( Settings.parent == null )
        {
            EditorGUILayout.LabelField( "Parent Package URI", "No Dependency" );
        }
        else
        {
            EditorGUILayout.LabelField( "Parent Package URI", Settings.parent.GetPackageUri() );
            string message = string.Format( "\"{0}\"\nwill be loaded before this package.", Settings.parent.GetPackageUri() );
            EditorGUILayout.HelpBox( message, MessageType.Info );
        }
    }

    private void ShowAssets()
    {
        EditorGUI.BeginChangeCheck();

        SerializedProperty property = serializedObject.FindProperty( "assets" );
        EditorGUILayout.PropertyField( property, true );

        if( EditorGUI.EndChangeCheck() )
        {
            Settings.assets.Clear();

            for( int i = 0; i < property.arraySize; i++ )
            {
                Settings.assets.Add( property.GetArrayElementAtIndex( i ).objectReferenceValue );
            }

            GetDependencies( true );
            EditorUtility.SetDirty( Settings );
        }
    }

    private void ShowIncludedAssets()
    {
        showIncludedAssets = EditorGUILayout.Foldout( showIncludedAssets, "Included Assets" );

        if( showIncludedAssets )
        {
            EditorGUI.indentLevel++;

            foreach( Object asset in cachedDependencies )
            {
                EditorGUILayout.ObjectField( asset, typeof( Object ), false );
            }

            EditorGUI.indentLevel--;
        }
    }

    private void GetDependencies( bool forceCache )
    {
        if( cachedDependencies == null || forceCache )
        {
            string[] dependencies = AssetDatabase.GetDependencies( Settings.assets.Select( a => AssetDatabase.GetAssetPath( a ) ).ToArray() );
            cachedDependencies = dependencies.Select( p => AssetDatabase.LoadAssetAtPath( p, typeof( Object ) ) );
        }
    }

    private void GetPackageSize()
    {
        long? meanSize = Settings.GetMeanPackageSize();

        if( meanSize.HasValue )
        {
            packageFileSizeString = FileHelper.GetSizeString( meanSize.Value );
        }
        else
        {
            packageFileSizeString = "Not built yet.";
        }
    }

    private void OnEnable()
    {
        scroll = Vector2.zero;
        GetDependencies( false );
        GetPackageSize();
    }

    private IEnumerable<Object> cachedDependencies = null;

    private RemotePackageSettings Settings { get { return target as RemotePackageSettings; } }
    private bool showIncludedAssets = false;
    private Vector2 scroll = Vector2.zero;
    private string packageFileSizeString;

#endif // !UNITY_4
    }
