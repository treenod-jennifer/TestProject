using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class RemotePackageSettings : ScriptableObject
{
#if !UNITY_4

    public class Runtime
    {
        static Runtime()
        {
            IsDownloadingManifest = false;
        }

        public static bool HasManifest { get { return manifest != null; } }
        public static bool IsDownloadingManifest { get; set; }

        private const string cachedManifestPrefsKey = "RemotePackageManager_ManifestCache";

        private static string CachedManifest
        {
            get { return PlayerPrefs.GetString( cachedManifestPrefsKey, null ); }
            set { PlayerPrefs.SetString( cachedManifestPrefsKey, value ); }
        }

        public static bool ParseOrLoadCache( WWW www )
        {
            return ParseManifest( www ) || LoadCachedManifest();
        }

        public static bool ParseManifest( WWW www )
        {
            if( !string.IsNullOrEmpty( www.error ) )
            {
                return false;
            }

            bool success = false;
            IsDownloadingManifest = false;

            try
            {
                manifestAssetBundle = www.assetBundle;
                if( manifestAssetBundle != null )
                {
                    manifest = manifestAssetBundle.LoadAllAssets<AssetBundleManifest>().FirstOrDefault();
                    CachedManifest = System.Convert.ToBase64String( www.bytes );
                    success = true;
                }
            }
            catch( System.Exception )
            {
                manifestAssetBundle = null;
                manifest = null;

                return false;
            }

            return success;
        }

        public static bool LoadCachedManifest()
        {
            string serialized = CachedManifest;
            if( string.IsNullOrEmpty( serialized ) )
            {
                return false;
            }

            byte[] manifestBytes = System.Convert.FromBase64String( serialized );

#if UNITY_5_0 || UNITY_5_1 || UNITY_5_2
			manifest = AssetBundle.CreateFromMemoryImmediate( manifestBytes ).LoadAllAssets<AssetBundleManifest>().FirstOrDefault();
#else
			manifest = AssetBundle.LoadFromMemory( manifestBytes ).LoadAllAssets<AssetBundleManifest>().FirstOrDefault();
#endif

            return true;
        }

        public static Runtime Get( string uri )
        {
            Runtime settings = null;
            if( HasManifest && !registry.TryGetValue( uri, out settings ) )
            {
                settings = new Runtime();
                settings.hash = manifest.GetAssetBundleHash( uri );
                settings.dependencyUris = manifest.GetDirectDependencies( uri );
                registry.Add( uri, settings );
            }

            return settings;
        }

        public bool HasDependency { get { return dependencyUris != null && dependencyUris.Length > 0; } }

        public Hash128 hash;
        public string[] dependencyUris = null;

        private static AssetBundle manifestAssetBundle = null;
        private static AssetBundleManifest manifest = null;

        private static Dictionary<string, Runtime> registry = new Dictionary<string, Runtime>();
    }

#else

    public class Runtime
    {
        public static bool HasManifest { get { return manifest != null; } }

        private const string cachedManifestPrefsKey = "RemotePackageManager_ManifestCache";
        private static string CachedManifest
        {
            get { return PlayerPrefs.GetString( cachedManifestPrefsKey, null ); }
            set { PlayerPrefs.SetString( cachedManifestPrefsKey, value ); }
        }

        public static bool ParseOrLoadCache( WWW www )
        {
            return ParseManifest( www ) || LoadCachedManifest();
        }

        public static bool ParseManifest( WWW www )
        {
            if( !string.IsNullOrEmpty( www.error ) )
            {
                return false;
            }

            return ParseManifest( www.text );
        }

        public static bool LoadCachedManifest()
        {
            string serialized = CachedManifest;

            if( !string.IsNullOrEmpty( serialized ) )
            {
                return ParseManifest( serialized );
            }

            return false;
        }

        public static Runtime Get( string uri )
        {
            Runtime settings = null;

            if( manifest != null && manifest.TryGetValue( uri, out settings ) )
            {
                return settings;
            }

            return null;
        }

        private static bool ParseManifest( string text )
        {
            try
            {
                manifest = new Dictionary<string, Runtime>();

                string[] lines = text.Split( '\n' );

                foreach( string line in lines )
                {
                    string[] keyPair = line.Split( ':' );

                    if( keyPair.Length >= 2 )
                    {
                        manifest.Add( keyPair[0], Parse( keyPair[1] ) );
                    }
                }

                CachedManifest = text;
            }
            catch( System.Exception )
            {
                manifest = null;
                return false;
            }

            return true;
        }

        private static Runtime Parse( string text )
        {
            Runtime settings = new Runtime();

            string[] parts = text.Split( ',' );

            if( parts.Length > 0 )
            {
                int.TryParse( parts[0], out settings.version );
            }

            if( parts.Length > 1 )
            {
                settings.parentPackageUri = parts[1];
            }

            return settings;
        }

        public bool HasDependency { get { return !string.IsNullOrEmpty( parentPackageUri ); } }

        public int version = 0;
        public string parentPackageUri = "";

        private static Dictionary<string, Runtime> manifest = null;
    }

#endif

		public int version = 0;

    public List<Object> assets = new List<Object>();
    public RemotePackageSettings parent;

    public bool isExpanded = true;
}
