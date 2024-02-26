public static class BatchHelper
{
    public static void RebuildAll()
    {
#if !UNITY_4
        RemotePackageManagerWindow.Window.ForEachSelectedBuildTarget( buildTarget => {
            PackageSettingsHelper.BuildAssetBundles( buildTarget, true, false );
        } );
#else
        RemotePackageManagerWindow.Window.ForEachSelectedBuildTarget( buildTarget => {
            PackageSettingsHelper.BuildAssetBundles( PackageSettingsHelper.AllPackageSettings, buildTarget );
        } );
#endif
    }
}
