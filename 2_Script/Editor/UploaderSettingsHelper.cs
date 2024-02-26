using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using UnityEditor;
using UnityEngine;

public static class UploaderSettingsHelper
{
#if UNITY_4
    public static void ShowUploadSettings()
    {
        bool clearAfterUpload = RemotePackageManagerWindow.Window.clearAfterUpload;

        EditorGUI.BeginChangeCheck();
        GUI.tooltip = "Should delete the \"RecentlyBuilt\" temporary folder after upload?";

        clearAfterUpload = GUILayout.Toggle( clearAfterUpload, EditorHelper.Label( "Clear After Upload" ), EditorStyles.toolbarButton );
        if( EditorGUI.EndChangeCheck() )
        {
            RemotePackageManagerWindow.Window.clearAfterUpload = clearAfterUpload;
            EditorPrefs.SetBool( "RemotePackageManager_ClearAfterUpload", clearAfterUpload );
        }
    }
#endif

    public static void UploadRecentlyBuiltButton( Uploader uploader )
    {
#if !UNITY_4
        if( Directory.Exists( BuilderHelper.GetRecentlyBuiltFolderPath( false ) ) )
#else
        if( Directory.Exists( BuilderHelper.recentlyBuiltFolderPath ) )
#endif
        {
            Uploader.RequireWeb( () => UploadRecentlyBuilt( uploader ) );
        }
        else
        {
            Debug.LogError( "Recently Built directory does not exist. Nothing to upload!" );
        }
    }

    private static void UploadManifest( Uploader uploader, System.Action<bool> callback )
    {
#if !UNITY_4
        if( !Directory.Exists( BuilderHelper.GetRecentlyBuiltFolderPath( false ) ) )
#else
        if( !Directory.Exists( BuilderHelper.recentlyBuiltFolderPath ) )
#endif
            return;

#if !UNITY_4
        UploadManifestFromFolder( uploader, callback, BuilderHelper.GetRecentlyBuiltFolderPath( false ) );
#else
        UploadManifestFromFolder( uploader, callback, BuilderHelper.recentlyBuiltFolderPath );
#endif
    }

    private static void UploadManifestFromFolder( Uploader uploader, System.Action<bool> callback, string folder )
    {
        foreach( string buildTargetFolder in Directory.GetDirectories( folder ) )
        {
#if !UNITY_4
            string manifestFileName = Path.GetFileNameWithoutExtension( buildTargetFolder );
            string exportPath = Path.Combine( buildTargetFolder, manifestFileName );
#else
            string exportPath = Path.Combine( buildTargetFolder, RemotePackageManager.manifestFileName );
#endif

            if( File.Exists( exportPath ) )
            {
#if !UNITY_4
                string manifestUri = string.Concat( manifestFileName, "/", manifestFileName );
                uploader.UploadFile( exportPath, manifestUri, callback );
#else
                string manifestText = File.ReadAllText( exportPath );
                string manifestUri = string.Concat( Path.GetFileNameWithoutExtension( buildTargetFolder ), "/", RemotePackageManager.manifestFileName );
                uploader.UploadText( manifestText, manifestUri, callback );
#endif
            }
        }
    }

    private static void UploadRecentlyBuilt( Uploader uploader )
    {
#if !UNITY_4
//        UploadManifest( uploader, s => { } );
        UploadRecentlyBuiltFromFolder( uploader, true, BuilderHelper.GetRecentlyBuiltFolderPath( false ) );
#else
        bool success = true;
        UploadManifest( uploader, s => success = ( success && s ) );
        success = success && UploadRecentlyBuiltFromFolder( uploader, success, BuilderHelper.recentlyBuiltFolderPath );
#endif

#if UNITY_4
        if( RemotePackageManagerWindow.Window.clearAfterUpload && success )
        {
            AssetDatabase.DeleteAsset( BuilderHelper.recentlyBuiltFolderPath );
        }
#endif
    }

    private static bool UploadRecentlyBuiltFromFolder( Uploader uploader, bool success, string folder )
    {
#if !UNITY_4
        List<string> allAssetBundleVariants = new List<string>( AssetBundleHelper.AllAssetBundleVariants );
        allAssetBundleVariants.Add( "" );

        foreach( string buildTargetFolder in Directory.GetDirectories( folder ) )
        {
            PackageSelectorHelper.ForEachSelectedPackage( path => {
                foreach( string variant in allAssetBundleVariants )
                {
                    string p = string.Concat( buildTargetFolder, "/", path );
                    if( !string.IsNullOrEmpty( variant ) )
                    {
                        p = string.Concat( p, ".", variant );
                    }

                    string packageFile = RemotePackageManager.GetExportedPackageFilePath( p );
                    if( !File.Exists( packageFile ) )
                        continue;
#else
        BuilderHelper.ForEachPackageInFolder( BuilderHelper.recentlyBuiltFolderPath, p => {
                    string packageFile = RemotePackageManager.GetExportedPackageFilePath( p );
#endif
                    string[] folders = p.Split( Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar );
                    folders = folders.Skip( 1 ).ToArray();
                    string uri = string.Join( "/", folders );

                    string packageUri = RemotePackageManager.GetExportedPackageFilePath( uri ).Replace( '\\', '/' );

#if !UNITY_4
                    uploader.UploadFile( packageFile, packageUri, s => { } );
                }
            } );
        }
#else
            uploader.UploadFile( packageFile, packageUri, s => success = ( success && s ) );
        } );
#endif

        return success;
    }
}
