using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditorInternal;
using UnityEngine;
using System;

[System.Serializable]
public class BundleCompareData
{
    public UInt32 crc;
    public string hash;
}

public class RemotePackageManagerWindow : EditorWindow
{
    List<string> delMatFileList = new List<string>();
    List<string> delFileNameList = new List<string>();
    List<string> delFolderList = new List<string>();

    public bool CheckDifferentSelectedBuildTarget()
    {
        if( !ManagerSettings.Instance.checkForDifferentSelectedBuildTarget )
            return true;

        BuildTarget activeTarget = EditorUserBuildSettings.activeBuildTarget;

        bool hasDifferent = false;
        ForEachSelectedBuildTarget( target => {
#if !UNITY_4
            hasDifferent = hasDifferent || ( activeTarget != target.buildTarget );
#else
            hasDifferent = hasDifferent || ( activeTarget != target );
#endif
        }, false );

        if( hasDifferent )
        {
            string dialog = string.Format( "You are about to build packages to a different platform.\nCurrent selected \"{0}\".", activeTarget );
            return EditorUtility.DisplayDialog( "Different Platform Warning", dialog, "Continue", "Cancel" );
        }

        return true;
    }

#if !UNITY_4

    public void ForEachSelectedBuildTarget( System.Action<ManagerSettings.Target> action, bool log = true )
#else
    public void ForEachSelectedBuildTarget( System.Action<BuildTarget> action, bool log = true )
#endif
    {
        if( action == null )
            return;

#if !UNITY_4
        List<ManagerSettings.Target> targets = ManagerSettings.Instance.targets;

        if( targets.Count == 0 )
        {
            if( log )
            {
                Debug.Log( "Please, selecte a Build Target first!" );
            }

            return;
        }

        for( int i = 0; i < targets.Count; i++ )
        {
            ManagerSettings.Target target = targets[i];

            if( target.enabled )
                action( target );
        }
#else
        if( selectedBuildTargets == 0 )
        {
            if( log )
            {
                Debug.Log( "Please, selecte a Build Target first!" );
            }

            return;
        }

        int mask = selectedBuildTargets;
        int count = System.Enum.GetValues( typeof( BuildTarget ) ).Length;

        for( int i = 0; i < count; i++ )
        {
            if( ( mask & 1 ) == 1 )
            {
                BuildTarget buildTarget = IndexToBuildTarget( i );
                action( buildTarget );
            }

            mask >>= 1;
        }
#endif
    }

#if UNITY_4
    private void OnProjectChange()
    {
        if( !ProjectChangeLocked )
        {
            PackageSettingsHelper.RefreshAllPackageSettingsCache();
        }
    }
#endif

    private void OnGUI()
    {
        ShowToolbarMenu();
        EditorGUILayout.Space();

#if !UNITY_4
        ShowPackagesTab();
        ShowUploadTab();
        GUILayout.FlexibleSpace();
#else
        switch( selectedTab )
        {
        case Tab.Packages:
            ShowPackagesTab();
            break;

        case Tab.Upload:
            ShowUploadTab();
            break;

        default:
            break;
        }

        CheckDeleteSettings();
#endif
    }

    private void ShowToolbarMenu()
    {
        EditorGUILayout.BeginHorizontal( EditorStyles.toolbar );

#if !UNITY_4
        EditorGUILayout.LabelField( "Remote Package Manager", EditorStyles.boldLabel );
#else
        switch( selectedTab )
        {
        case Tab.Packages: ShowPackagesHeader(); break;
        case Tab.Upload: ShowUploadHeader(); break;
        default: break;
        }

        GUILayout.FlexibleSpace();

        foreach( System.Enum tabEnum in System.Enum.GetValues( typeof( Tab ) ) )
        {
            Tab tab = ( Tab ) tabEnum;

            if( GUILayout.Toggle( tab == selectedTab, tab.ToString(), EditorStyles.toolbarButton ) )
            {
                selectedTab = tab;
            }
        }
#endif

        EditorGUILayout.EndHorizontal();
    }

    private void ShowBuildTargets()
    {
#if !UNITY_4
        ShowTargetList();
#else
        GUI.tooltip = "Choose in which platform the packages are going to be used.";

        EditorGUI.BeginChangeCheck();
        string[] options = System.Enum.GetNames( typeof( BuildTarget ) );
        selectedBuildTargets = EditorGUILayout.MaskField( selectedBuildTargets, options, EditorStyles.toolbarPopup, GUILayout.Width( 160.0f ) );
        if( EditorGUI.EndChangeCheck() )
        {
            EditorPrefs.SetInt( "RemotePackageManager_SelectedBuildTargets", selectedBuildTargets );
        }
#endif
    }

#if UNITY_4
    private void ShowPackagesHeader()
    {
        EditorGUILayout.BeginHorizontal( EditorStyles.toolbar );
        ShowBuildTargets();
        EditorGUILayout.EndHorizontal();
    }

    private void ShowUploadHeader()
    {
        UploaderSettingsHelper.ShowUploadSettings();
    }
#endif

    private void ShowPackagesTab()
    {
#if !UNITY_4
        ShowBuildPackages();
        ShowBuildTargets();
        if( AssetBundleHelper.ShowAssetBundleMenu() )
        {
            PackageSelectorHelper.BuildPackageSelectorHierarchy();
        }
        PackageSelectorHelper.ShowSelectablePackages();
#else
        ShowBuildPackages();

        EditorGUI.BeginDisabledGroup( showOutdatedPackages );
        EditorGUILayout.HelpBox( "Type to search or create packages.", MessageType.None );
        packagesSearch = EditorHelper.SearchField( packagesSearch );
        EditorGUI.EndDisabledGroup();

        EditorGUILayout.Space();

        scroll = EditorGUILayout.BeginScrollView( scroll );

        if( showOutdatedPackages )
        {
            CheckOutdatedPackages();
            ListPackages( outdatedPackagesSettings );
        }
        else if( string.IsNullOrEmpty( packagesSearch ) )
        {
            HierarchyHelper.DrawHierarchyView();
        }
        else
        {
            ShowSearchCreatePackage();
        }

        EditorGUILayout.EndScrollView();

        Repaint();
#endif
    }

    private void ShowUploadTab()
    {
        Uploader.Get<AmazonS3Uploader, RemotePackageManagerWindow>().ShowSettingsInspector( UploaderSettingsHelper.UploadRecentlyBuiltButton );
        Uploader.Get<FtpUploader, RemotePackageManagerWindow>().ShowSettingsInspector( UploaderSettingsHelper.UploadRecentlyBuiltButton );
    }

#if !UNITY_4
    private static Color32 buildPackagesButtonColor = new Color32( 166, 191, 225, 255 );
    private static Color32 compressBGColor = new Color32( 255, 128, 128, 255 );
    private static bool _compress = false;
    
    private void ShowBuildPackages() {
        GUI.tooltip = "Compress Asset Bundle.";
        //GUI.backgroundColor = compressBGColor;
        _compress = GUILayout.Toggle(_compress, " COMPRESS (LZH)");
         
        GUI.tooltip = "Incrementally build all packages.";
        GUI.backgroundColor = buildPackagesButtonColor;
        if( GUILayout.Button( EditorHelper.Label( "Build Packages." ), GUILayout.Height( 32.0f ) ) )
        {
            if( CheckDifferentSelectedBuildTarget() )
            {
                // 전체 Mat파일 및 Folder데이터 삭제
                this.RemoveAllMatData();

                ForEachSelectedBuildTarget( target => PackageSettingsHelper.BuildAssetBundles( target, false, _compress ) );
                AssetDatabase.Refresh();
                this.ReadAssetBundleList();
            }
        }
        GUI.backgroundColor = Color.white;

        GUI.tooltip = "Force all packages to be rebuilt.";
        if( GUILayout.Button( EditorHelper.Label( "Force Rebuild" ), GUILayout.Height( 24.0f ) ) )
        {
            string dialog = "Are you sure you want to rebuild all packages?\nIt may take a while";
            if( EditorUtility.DisplayDialog( "Force Rebuild", dialog, "Rebuild", "Cancel" ) )
            {
                if( CheckDifferentSelectedBuildTarget() )
                {
                    // 전체 Mat파일 및 Folder데이터 삭제
                    this.RemoveAllMatData();

                    AssetDatabase.DeleteAsset( BuilderHelper.GetRecentlyBuiltFolderPath( true ) );
                    AssetDatabase.DeleteAsset( BuilderHelper.GetRecentlyBuiltFolderPath( false ) );
                    AssetDatabase.Refresh();

                    ForEachSelectedBuildTarget( target => PackageSettingsHelper.BuildAssetBundles( target, true, _compress ) );
                    AssetDatabase.Refresh();
                    this.ReadAssetBundleList();
                }
            }
        }

        GUI.tooltip = "Make fileList of Asset In the CustomFolder";
        if(GUILayout.Button(EditorHelper.Label("Make FileList"), GUILayout.Height(24.0f)))
        {
            RemotePackageMakeFileListWindow eventWindow = ( RemotePackageMakeFileListWindow ) EditorWindow.GetWindow( typeof( RemotePackageMakeFileListWindow ) );
        }
    }

    private void RemoveAllMatData ()
    {
        // mat 파일 및 meta 파일 삭제 ---------------------------------------------
        string _findText = Application.dataPath + "/5_OutResource";
        this.delMatFileList.Clear();
        this.delFileNameList.Clear();
        //MaterialFileSearch
        this.GetSearchMatFile( _findText );

        Debug.Log( "DeleteFileList : " + JsonFx.Json.JsonWriter.Serialize( delFileNameList ) );
        File.WriteAllText( _findText + "/DeleteFileList.text",
             JsonFx.Json.JsonWriter.Serialize( delFileNameList ),
             System.Text.Encoding.UTF8 );

        for ( int i = 0; i < delMatFileList.Count; i++ )
        {
            if ( File.Exists( delMatFileList[i] ) )
            {
                File.Delete( delMatFileList[i] );
            }
        }
        this.delMatFileList.Clear();
        this.delFileNameList.Clear();

        // 파일을 모두 삭제한 후 빈 폴더가 있나 검색
        this.GetSearchEmptyMaterialFolder( _findText );

        // 빈폴더 리스트 로그 및 text로 업데이트
        string pathList = JsonFx.Json.JsonWriter.Serialize( this.delFolderList ).Replace( ",", ",\n" );
        Debug.Log( "DeleteFolderList: " + pathList );
        File.WriteAllText( Application.dataPath + "/5_OutResource/DeleteEmptyFolderList.text",
             pathList,
             System.Text.Encoding.UTF8 );

        for ( int i = 0; i < delFolderList.Count; i++ )
        {
            if ( Directory.Exists( delFolderList[i] ) )
            {
                Directory.Delete( delFolderList[i] );
            }
        }

        this.delFolderList.Clear();

        // --------------------------------------------------------------------------
    }

    private void ReadAssetBundleList ()
    {
        Dictionary<string, BundleCompareData> bundleData = new Dictionary<string, BundleCompareData>();

        string path = Application.dataPath.Replace( "/Assets", "/AssetBundles/" + EditorUserBuildSettings.activeBuildTarget.ToString().ToLower() );
        FileInfo[] fileInfo = new DirectoryInfo ( path ).GetFiles();
        int length = fileInfo.Length;
        for(int i = 0; i < length; i++)
        { 
            FileInfo file = fileInfo[i];
            if( file.Name.IndexOf(".manifest") != -1f )
            {
                string text = file.OpenText().ReadToEnd();
                BundleCompareData data = new BundleCompareData();
                data.crc  = this.GetCRC(text);
                data.hash = this.GetHash(text).ToString();
              
                bundleData.Add( file.Name.Replace( ".manifest", "" ), data );
            }
        }

        File.WriteAllText( path + "/fileList.text", JsonFx.Json.JsonWriter.Serialize(bundleData), System.Text.Encoding.UTF8  );
    }

    /// <summary>
    /// Mat파일 검색
    /// </summary>
    /// <param name="path"></param>
    private void GetSearchMatFile ( string path )
    {
        string[] directoryInfo = Directory.GetDirectories( path );
        FileInfo[] fileInfo = new DirectoryInfo( path ).GetFiles();

        // 전체 폴더 검색
        int dirLength = directoryInfo.Length;
        for ( int i = 0; i < dirLength; i++ )
        {
            string folderPath = directoryInfo[i];
            this.GetSearchMatFile( folderPath );
        }

        int fileLength =  fileInfo.Length;
        for ( int i = 0; i < fileLength; i++ )
        {
            FileInfo file = fileInfo[i];

            if ( file.Name.IndexOf( ".mat" ) != -1f && file.Name.IndexOf( ".mat.meta" ) == -1f )
            {
                string filePath = path + "/" + file.Name;
                filePath = filePath.Replace( "\\", "/" );
                filePath = filePath.Replace( Application.dataPath, "Assets" );
                Material objMat = AssetDatabase.LoadAssetAtPath( filePath, typeof( Material ) ) as Material;
                // Shader Standard일때
                Shader tempShader = Shader.Find( "Standard" );
                if ( objMat.shader == tempShader )
                {
                    delMatFileList.Add( filePath );
                    delFileNameList.Add( file.Name );
                    if ( File.Exists( filePath + ".meta" ) )
                    {
                        delMatFileList.Add( filePath + ".meta" );
                        delFileNameList.Add(file.Name + ".meta");
                    }
                }
            }
        }
    }

    /// <summary>
    /// 빈 폴더 검색
    /// </summary>
    /// <param name="path"></param>
    private void GetSearchEmptyMaterialFolder ( string path )
    {
        // 1.Path를 받음
        // 2.폴더와 파일 정보 받음
        string[] directoryInfo = Directory.GetDirectories( path );
        FileInfo[] fileInfo = new DirectoryInfo( path ).GetFiles();

        // 3. 받은 전체 폴더 검색
        int dirLength = directoryInfo.Length;
        string folderPath = "";
        for ( int i = 0; i < dirLength; i++ )
        {
           folderPath = directoryInfo[i];
           this.GetSearchEmptyMaterialFolder( folderPath );
        }

        bool isFileNull   = ( fileInfo.Length == 0 || fileInfo == null );
        bool isFolderNull = ( dirLength == 0 || directoryInfo == null );

        // 4. 파일 전체 검색
        if ( isFolderNull && isFileNull )
        {
            string folderName = "";
#if UNITY_EDITOR && !UNITY_EDITOR_OSX
            folderName = path.Replace( Path.GetDirectoryName( path ) + "\\", "" );
#elif UNITY_EDITOR_OSX
            folderName = path.Replace( Path.GetDirectoryName( path ) + "/", "" );
#endif
            // 폴더 이름이 Materials이라면
            if ( folderName.Equals( "Materials" ) )
            {
                this.delFolderList.Add( path );
            }
        }
    }

    private UInt32 GetCRC (string manifest)
    {
        return Convert.ToUInt32( GetValue( manifest, 1 ) );
    }

    private Hash128 GetHash (string manifest)
    {
        return Hash128.Parse( GetValue( manifest, 5 ) );
    }
    private string GetValue (string manifest, int index)
    {
        return manifest.Split( "\n".ToCharArray() )[index].Split( ':' )[1].Trim();
    }


#else
    private void ShowBuildPackages()
    {
        EditorGUILayout.BeginHorizontal();

        EditorGUI.BeginDisabledGroup( !Selection.objects.Any( o => o is RemotePackageSettings ) );
        GUI.tooltip = "Build all selected packages.";
        if( GUILayout.Button( EditorHelper.Label( "Build Selected" ), EditorStyles.miniButtonLeft ) )
        {
            IEnumerable<RemotePackageSettings> allSettings = Selection.objects.Where( o => o is RemotePackageSettings ).Cast<RemotePackageSettings>();

            ForEachSelectedBuildTarget( buildTarget => PackageSettingsHelper.BuildAssetBundles( allSettings, buildTarget ) );
        }
        EditorGUI.EndDisabledGroup();

        if( showOutdatedPackages )
        {
            GUI.tooltip = "Build only outdated packages.";
            if( GUILayout.Button( EditorHelper.Label( "Build Outdated" ), EditorStyles.miniButtonMid ) )
            {
                ForEachSelectedBuildTarget( buildTarget => PackageSettingsHelper.BuildAssetBundles( outdatedPackagesSettings, buildTarget ) );
                showOutdatedPackages = false;
            }
        }
        else
        {
            GUI.tooltip = "Rebuild all packages from scratch.";
            if( GUILayout.Button( EditorHelper.Label( "Rebuild All" ), EditorStyles.miniButtonMid ) )
            {
                if( EditorUtility.DisplayDialog( "Rebuild All", "Are you sure you want to rebuild all packages?\nIt may take a while", "Rebuild", "Cancel" ) )
                {
                    PackageSettingsHelper.RefreshAllPackageSettingsCache();

                    ForEachSelectedBuildTarget( buildTarget => PackageSettingsHelper.BuildAssetBundles( PackageSettingsHelper.AllPackageSettings, buildTarget ) );
                }
            }
        }

        EditorGUI.BeginDisabledGroup( !Directory.Exists( BuilderHelper.recentlyBuiltFolderPath ) );
        GUI.tooltip = "Delete the 'AssetBundles_RecentlyBuilt' temp folder.";
        if( GUILayout.Button( EditorHelper.Label( "Clear Recently Built" ), EditorStyles.miniButtonRight ) )
        {
            AssetDatabase.DeleteAsset( BuilderHelper.recentlyBuiltFolderPath );
        }
        EditorGUI.EndDisabledGroup();

        GUILayout.FlexibleSpace();

        bool noOutdatedPackages = outdatedPackagesSettings.Count == 0;
        if( noOutdatedPackages )
        {
            showOutdatedPackages = false;
        }

        EditorGUI.BeginDisabledGroup( noOutdatedPackages );
        GUI.tooltip = "Show only outdated packages.";
        showOutdatedPackages = GUILayout.Toggle( showOutdatedPackages, EditorHelper.Label( "Show Outdated" ), EditorStyles.miniButton );
        EditorGUI.EndDisabledGroup();

        EditorGUILayout.EndHorizontal();
    }

    private void ShowSearchCreatePackage()
    {
        string lowerSearch = packagesSearch.ToLower();

        IEnumerable<RemotePackageSettings> filteredSettings = PackageSettingsHelper.AllPackageSettings.Where( settings => {
            return settings.name.ToLower().Contains( lowerSearch );
        } );

        if( filteredSettings.Count() > 0 )
        {
            ListPackages( filteredSettings );
        }
        else if( GUILayout.Button( "Create Package" ) )
        {
            packagesSearch = packagesSearch.Replace( " ", "" );

            string newPackagePath = Path.Combine( BuilderHelper.assetBundlesFolderPath, packagesSearch );
            BuilderHelper.CreateFolderTree( newPackagePath );

            RemotePackageSettings settings = PackageSettingsHelper.Get( newPackagePath );
            Selection.activeObject = settings;
            EditorGUIUtility.PingObject( settings );

            packagesSearch = "";
            PackageSettingsHelper.RefreshAllPackageSettingsCache();
        }
    }

    private void ListPackages( IEnumerable<RemotePackageSettings> settingsCollection )
    {
        foreach( RemotePackageSettings settings in settingsCollection )
        {
            Rect position = EditorHelper.Rect( EditorStyles.label );

            if( Event.current.type == EventType.MouseDown )
            {
                bool mouseOver = position.Contains( Event.current.mousePosition );
                DragDropHelper.SelectSettings( settings, mouseOver );
            }

            DragDropHelper.ShowSelection( position, settings );
            EditorGUI.LabelField( position, settings.name );
        }
    }

    private void CheckOutdatedPackages()
    {
        if( checkedOutdated )
            return;

        outdatedPackagesSettings.Clear();

        foreach( RemotePackageSettings settings in PackageSettingsHelper.AllPackageSettings )
        {
            CheckOutdated( settings );
        }

        checkedOutdated = true;
    }

    private void CheckOutdated( RemotePackageSettings settings )
    {
        string settingsPath = AssetDatabase.GetAssetPath( settings );
        System.DateTime settingsTime = File.GetLastWriteTime( settingsPath );

        List<System.DateTime> packagesTime = new List<System.DateTime>();
        ForEachSelectedBuildTarget( target => {
            string packagePath = RemotePackageManager.GetPackageFilePath( settings.GetPackageFolderPath(), target );
            packagesTime.Add( File.GetLastWriteTime( packagePath ) );
        } );

        foreach( Object asset in settings.assets )
        {
            string assetPath = AssetDatabase.GetAssetPath( asset );

            List<string> dependencies = AssetDatabase.GetDependencies( new string[] { assetPath } ).ToList();
            dependencies.Add( assetPath );

            bool anyOutdatedDependency = dependencies.Any( d => {
                if( string.IsNullOrEmpty( d ) )
                    return false;

                System.DateTime lastWriteTime = File.GetLastWriteTime( d );
                bool anyPackageOutDated = packagesTime.Any( time => time.CompareTo( lastWriteTime ) < 0 );

                return anyPackageOutDated || settingsTime.CompareTo( lastWriteTime ) < 0;
            } );

            if( anyOutdatedDependency )
            {
                outdatedPackagesSettings.Add( settings );
                break;
            }
        }
    }

    private void CheckDeleteSettings()
    {
        if( Event.current.type == EventType.KeyUp && Event.current.keyCode == KeyCode.Delete )
        {
            foreach( Object obj in Selection.objects )
            {
                if( obj is RemotePackageSettings )
                {
                    RemotePackageSettings settings = obj as RemotePackageSettings;
                    TryDeletePackage( settings.GetPackageFolderPath() );
                }
            }
        }
    }

    private void TryDeletePackage( string packageFolderPath )
    {
        if( selectedTab != Tab.Packages )
            return;

        string message = "Are you sure you want to delete all selected packages?\nIt's a destructive operation!";
        if( EditorUtility.DisplayDialog( "Delete Selected", message, "Ok", "Cancel" ) )
        {
            AssetDatabase.MoveAssetToTrash( packageFolderPath );
        }
    }
#endif

#if !UNITY_4

    private static void InitializeTargetList()
    {
        if( targetList != null )
            return;

        targetList = new ReorderableList( ManagerSettings.Instance.targets, typeof( ManagerSettings.Target ) );
        targetList.drawHeaderCallback += DrawTargetListHeader;
        targetList.drawElementCallback += DrawTargetListElement;
    }

    private static void ShowTargetList()
    {
        InitializeTargetList();

        if( showTargetList )
        {
            targetList.DoLayoutList();
        }
        else
        {
            GUI.tooltip = "Choose in which platform the packages are going to be used.";
            showTargetList = GUILayout.Toggle( showTargetList, EditorHelper.Label( "Build Targets" ), EditorHelper.Styles.Minimized );
        }
    }

    private static void DrawTargetListHeader( Rect rect )
    {
        GUI.tooltip = "Choose in which platform the packages are going to be used.";
        showTargetList = GUI.Toggle( rect, showTargetList, EditorHelper.Label( "Build Targets" ), EditorStyles.label );
    }

    private static void DrawTargetListElement( Rect rect, int index, bool isActive, bool isFocused )
    {
        float enabledToggleWidth = 16.0f;
        float localToggleWidth = 48.0f;

        Rect enabledToggleRect = new Rect( rect );
        enabledToggleRect.width = enabledToggleWidth;
        Rect popupRect = new Rect( rect );
        popupRect.xMin = enabledToggleRect.xMax;
        popupRect.xMax -= localToggleWidth;
        Rect localToggleRect = new Rect( rect );
        localToggleRect.xMin = popupRect.xMax;
        localToggleRect.height = 16.0f;

        ManagerSettings.Target target = ManagerSettings.Instance.targets[index];

        EditorGUI.BeginChangeCheck();

        target.enabled = EditorGUI.Toggle( enabledToggleRect, target.enabled );
        target.buildTarget = ( BuildTarget ) EditorGUI.EnumPopup( popupRect, target.buildTarget );
        target.useStreamingAssets = GUI.Toggle( localToggleRect, target.useStreamingAssets, "Local", EditorStyles.miniButton );

        if( EditorGUI.EndChangeCheck() )
        {
            ManagerSettings.Instance.Save();
        }
    }

#else

    private static BuildTarget IndexToBuildTarget( int index )
    {
        if( indexToBuildTarget.Count == 0 )
        {
            System.Array values = System.Enum.GetValues( typeof( BuildTarget ) );

            for( int i = 0; i < values.Length; i++ )
            {
                indexToBuildTarget.Add( i, ( BuildTarget ) values.GetValue( i ) );
            }
        }

        return indexToBuildTarget[index];
    }

#endif

#if UNITY_4
    public Vector2 scroll = Vector2.zero;
    public Tab selectedTab = Tab.Packages;
    public int selectedBuildTargets = 0;
    public string packagesSearch = "";

    public bool showOutdatedPackages = false;
    public List<RemotePackageSettings> outdatedPackagesSettings = new List<RemotePackageSettings>();
    private bool checkedOutdated = false;

    public bool clearAfterUpload = false;
#endif

    private bool hasInitialized = false;

    public static RemotePackageManagerWindow Window
    { get { return GetWindow( false ); } }

    public static bool ProjectChangeLocked
    { get; set; }

#if !UNITY_4
    private static ReorderableList targetList;
    private static bool showTargetList = true;
#else
    private static Dictionary<int, BuildTarget> indexToBuildTarget = new Dictionary<int, BuildTarget>();
#endif

    static private int? defaultBuildTargetMask;

    static private int DefaultBuildTargetMask
    {
        get
        {
            if( !defaultBuildTargetMask.HasValue )
            {
                BuildTarget defaultBuildTarget = BuildTarget.StandaloneWindows;
                defaultBuildTargetMask = 1 << System.Array.IndexOf( System.Enum.GetValues( typeof( BuildTarget ) ), defaultBuildTarget );
            }

            return defaultBuildTargetMask.Value;
        }
    }

    public enum Tab
    {
        Packages,
        Upload
    }

    [MenuItem( "Window/Remote Package Manager" )]
    public static void OpenWindow()
    {
        GetWindow( true );
    }

    private static RemotePackageManagerWindow GetWindow( bool focus )
    {
        RemotePackageManagerWindow window = EditorWindow.GetWindow<RemotePackageManagerWindow>( "Pkg Manager", focus );
        window.Initialize();
        return window;
    }

    private void OnFocus()
    {
        LoadPrefs();
    }

    private void OnSelectionChange()
    {
        Repaint();
    }

    private void Initialize()
    {
        if( !hasInitialized )
        {
            hasInitialized = true;

            minSize = new Vector2( 400.0f, 400.0f );
            LoadPrefs();
            VersionHelper.CheckVersion();

#if !UNITY_4
            PackageSettingsHelper.RefreshAllPackageSettingsCache();
            LegacyUpgradeHelper.CheckUpgrade();
#endif
        }
    }

    private void LoadPrefs()
    {
#if !UNITY_4
        PackageSelectorHelper.BuildPackageSelectorHierarchy();
        AssetBundleHelper.UpdateMenuOptions();
#else
        clearAfterUpload = EditorPrefs.GetBool( "RemotePackageManager_ClearAfterUpload", clearAfterUpload );
        selectedBuildTargets = EditorPrefs.GetInt( "RemotePackageManager_SelectedBuildTargets", DefaultBuildTargetMask );
        PackageSettingsHelper.RefreshAllPackageSettingsCache();
        checkedOutdated = false;
#endif
    }
}
