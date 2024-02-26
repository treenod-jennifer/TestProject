using UnityEngine;
using UnityEditor;
using System.Collections;
using System.Collections.Generic;
using System.Text;

#if !UNITY_4
public static class LegacyUpgradeHelper
{
    public static void CheckUpgrade()
    {
        if( PackageSettingsHelper.AllPackageSettings.Count > 0 && UserWantsToUpgrade )
        {
            string title = "RemotePackageManager - Bundles Upgrade!";
            StringBuilder message = new StringBuilder();
            message.AppendLine( "It was detected that you are using the legacy package system in a newer Unity version." );
            message.AppendLine( "Do you wish to upgrade to the new Unity Asset Bundle system?" );
            message.AppendLine( "You may upgrade later at any legacy PackageSettings inspector." );

            if( EditorUtility.DisplayDialog( title, message.ToString(), "Upgrade", "Not now" ) )
            {
                Upgrade();
            }
            else
            {
                UserWantsToUpgrade = false;
            }
        }
    }

    public static void Upgrade()
    {
        foreach( RemotePackageSettings settings in PackageSettingsHelper.AllPackageSettings )
        {
            foreach( Object asset in settings.assets )
            {
                string assetPath = AssetDatabase.GetAssetPath( asset );
                if( string.IsNullOrEmpty( assetPath ) ) break;

                AssetImporter importer = AssetImporter.GetAtPath( assetPath );

                if( importer != null && string.IsNullOrEmpty( importer.assetBundleName ) )
                {
                    importer.assetBundleName = settings.GetPackageUri();
                }
            }
        }

        AssetDatabase.Refresh();

        Debug.Log( "Legacy packages successfully upgraded to the new Unity bundles system! You can delete all PackageSettings now!" );
    }

    private static bool UserWantsToUpgrade
    {
        get { return ManagerSettings.Instance.checkForUpgrade; }
        set { ManagerSettings.Instance.checkForUpgrade = value; ManagerSettings.Instance.Save(); }
    }
}
#endif
