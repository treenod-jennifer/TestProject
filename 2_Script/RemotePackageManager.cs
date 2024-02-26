using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
//using NUnit.Framework.Constraints;
using UnityEngine;
using UnityEngine.Networking;

/// <summary>
/// RemotePackageManager.
///
/// Main class for managing your remote packages (AssetBundles).
///
/// <example>
/// <code>
/// RemotePackageManager.Load( "Your/Package/Name" ).GetAndInstantiate();
/// </code>
/// </example>
/// </summary>
///
/// <seealso cref="Load"/>
/// <seealso cref="RemotePackageSettingsDownloaderRequest"/>
public class RemotePackageManager : MonoBehaviour
{
	public const string packageFileNameSuffix = "";
	public bool forceWWWRequests = false;
	public bool useStreamingAssets = false;
	public string defaultVariant = "";
	public int selectedBaseUriIndex = 0;

	public List<string> baseUris = new List<string>( new[] { "https://s3.amazonaws.com/_your_bucket_name_/" } );

	private const string settingsFileNameSuffix = "";
	private static RemotePackageManager instance = null;

	private Dictionary<string, AssetBundle> assetBundleRegistry = new Dictionary<string, AssetBundle>();
	private HashSet<string> errorSet = new HashSet<string>();
	private string cachedSelectedBaseUri = null;
	private string cachedPlatformBaseUri = null;

	/// <summary>
	/// Gets a value indicating whether RemotePackageManager has instance.
	/// </summary>
	/// <value>
	/// <c>true</c> if RemotePackageManager has instance; otherwise, <c>false</c>.
	/// </value>
	public static bool HasInstance { get { return GetInstance( false ) != null; } }

	/// <summary>
	/// Base url to compose the final package url in runtime.
	/// </summary>
	/// <value>
	/// RemotePackageManager's base URL string.
	/// </value>
	public static string BaseUrl
	{
		get
		{
			RemotePackageManager manager = GetInstance( true );
			return manager != null ? manager.SelectedBaseUri : "";
		}
	}

	/// <summary>
	/// Check if a package is already loaded.
	///
	/// Note that is not needed to call this method before "Load()" as it already checks for
	/// cached packages for you!
	/// </summary>
	///
	/// <param name='packageUri'>
	/// Unity 4: Package's URI (path) starting from the folder "Assets/RemotePackageManger/AssetBundles/".
	/// Unity 5: Package's URI (AssetBundle name).
	/// </param>
	/// <param name='packageVariant'>
	/// Unity 5 only: Package's Variant (AssetBundle Variant).
	/// </param>
	/// <returns>
	/// True if the package is loaded, false otherwise.
	/// </returns>
	///
	/// <seealso cref="Load"/>
#if !UNITY_4

	public static bool IsUsingWWW
	{
		get
		{
#if UNITY_EDITOR
			RemotePackageManager manager = GetInstance( true );
			return manager != null ? manager.forceWWWRequests : false;
#else
			return true;
#endif
		}
	}

	/// <summary>
	/// Change the selected BaseUri given its new index. If no RemotePackageManager is instantiated, -1 is returned.
	/// </summary>
	public static int SelectedBaseUriIndex
	{
		get
		{
			RemotePackageManager manager = GetInstance( true );
			return manager != null ? manager.selectedBaseUriIndex : -1;
		}

		set
		{
			RemotePackageManager manager = GetInstance( true );
			if( manager != null )
			{
				manager.cachedSelectedBaseUri = null;
				manager.selectedBaseUriIndex = Mathf.Clamp( value, 0, manager.baseUris.Count - 1 );
			}
		}
	}

	private string PlatformBaseUri
	{
		get
		{
			if( string.IsNullOrEmpty( cachedPlatformBaseUri ) )
			{
				string url = SelectedBaseUri;

				if( url.EndsWith( "/" ) )
					cachedPlatformBaseUri = string.Concat( url, GetCurrentPlatformName() );
				else
					cachedPlatformBaseUri = string.Concat( url, "/", GetCurrentPlatformName() );
			}

			return cachedPlatformBaseUri;
		}
	}

	private string SelectedBaseUri
	{
		get
		{
			if( string.IsNullOrEmpty( cachedSelectedBaseUri ) )
			{
				if( selectedBaseUriIndex >= 0 && selectedBaseUriIndex < baseUris.Count )
				{
					if( !useStreamingAssets )
						cachedSelectedBaseUri = baseUris[selectedBaseUriIndex];
					else if( Application.streamingAssetsPath.Contains( "://" ) )
						cachedSelectedBaseUri = Application.streamingAssetsPath;
					else
						cachedSelectedBaseUri = string.Concat( Global.FileUri, Application.streamingAssetsPath );
				}
				else
				{
					cachedSelectedBaseUri = "";
				}
			}

			return cachedSelectedBaseUri;
		}
	}

	/// <summary>
	/// Checks whether a package was already downloaded (loaded into memory).
	/// </summary>
	/// <param name="packageUri"></param>
	/// <param name="packageVariant"></param>
	/// <returns></returns>
	public static bool IsLoaded( string packageUri, string packageVariant = null )
#else
	public static bool IsLoaded( string packageUri )
#endif
	{
		RemotePackageManager manager = GetInstance( true );

		if( manager != null )
		{
#if !UNITY_4
			return manager.InstanceIsLoaded( packageUri, packageVariant );
#else
			return manager.InstanceIsLoaded( packageUri );
#endif
		}

		return false;
	}

	/// <summary>
	/// RemotePackageManager's main method. It loads a package from the web or from inside the editor.
	///
	/// <example>
	/// <code>
	/// RemotePackageManager.Load( "Your/Package/Name" ).GetAndInstantiate();
	/// </code>
	/// </example>
	/// </summary>
	///
	/// <param name='packageUri'>
	/// Unity 4: Package's URI (path) starting from the folder "Assets/RemotePackageManger/AssetBundles/".
	/// Unity 5: Package's URI (AssetBundle name).
	/// </param>
	/// <param name='packageVariant'>
	/// Unity 5 only: Package's Variant (AssetBundle Variant).
	/// </param>
	/// <returns>
	/// A RemotePackageRequest is returned to gain more flexibility on the just downloaded package.
	/// </returns>
	///
	/// <seealso cref="RemotePackageSettingsDownloaderRequest"/>

//	public static RemotePackageRequest Load( string packageUri, string packageVariant = null )
    public static void Load(string packageUri, string packageVariant = null, System.Action<AssetBundle> callback = null)
	{
		RemotePackageManager manager = GetInstance( true );
	    if (manager != null)
            manager.InstanceLoad( packageUri, packageVariant, callback );
	}

    public static IEnumerator LoadSync(string packageUri, string packageVariant = null)
    {
        yield return null;
        RemotePackageManager manager = GetInstance(true);

        if (manager != null)
        {
            packageUri = manager.GetVariantPackageUri(packageUri.ToLower(), packageVariant);
            yield return manager.StartCoroutine(manager.InstanceLoadSync(packageUri));
        }
    }

	/// <summary>
	/// Unload all packages to save memory.
	///
	/// Note that this discards all cached references to downloaded assets meaning that subsequent
	/// calls to "Load" will actually download the entire package from the internet again.
	///
	/// Don't call this method when Objects from downloaded packages are still in use!
	/// This will cause undefined behaviour!
	/// </summary>
	///
	/// <seealso cref="Load"/>
	public static void Unload()
	{
		RemotePackageManager manager = GetInstance( true );

		if( manager != null )
		{
			manager.InstanceUnload();
		}
	}

	public static string GetExportedPackageFilePath( string packagePath )
	{
#if !UNITY_4
		return string.Concat( packagePath, packageFileNameSuffix );
#else
		string packageName = Path.GetFileNameWithoutExtension( packagePath );
		return Path.Combine( packagePath, packageName + packageFileNameSuffix );
#endif
	}

#if UNITY_EDITOR

	public static string GetPackageFilePath( string packagePath, UnityEditor.BuildTarget buildTarget )
	{
		string packageName = Path.GetFileNameWithoutExtension( packagePath );
		string platformName = GetPlatformName( buildTarget );

		return Path.Combine( packagePath, string.Concat( packageName, "-", platformName, packageFileNameSuffix ) );
	}

	public static string GetPlatformName( UnityEditor.BuildTarget target )
	{
		string platformName = target.ToString().ToLower();

// KTW CHG
        if (platformName.Contains("webplayer"))
            return "webplayer";
        if (platformName.Contains("standalone"))
            return "standalone";
        /*
        if (platformName.Contains("webplayer"))
            return "webplayer";
        else if (platformName.Contains("standalone"))
            return "standalone";
        else if (platformName.Contains("android"))
            return "aos";
        */
		return platformName.Replace( "player", "" );
	}

#endif

	public static string GetSettingsFilePath( string packagePath )
	{
		string packageName = Path.GetFileNameWithoutExtension( packagePath );
		return Path.Combine( packagePath, packageName + settingsFileNameSuffix );
	}

	private static string GetCurrentPlatformName()
	{
#if UNITY_EDITOR
		return GetPlatformName( UnityEditor.EditorUserBuildSettings.activeBuildTarget );
#else
		string platformName = Application.platform.ToString().ToLower();

		if( platformName.Contains( "webplayer" ) ) return "webplayer";

		bool windows = platformName.Contains( "windows" );
		bool osx = platformName.Contains( "osx" );
		bool linux = platformName.Contains( "linux" );
		if( windows || osx || linux ) return "standalone";

		return platformName.Replace( "player", "" ).Replace( "iphone", "ios" );
#endif
	}

	public static RemotePackageManager GetInstance( bool showError = false )
	{
		if( instance == null )
		{
			instance = Object.FindObjectOfType( typeof( RemotePackageManager ) ) as RemotePackageManager;

			if( instance == null && showError )
			{
				Debug.LogError( "Could not locate an instance of " + typeof( RemotePackageManager ).Name + "!" );
			}
		}

		return instance;
	}

#if !UNITY_4

	public string GetVariantPackageUri( string packageUri, string packageVariant )
	{
		packageVariant = string.IsNullOrEmpty( packageVariant ) ? defaultVariant : packageVariant;
		if( !string.IsNullOrEmpty( packageVariant ) )
		{
			return string.Concat( packageUri, ".", packageVariant );
		}

		return packageUri;
	}

	private bool InstanceIsLoaded( string packageUri, string packageVariant )
	{
		packageUri = GetVariantPackageUri( packageUri, packageVariant );
#else
	private bool InstanceIsLoaded( string packageUri )
	{
#endif
		AssetBundle assetBundle = null;
		return assetBundleRegistry.TryGetValue( packageUri, out assetBundle ) && assetBundle == null;
	}

	private void InstanceLoad( string packageUri, string packageVariant, System.Action<AssetBundle> callback = null )
	{
		packageUri = GetVariantPackageUri( packageUri.ToLower(), packageVariant );
        StartCoroutine(InstanceLoadSync(packageUri, callback));
	}

    public IEnumerator InstanceLoadSync(string packageUri, System.Action<AssetBundle> callback = null)
	{
		yield return null;
        bool bLocalLoad = false;
        string localPath = pathForDocumentsFile(packageUri);
        AssetBundle assetBundle = null;
        if (File.Exists(localPath))
            assetBundle = AssetBundle.LoadFromFile(localPath);
        if (assetBundle != null)
            bLocalLoad = true;
	    if (!bLocalLoad)
	    {
            WWW assetWWW = new WWW(GetPackageUrl(packageUri));
            yield return assetWWW;
            if (CheckError(packageUri, assetWWW))
            {
                assetWWW.Dispose();
                yield break;
            }
            assetBundle = assetWWW.assetBundle;

            string strLocal = pathForDocumentsFile(packageUri);
            string strLocalPath = strLocal.Substring(0, strLocal.LastIndexOf('/'));
            if (!Directory.Exists(strLocalPath))
                Directory.CreateDirectory(strLocalPath);
            byte[] bytes = assetWWW.bytes;
            File.WriteAllBytes(strLocal, bytes);

            assetWWW.Dispose();
            if (assetBundle == null)
            {
                Error(packageUri, "AssetBundle is null.");
                yield break;
            }
	    }

        if (errorSet.Contains(packageUri))
		{
            errorSet.Remove(packageUri);
			yield break;
		}
        if (callback != null)
            callback(assetBundle);
        yield return assetBundle;
	}

	private void InstanceUnload()
	{
		foreach( AssetBundle assetBundle in assetBundleRegistry.Values )
		{
			if( assetBundle != null )
			{
				assetBundle.Unload( true );
			}
		}

		assetBundleRegistry.Clear();
	}
    
    public static string pathForDocumentsFile(string filename)
    {
        if (Application.platform == RuntimePlatform.IPhonePlayer)
        {
            string path = Application.dataPath.Substring(0, Application.dataPath.Length - 5);
            path = path.Substring(0, path.LastIndexOf('/'));
            return Path.Combine(Path.Combine(path, "Documents"), filename);
        }

        else if (Application.platform == RuntimePlatform.Android)
        {
            string path = Application.persistentDataPath;
            path = path.Substring(0, path.LastIndexOf('/'));
            return Path.Combine(path, filename);
        }
        else
        {
            string path = Application.dataPath;
            path = path.Substring(0, path.LastIndexOf('/'));
            return Path.Combine(path, filename);
        }
    }

	private bool CheckError( string packageUri, string errorMessage )
	{
		if( !string.IsNullOrEmpty( errorMessage ) )
		{
			Error( packageUri, errorMessage );

			if( !errorSet.Contains( packageUri ) )
			{
				errorSet.Add( packageUri );
			}

			return true;
		}

		return false;
	}

	private bool CheckError( string packageUri, WWW www )
	{
		return www != null && CheckError( packageUri, www.error );
	}

    private bool CheckError(string packageUri, UnityWebRequest webRequest)
    {
        return webRequest != null && CheckError(packageUri, webRequest.error);
    }

	private string GetPackageUrl( string packageUri )
	{
		string uid = useStreamingAssets ? null : System.Guid.NewGuid().ToString();

#if !UNITY_4
		if( useStreamingAssets )
			return string.Format( "{0}/{1}{2}", PlatformBaseUri, packageUri, packageFileNameSuffix );
		else
			return string.Format( "{0}/{1}{2}?_t={3}", PlatformBaseUri, packageUri, packageFileNameSuffix, uid );
#else
		string packageName = Path.GetFileNameWithoutExtension( packageUri );
		if( useStreamingAssets )
			return string.Format( "{0}/{1}/{2}{3}", PlatformBaseUri, packageUri, packageName, packageFileNameSuffix );
		else
			return string.Format( "{0}/{1}/{2}{3}?_t={4}", PlatformBaseUri, packageUri, packageName, packageFileNameSuffix, uid );
#endif
	}

	private void Error( string packageUri, string errorMessage )
	{
		assetBundleRegistry.Remove( packageUri );
		Debug.LogError( string.Format( "\"{0}\" package error: \"{1}\"", packageUri, errorMessage ) );
	}

	private void Start()
	{
		DontDestroyOnLoad( gameObject );
	}

	private void OnDestroy()
	{
		InstanceUnload();
	}

#if UNITY_4
	private const string packageFileNameSuffix = "-package.unity3d";
	private const string settingsFileNameSuffix = "-settings.asset";
	public const string manifestFileName = "manifest.txt";
#endif
}
