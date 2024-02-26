using UnityEngine;
using System.Collections;
using System.IO;

public static class FileHelper
{
    public static long? GetFileSize( string filePath )
    {
        FileInfo fileInfo = new FileInfo( filePath );
        return fileInfo.Exists ? ( long? ) fileInfo.Length : null;
    }

    public static string GetSizeString( long size )
    {
        int i = 0;
        for( ; i < sizeNames.Length; i++ )
        {
            if( size < GetPowerOf2( i + 1 ) )
                break;
        }

        return string.Format( "{0:0.00} {1}", ( float ) size / GetPowerOf2( i ), sizeNames[i] );
    }

    private static int GetPowerOf2( int n )
    {
        return 1 << ( n * 10 );
    }

    private static readonly string[] sizeNames = { "B", "KB", "MB" };
}
