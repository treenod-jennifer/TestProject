using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;

public class BuilderHelper
{
//    public const string assetBundlesFolderPath = "Assets/RemotePackageManager/AssetBundles";
    public const string assetBundlesFolderPath = "AssetBundles";

#if UNITY_4
    public const string recentlyBuiltFolderPath = "Assets/RemotePackageManager/AssetBundles_RecentlyBuilt";

    public static List<string> GetAllPackagePaths()
    {
        List<string> paths = new List<string>();

        BuilderHelper.ForEachPackageInFolder( BuilderHelper.assetBundlesFolderPath, package => {
            package = package.Replace( '\\', '/' );
            package = package.Remove( 0, BuilderHelper.assetBundlesFolderPath.Length + 1 );

            paths.Add( package );
        } );

        return paths;
    }
#endif

#if !UNITY_4

    public static string GetRecentlyBuiltFolderPath( bool useStreamingAssets )
    {
#if UNITY_EDITOR
        return useStreamingAssets ? "Assets/StreamingAssets" : ManagerSettings.Instance.exportedAssetBundlesFolder;
#else
        return useStreamingAssets ? Application.streamingAssetsPath : ManagerSettings.Instance.exportedAssetBundlesFolder;
#endif
    }

    public static void ForEachPackageInFolderLegacy( string folderPath, System.Action<string> callback )
#else
    public static void ForEachPackageInFolder( string folderPath, System.Action<string> callback )
#endif
    {
        if( !Directory.Exists( folderPath ) )
            return;

        Queue<string> queue = new Queue<string>();

        foreach( string childFolder in Directory.GetDirectories( folderPath ) )
        {
            queue.Enqueue( childFolder );
        }

        while( queue.Count > 0 )
        {
            string folder = queue.Dequeue();
            string[] childFolders = Directory.GetDirectories( folder );

            if( childFolders.Length == 0 )
            {
                callback( folder );
            }

            foreach( string childFolder in childFolders )
            {
                queue.Enqueue( childFolder );
            }
        }
    }

    public static void CreateFolderTree( string path )
    {
        if( !string.IsNullOrEmpty( Path.GetExtension( path ) ) )
        {
            path = Path.GetDirectoryName( path );
        }

        if( Directory.Exists( path ) )
        {
            return;
        }

        string[] folders = path.Split( Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar );
        path = folders[0];

        RemotePackageManagerWindow.ProjectChangeLocked = true;

        foreach( string folder in folders.Skip( 1 ) )
        {
            if( !Directory.Exists( Path.Combine( path, folder ) ) )
            {
                AssetDatabase.CreateFolder( path, folder );
            }

            path = Path.Combine( path, folder );
        }

        RemotePackageManagerWindow.ProjectChangeLocked = false;
        PackageSettingsHelper.RefreshAllPackageSettingsCache();

        AssetDatabase.Refresh();
    }

#if !UNITY_4

    public static string GetRecentlyBuiltFolderByTarget( ManagerSettings.Target target )
    {
        string platformName = RemotePackageManager.GetPlatformName( target.buildTarget );
        return string.Concat( GetRecentlyBuiltFolderPath( target.useStreamingAssets ), "/", platformName );
    }

#else
    public static string GetRecentlyBuiltFolderByTarget( BuildTarget buildTarget )
    {
        string platformName = RemotePackageManager.GetPlatformName( buildTarget );
        return string.Concat( BuilderHelper.recentlyBuiltFolderPath, "/", platformName );
    }
#endif

    public static void ExportPackageToFolder( string packagePath, BuildTarget buildTarget, string folderPath )
    {
        RemotePackageSettings settings = PackageSettingsHelper.Get( packagePath );

        folderPath = Path.Combine( folderPath, settings.GetPackageUri() );

        CreateFolderTree( folderPath );
        CreateFolderTree( packagePath );

        string copyPackagePath = RemotePackageManager.GetExportedPackageFilePath( folderPath );

        if( File.Exists( copyPackagePath ) )
        {
            AssetDatabase.DeleteAsset( copyPackagePath );
        }

        AssetDatabase.CopyAsset( RemotePackageManager.GetPackageFilePath( packagePath, buildTarget ), copyPackagePath );
    }

#if UNITY_4
    public static void ExportManifest( BuildTarget target )
    {
        string recentlyBuiltPath = BuilderHelper.GetRecentlyBuiltFolderByTarget( target );
        string exportPath = Path.Combine( recentlyBuiltPath, RemotePackageManager.manifestFileName );
        File.WriteAllText( exportPath, PackageSettingsHelper.SerializeManifest() );
    }
#endif
}
