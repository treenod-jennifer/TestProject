using UnityEngine;
using UnityEditor;
using System.Collections;

public static class VersionHelper
{
    public const string Version = "3.2";

    public static void CheckVersion()
    {
        string versionKey = string.Concat( typeof( RemotePackageManager ).Name, "_", typeof( VersionHelper ).Name );
        EditorPrefs.GetString( versionKey, Version );

        if( !Version.Equals( versionKey ) )
        {
            EditorPrefs.SetString( versionKey, Version );
        }
    }
}
