using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.Networking;

public class ManagerAssetBundle : MonoBehaviour
{
	public const string packageFileNameSuffix = "";
	public bool useStreamingAssets = false;
	public string defaultVariant = "";
	public int selectedBaseUriIndex = 0;

	public List<string> baseUris = new List<string>( new[] { "https://s3.amazonaws.com/_your_bucket_name_/" } );

	private const string settingsFileNameSuffix = "";
	private static ManagerAssetBundle instance = null;

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
			ManagerAssetBundle manager = GetInstance( true );
			return manager != null ? manager.SelectedBaseUri : "";
		}
	}

	/// <summary>
	/// Change the selected BaseUri given its new index. If no RemotePackageManager is instantiated, -1 is returned.
	/// </summary>
	public static int SelectedBaseUriIndex
	{
		get
		{
			ManagerAssetBundle manager = GetInstance( true );
			return manager != null ? manager.selectedBaseUriIndex : -1;
		}

		set
		{
			ManagerAssetBundle manager = GetInstance( true );
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

    public static void Load(string packageUri, string packageVariant = null, System.Action<AssetBundle> callback = null)
	{
		ManagerAssetBundle manager = GetInstance( true );
	    if (manager != null)
            manager.InstanceLoad( packageUri, packageVariant, callback );
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
	public static string GetExportedPackageFilePath( string packagePath )
	{
		return string.Concat( packagePath, packageFileNameSuffix );
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

	public static ManagerAssetBundle GetInstance( bool showError = false )
	{
		if( instance == null )
		{
			instance = Object.FindObjectOfType( typeof( ManagerAssetBundle ) ) as ManagerAssetBundle;

			if( instance == null && showError )
			{
				Debug.LogError( "Could not locate an instance of " + typeof( RemotePackageManager ).Name + "!" );
			}
		}

		return instance;
	}

	public string GetVariantPackageUri( string packageUri, string packageVariant )
	{
		packageVariant = string.IsNullOrEmpty( packageVariant ) ? defaultVariant : packageVariant;
		if( !string.IsNullOrEmpty( packageVariant ) )
		{
			return string.Concat( packageUri, ".", packageVariant );
		}

		return packageUri;
	}

    private void InstanceLoad( string packageUri, string packageVariant, System.Action<AssetBundle> callback = null )
	{
		packageUri = GetVariantPackageUri( packageUri.ToLower(), packageVariant );
        StartCoroutine(InstanceLoadSync(packageUri, callback));
	}
  /*  static IEnumerator InstanceLoadSync(WWW www, IObserver<AssetBundle> observer, IProgress<float> reportProgress, CancellationToken cancel)
    {
        using (www)
        {
            if (reportProgress != null)
            {
                while (!www.isDone && !cancel.IsCancellationRequested)
                {
                    try
                    {
                        reportProgress.Report(www.progress);
                    }
                    catch (Exception ex)
                    {
                        observer.OnError(ex);
                        yield break;
                    }
                    yield return null;
                }
            }
            else
            {
                if (!www.isDone)
                {
                    yield return www;
                }
            }

            if (cancel.IsCancellationRequested)
            {
                yield break;
            }

            if (reportProgress != null)
            {
                try
                {
                    reportProgress.Report(www.progress);
                }
                catch (Exception ex)
                {
                    observer.OnError(ex);
                    yield break;
                }
            }

            if (!string.IsNullOrEmpty(www.error))
            {
                observer.OnError(new WWWErrorException(www, ""));
            }
            else
            {
                observer.OnNext(www.assetBundle);
                observer.OnCompleted();
            }
        }
    }*/
	
	// FOR ManagerData.cs
    public IEnumerator InstanceLoadSync(string packageUri,System.Action<float> reportProgress, System.Action<AssetBundle> callback = null)
    {
        yield return null;
        bool bLocalLoad = false;
        string localPath = pathForDocumentsFile(packageUri);

       
        AssetBundle assetBundle = null;
	    if (File.Exists(localPath)) {
		   
		   // assetBundle = AssetBundle.LoadFromFile(localPath);
		    var bundleRequest = AssetBundle.LoadFromFileAsync(localPath);
		    yield return bundleRequest;
		    assetBundle = bundleRequest.assetBundle;

	    }
	    if (assetBundle != null)
        {
            bLocalLoad = true;
            if (reportProgress != null)
                reportProgress(1f);
        }

        if (!bLocalLoad)
        {
            WWW assetWWW = new WWW(GetPackageUrl(packageUri));
			Debug.Log("[loading] Asset Bundle in WWW: " + packageUri);
            while (!assetWWW.isDone)
            {
                if (reportProgress != null)
                    reportProgress(assetWWW.progress);
                yield return null;
            }
            if (reportProgress != null)
                reportProgress(1f);

            if (CheckError(packageUri, assetWWW))
            {
                assetWWW.Dispose();
                yield break;
            }
            assetBundle = assetWWW.assetBundle;

            string strLocal = pathForDocumentsFile(packageUri);

            byte[] bytes = assetWWW.bytes;
            File.WriteAllBytes(strLocal, bytes);
            Debug.Log(strLocal);
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


    public IEnumerator InstanceLoadSync(string packageUri, System.Action<AssetBundle> callback = null)
	{
		yield return null;
        bool bLocalLoad = false;
        string localPath = pathForDocumentsFile(packageUri);

       // Debug.Log(localPath);

        AssetBundle assetBundle = null;
		if (File.Exists(localPath)) {
			//assetBundle = AssetBundle.LoadFromFile(localPath);
			var bundleRequest = AssetBundle.LoadFromFileAsync(localPath);
			yield return bundleRequest;
			assetBundle = bundleRequest.assetBundle;
		}
		
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
          //  Debug.Log(strLocal);
            //string strLocalPath = strLocal.Substring(0, strLocal.LastIndexOf('/'));
            //if (!Directory.Exists(strLocalPath))
              //  Directory.CreateDirectory(strLocalPath);
            byte[] bytes = assetWWW.bytes;
            File.WriteAllBytes(strLocal, bytes);
            Debug.Log(strLocal);
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

    public static string pathForDocumentsFile(string filename)
    {
      //  string path = Application.persistentDataPath + Global.gameDataDirectory;
        //path = path.Substring(0, path.LastIndexOf('/'));

        return Path.Combine(Global.gameDataDirectory, filename);

       /* if (Application.platform == RuntimePlatform.IPhonePlayer)
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
        }*/
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

	private string GetPackageUrl( string packageUri )
	{
		string uid = useStreamingAssets ? null : System.Guid.NewGuid().ToString();

		if( useStreamingAssets )
			return string.Format( "{0}/{1}{2}", PlatformBaseUri, packageUri, packageFileNameSuffix );
		else
			return string.Format( "{0}/{1}{2}?_t={3}", PlatformBaseUri, packageUri, packageFileNameSuffix, uid );
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
}
