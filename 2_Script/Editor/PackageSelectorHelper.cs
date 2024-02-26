using UnityEditor;
using UnityEngine;

#if !UNITY_4

public static class PackageSelectorHelper
{
    public static void BuildPackageSelectorHierarchy()
    {
        packageSelectorRoot = PackageSelectorNode.BuildHierarchy( AssetBundleHelper.AllAssetBundleNames );
    }

    public static void ForEachSelectedPackage( System.Action<string> callback )
    {
        if( callback == null )
            return;

        PackageSelector selector = PackageSelector.GetInstance();

        bool hasPackageSelected = false;
        foreach( var e in selector.Registry )
        {
            if( e.Value )
            {
                hasPackageSelected = true;
                callback( e.Key );
            }
        }

        if( !hasPackageSelected )
        {
            Debug.Log( "Please, select a Package first!" );
        }
    }

    public static void ShowSelectablePackages()
    {
        PackageSelector selector = PackageSelector.GetInstance();
        string[] allAssetBundles = AssetBundleHelper.AllAssetBundleNames;

        EditorHelper.BeginBoxHeader();

        EditorGUILayout.LabelField( "Packages", GUILayout.Width( 64.0f ) );
        GUILayout.FlexibleSpace();
        ShowSelectAllButton( selector, allAssetBundles, "Select All", true );
        ShowSelectAllButton( selector, allAssetBundles, "Clear", false );
        
        scroll = EditorHelper.EndBoxHeaderBeginContent( scroll );

        foreach( PackageSelectorNode node in packageSelectorRoot.children )
        {
            PackageSelectorNode.Show( selector, node, 0 );
        }

        if( EditorHelper.EndBox() )
        {
            selector.Save();
        }
    }

    public static void ShowSelectAllButton( PackageSelector selector, string[] allAssetBundles, string label, bool isSelected )
    {
        if( GUILayout.Button( label, EditorStyles.toolbarButton ) )
        {
            foreach( string assetBundle in allAssetBundles )
            {
                selector[assetBundle] = isSelected;
            }
        }
    }

//    private static PackageSelectorNode packageSelectorRoot = new PackageSelectorNode();
    public static PackageSelectorNode packageSelectorRoot = new PackageSelectorNode();

    private static Vector2 scroll = Vector2.zero;
}

#endif
