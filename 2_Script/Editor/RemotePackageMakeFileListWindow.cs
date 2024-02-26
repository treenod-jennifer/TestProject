using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System.IO;
using System.Linq;
using System;

public class RemotePackageMakeFileListWindow : EditorWindow 
{
    public string _findText = "";

    public void OnGUI ()
    {
        EditorGUILayout.BeginHorizontal();
        _findText = EditorGUILayout.TextField( "CurrentAssetPath", _findText );
        EditorGUILayout.EndHorizontal();

        this.ShowButtonList();
    }

    private void ShowButtonList ()
    {
        GUI.tooltip = "Read Asset In the CustomFolder Path";
        if ( GUILayout.Button( EditorHelper.Label( "ReadFilePath" ), GUILayout.Height( 60.0f ) ) )
        {
            this.OpenFindDirectory();
        }

        GUI.tooltip = "MakeFilelist";
        if ( GUILayout.Button( EditorHelper.Label( "MakeFilelist" ), GUILayout.Height( 60.0f ) ) )
        {
            if ( _findText.Equals( "" ))
            {
                EditorUtility.DisplayDialog( "Select FolderPath", "You must select a Folder Path!", "OK" );
                return;
            }
            this.ReadAssetBundleList();
        }
    }

    void OpenFindDirectory ()
    {
        _findText = EditorUtility.OpenFolderPanel( "Overwrite with folder", "", "" );
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

    private void ReadAssetBundleList ()
    {
        Dictionary<string, BundleCompareData> bundleData = new Dictionary<string, BundleCompareData>();

        FileInfo[] fileInfo = new DirectoryInfo( _findText ).GetFiles();
        int length = fileInfo.Length;
        
        string originText = "";

        // 모든 매니페스트를 읽어옴
        for ( int i = 0; i < length; i++ )
        {
            FileInfo file = fileInfo[i];
            if ( file.Name.IndexOf( ".manifest" ) != -1f )
            {
                StreamReader reader = file.OpenText();
                string text = reader.ReadToEnd();
                BundleCompareData data = new BundleCompareData();
                data.crc = this.GetCRC( text );
                data.hash = this.GetHash( text ).ToString();

                bundleData.Add( file.Name.Replace( ".manifest", "" ), data );
                reader.Close();   
            }
            else if ( file.Name.Equals ( "fileList.text" ) ) // 파일리스트라면 해당 파일리스트 값 저장
            {
                StreamReader reader = file.OpenText();
                originText = reader.ReadToEnd();
                reader.Close();
            }
        }

        string filePath = _findText + "/fileList.text";
        if ( File.Exists( filePath ) )
        {
            if ( File.Exists( _findText + "/ChangeFileList.text" ) )
            {
                File.Delete( _findText + "/ChangeFileList.text" );
            }

            File.WriteAllText( _findText + "/ChangeFileList.text",
                this.CompareChangeAssetBundleList( JsonFx.Json.JsonReader.Deserialize<Dictionary<string, BundleCompareData>>( originText ), bundleData ), 
                System.Text.Encoding.UTF8 );
            File.Delete( filePath );
        }

        File.WriteAllText( _findText + "/fileList.text", JsonFx.Json.JsonWriter.Serialize( bundleData ), System.Text.Encoding.UTF8 );

        EditorUtility.DisplayDialog( "Complete Make FileList", "Complete Make FileList!", "OK" );
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="originData">원래의 FileList데이터</param>
    /// <param name="changeBundleData"> 바뀐 manifest값 데이터 </param>
    /// <returns></returns>
    private string CompareChangeAssetBundleList ( Dictionary<string, BundleCompareData> originDataList, Dictionary<string, BundleCompareData> changeBundleData )
    {
       List<string> changeOriginCompareKeyList    = this.CompareData ( originDataList, changeBundleData );
       List<string> changeNewBundleCompareKeyList = this.CompareData ( changeBundleData, originDataList );

       List<string> resultKeyList = changeOriginCompareKeyList;

        for(int i = 0; i < changeNewBundleCompareKeyList.Count; i++)
        {
            if( resultKeyList.Contains( changeNewBundleCompareKeyList[i] ) )
                continue;

            resultKeyList.Add(changeNewBundleCompareKeyList[i]);
        }

        string reuslt = " ChangerFileList : " + JsonFx.Json.JsonWriter.Serialize( resultKeyList );
        Debug.Log(reuslt);
        return reuslt;
    }


    private List<string> CompareData ( Dictionary<string, BundleCompareData> originDataList, Dictionary<string, BundleCompareData> changeBundleData )
    {
        // 원래의 데이터 키값들, ValueList들
        List<BundleCompareData> dataList = originDataList.Values.ToList();
        List<string> keyDataList = originDataList.Keys.ToList();

        List<string> changeOriginKeyList = new List<string>();

        string curKey = "";
        for ( int i = 0; i < keyDataList.Count; i++ )
        {
            curKey = keyDataList[i];
            if ( changeBundleData.ContainsKey( curKey ) )
            {
                if ( dataList[i].crc.Equals( changeBundleData[curKey].crc ) == false ||
                    dataList[i].hash.Equals( changeBundleData[curKey].hash ) == false )
                {
                    changeOriginKeyList.Add( curKey );
                }
            }
            else
            {
                changeOriginKeyList.Add( curKey );
            }
        }

        return changeOriginKeyList;
    }
}
