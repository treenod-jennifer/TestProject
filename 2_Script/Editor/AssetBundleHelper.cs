using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using UnityEditor;
using UnityEngine;

#if !UNITY_4

public class AssetBundlePostprocessorHelper : AssetPostprocessor
{
    public void OnPostprocessAssetbundleNameChanged( string assetPath, string previousAssetBundleName, string newAssetBundleName )
    {
        AssetBundleHelper.UpdateMenuOptions();

        PackageVariantSelectorDrawer.OnUpdatePackageVariants();
        RemotePackageManagerEditor.OnUpdatePackageVariants();
    }
}

public static class AssetBundleHelper
{
    public static string[] AllAssetBundleNames
    {
        get { if( assetBundleNames == null ) UpdateMenuOptions(); return assetBundleNames; }
    }

    public static string[] AllAssetBundleVariants
    {
        get { if( assetBundleVariants == null ) UpdateMenuOptions(); return assetBundleVariants; }
    }

    public static void FilterButton( string assetBundleName, GUIStyle style, params GUILayoutOption[] options )
    {
        if( ProjectBrowserHelper.HasImplementation )
        {
            EditorGUI.BeginDisabledGroup( string.IsNullOrEmpty( assetBundleName ) || assetBundleName.Equals( SmartPopupHelper.NoneOption ) );
            GUI.tooltip = "Filter the project window to only show this Asset Bundle's assets.";
            if( GUILayout.Button( EditorHelper.Label( "Filter" ), style, options ) )
            {
                ProjectBrowserHelper.SetSearch( string.Concat( "b:", assetBundleName ) );
            }
            EditorGUI.EndDisabledGroup();
        }
    }

    public static bool ShowAssetBundleMenu()
    {
        if( !namesSmartPopup.HasOptions )
        {
            UpdateMenuOptions();
        }

        EditorHelper.BeginBoxHeader();

        EditorGUILayout.LabelField( "AssetBundle" );
        GUILayout.FlexibleSpace();

        bool requireRefresh = false;

        GUI.tooltip = "Remove all unused AssetBundle names.";
        if( GUILayout.Button( EditorHelper.Label( "Clear Unused" ), EditorStyles.toolbarButton ) )
        {
            AssetDatabase.RemoveUnusedAssetBundleNames();
            UpdateMenuOptions();
            requireRefresh = true;
        }

        EditorHelper.EndBoxHeaderBeginContent( Vector2.zero );

        Object[] assets = Selection.GetFiltered( typeof( Object ), SelectionMode.Assets );

        GUI.tooltip = "The currently selected asset in project.";
        if( assets.Length > 1 )
        {
            EditorGUILayout.LabelField( "Selected", "Multiple selected assets." );
        }
        else if( assets.Length > 0 )
        {
            EditorGUILayout.ObjectField( EditorHelper.Label( "Selected" ), assets[0], typeof( Object ), false );
        }
        else
        {
            EditorGUILayout.LabelField( "Selected", "No asset selected." );
        }

        if( assets.Length > 0 )
        {
            string assetBundleName = GetObjectsAssetBundle( assets );
            string assetBundleVariant = GetObjectsVariant( assets );

            EditorGUILayout.BeginHorizontal();
            namesSmartPopup.Show( "Package", assetBundleName, OnChangeAssetBundleName );
            FilterButton( assetBundleName, EditorStyles.miniButton, GUILayout.Width( 40.0f ) );
            EditorGUILayout.EndHorizontal();

            variantsSmartPopup.Show( "Variant", assetBundleVariant, OnChangeAssetBundleVariant );
        }
        else
        {
            EditorHelper.Rect( EditorStyles.popup );
            EditorHelper.Rect( EditorStyles.popup );
        }
        GUILayout.Space(30f);

        EditorHelper.EndBox();

        return requireRefresh;
    }

    private static void OnChangeAssetBundleName( string newName, bool isNew )
    {
        Object[] assets = Selection.GetFiltered( typeof( Object ), SelectionMode.Assets );
        SetObjectsAssetBundle( assets, newName );
    }

    private static void OnChangeAssetBundleVariant( string newName, bool isNew )
    {
        Object[] assets = Selection.GetFiltered( typeof( Object ), SelectionMode.Assets );
        SetObjectsVariant( assets, newName );
    }

    private static void SetForceAssetBundleName(string strFolder)
    {
        if (PackageSettingsHelper.IsBuildingPackages == true)
            return;
        AssetImporter assetImporter = null;
        DirectoryInfo dir = new DirectoryInfo(strFolder);
        if (dir.Exists == false)
            return;

        DirectoryInfo[] info = dir.GetDirectories();

        foreach (DirectoryInfo dirInfo in info)
        {
            string strPathName = dirInfo.Parent.FullName;
            int i_count = strPathName.LastIndexOf("5_OutResource");
            if (i_count >= 0)
            {
                string[] s = strPathName.Remove(0, i_count + 13).Split('\\');
                if (s.Length > 1)
                    strPathName = s[1];
                else
                    strPathName = s[0];
                if (!string.IsNullOrEmpty(strPathName))
                    strPathName += "/";
            }
            

            if (dirInfo.Name == "prefab")
            {
                string strPath = dirInfo.FullName;
                i_count = strPath.LastIndexOf("Assets");
                if (i_count >= 0)
                    strPath = strPath.Remove(0, i_count);
                assetImporter = AssetImporter.GetAtPath(strPath);
                if (assetImporter)
                { 
                    if(dirInfo.Parent.Name.Contains("common"))
                        assetImporter.assetBundleName = dirInfo.Parent.Name.Replace("areacommon", "ac") + "_p";
                    else
                        assetImporter.assetBundleName = dirInfo.Parent.Name.Replace("area", "a") + "_p";
                }
                
            }
            else if (dirInfo.Name == "resource")
            {
                string strPath = dirInfo.FullName;
                i_count = strPath.LastIndexOf("Assets");
                if (i_count >= 0)
                    strPath = strPath.Remove(0, i_count);
                assetImporter = AssetImporter.GetAtPath(strPath);
                if (assetImporter)
                {
                    if(dirInfo.Parent.Name.Contains("common"))
                        assetImporter.assetBundleName = dirInfo.Parent.Name.Replace("areacommon", "ac") + "_r";
                    else
                        assetImporter.assetBundleName = dirInfo.Parent.Name.Replace("area", "a") + "_r";
                }
            }
            else if (dirInfo.Name.Contains("global_"))
            {
                string strPath = dirInfo.FullName;
                i_count = strPath.LastIndexOf("Assets");
                if (i_count >= 0)
                    strPath = strPath.Remove(0, i_count);
                assetImporter = AssetImporter.GetAtPath(strPath);
                if (assetImporter)
                    assetImporter.assetBundleName = dirInfo.Name.Replace("global", "g") + "_p";
            }
            else if (dirInfo.Name.Contains("common_") && !dirInfo.Name.Contains("areacommon_"))
            {
                string strPath = dirInfo.FullName;
                i_count = strPath.LastIndexOf("Assets");
                if (i_count >= 0)
                    strPath = strPath.Remove(0, i_count);
                assetImporter = AssetImporter.GetAtPath(strPath);
                if (assetImporter)
                    assetImporter.assetBundleName = dirInfo.Name.Replace("common", "gc") + "_r";
            }
            else if (dirInfo.Name.Contains("housing_"))
            {
                string strPath = dirInfo.FullName;
                i_count = strPath.LastIndexOf("Assets");
                if (i_count >= 0)
                    strPath = strPath.Remove(0, i_count);
                assetImporter = AssetImporter.GetAtPath(strPath);
                if (assetImporter)
                    assetImporter.assetBundleName = dirInfo.Name.Replace("housing", "h");
                //assetImporter.assetBundleName = strPathName + dirInfo.Name.Replace("housing", "h");

            }
            else if (dirInfo.Name.Contains("stamp_"))
            {
                string strPath = dirInfo.FullName;
                i_count = strPath.LastIndexOf("Assets");
                if (i_count >= 0)
                    strPath = strPath.Remove(0, i_count);
                assetImporter = AssetImporter.GetAtPath(strPath);
                if (assetImporter)
                    assetImporter.assetBundleName = dirInfo.Name.Replace("stamp", "s");
                //assetImporter.assetBundleName = strPathName + dirInfo.Name.Replace("housing", "h");

            }
            else if (dirInfo.Name.Contains("event_"))
            {
                string strPath = dirInfo.FullName;
                i_count = strPath.LastIndexOf("Assets");
                if (i_count >= 0)
                    strPath = strPath.Remove(0, i_count);
                assetImporter = AssetImporter.GetAtPath(strPath);
                if (assetImporter)
                    assetImporter.assetBundleName = dirInfo.Name;//.Replace("stamp", "s");

            }
            else if (dirInfo.Name.Contains("shop_"))
            {
                string strPath = dirInfo.FullName;
                i_count = strPath.LastIndexOf("Assets");
                if (i_count >= 0)
                    strPath = strPath.Remove(0, i_count);
                assetImporter = AssetImporter.GetAtPath(strPath);
                if (assetImporter)
                    assetImporter.assetBundleName = dirInfo.Name;//.Replace("stamp", "s");
            }
            else if (dirInfo.Name.ToLower().Contains("l2d_"))
            {
                string strPath = dirInfo.FullName;
                i_count = strPath.LastIndexOf("Assets");
                if (i_count >= 0)
                    strPath = strPath.Remove(0, i_count);
                assetImporter = AssetImporter.GetAtPath(strPath);
                if (assetImporter)
                    assetImporter.assetBundleName = dirInfo.Name;//.Replace("stamp", "s");
            }
            else if (dirInfo.Name.ToLower().Contains("char_"))
            {
                string strPath = dirInfo.FullName;
                i_count = strPath.LastIndexOf("Assets");
                if (i_count >= 0)
                    strPath = strPath.Remove(0, i_count);
                assetImporter = AssetImporter.GetAtPath(strPath);
                if (assetImporter)
                    assetImporter.assetBundleName = dirInfo.Name;//.Replace("stamp", "s");
            }
            else if (dirInfo.Name.ToLower().Contains("title"))
            {
                string strPath = dirInfo.FullName;
                i_count = strPath.LastIndexOf("Assets");
                if (i_count >= 0)
                    strPath = strPath.Remove(0, i_count);
                assetImporter = AssetImporter.GetAtPath(strPath);
                if (assetImporter)
                    assetImporter.assetBundleName = dirInfo.Name;
            }
            else
            {
                SetForceAssetBundleName(dirInfo.FullName);
            }
        }

    }

    public static void UpdateMenuOptions()
    {
        HashSet<string> names = new HashSet<string>();
        HashSet<string> variants = new HashSet<string>();

        SetForceAssetBundleName(Application.dataPath + "/5_OutResource");

        foreach( string assetBundleName in AssetDatabase.GetAllAssetBundleNames() )
        {
            int index = assetBundleName.LastIndexOf( '.' );

            if( index >= 0 )
            {
                names.Add( assetBundleName.Substring( 0, index ) );

                if( assetBundleName.Length > index + 1 )
                {
                    variants.Add( assetBundleName.Substring( index + 1 ) );
                }
            }
            else
            {
                names.Add( assetBundleName );
            }
        }

        assetBundleNames = names.ToArray();
        assetBundleVariants = variants.ToArray();

        System.Array.Sort( assetBundleNames );
        System.Array.Sort( assetBundleVariants );

        namesSmartPopup.SetOptions( assetBundleNames );
        variantsSmartPopup.SetOptions( assetBundleVariants );

//        Debug.Log("UpdateMenuOptions()");

    }

    private static string GetObjectsAssetBundle( Object[] assets )
    {
        string assetBundleName = null;
        foreach( Object asset in assets )
        {
            string name = AssetImporter.GetAtPath( AssetDatabase.GetAssetPath( asset ) ).assetBundleName;
            name = string.IsNullOrEmpty( name ) ? SmartPopupHelper.NoneOption : name;

            if( assetBundleName == null )
            {
                assetBundleName = name;
            }
            else if( !assetBundleName.Equals( name ) )
            {
                return "";
            }
        }

        return assetBundleName;
    }

    private static void SetObjectsAssetBundle( Object[] assets, string assetBundleName )
    {
        foreach( Object asset in assets )
        {
            AssetImporter.GetAtPath( AssetDatabase.GetAssetPath( asset ) ).assetBundleName = assetBundleName;
        }
    }

    private static string GetObjectsVariant( Object[] assets )
    {
        string assetBundleVariant = null;
        foreach( Object asset in assets )
        {
            string variant = AssetImporter.GetAtPath( AssetDatabase.GetAssetPath( asset ) ).assetBundleVariant;
            variant = string.IsNullOrEmpty( variant ) ? SmartPopupHelper.NoneOption : variant;

            if( assetBundleVariant == null )
            {
                assetBundleVariant = variant;
            }
            else if( !assetBundleVariant.Equals( variant ) )
            {
                return "";
            }
        }

        return assetBundleVariant;
    }

    private static void SetObjectsVariant( Object[] assets, string assetBundleVariant )
    {
        foreach( Object asset in assets )
        {
            AssetImporter.GetAtPath( AssetDatabase.GetAssetPath( asset ) ).assetBundleVariant = assetBundleVariant;
        }
    }

    private static SmartPopupHelper namesSmartPopup = new SmartPopupHelper(
        "NewPackageNameTextField",
        "Select the AssetBundle in which this asset will be included.",
        "Create a new AssetBundle."
    );

    private static SmartPopupHelper variantsSmartPopup = new SmartPopupHelper(
        "NewPackageVariantTextField",
        "Select the AssetBundle Variant in which this asset will be included.",
        "Create a new AssetBundle Variant."
    );

    private static string[] assetBundleNames = null;
    private static string[] assetBundleVariants = null;
}

#endif
