using System;
using System.IO;
using System.Text;
using UnityEditor;
using UnityEngine;

public class Edit_EventStage_DummyMaker : EditorWindow
{
    enum ProcessState
    {
        None,
        Copy,
        MetaDataTransfer,
        ConnectPrefab
    }
    
    private ProcessState _processState = ProcessState.None;
    private EVENT_CHAPTER_TYPE _eventType = EVENT_CHAPTER_TYPE.FAIL_RESET;
    private static string _eventNumber;
    private GameObject _dummyPrefab;
    private string _dummyName = @"event_v2_dummy_1\";
    
    [MenuItem("Poko/MakeBundle/EventStage Dummy Maker")]
    private static void OpenWindow()
    {
        GetWindow<Edit_EventStage_DummyMaker>();
    }

    private void OnGUI()
    {
        //이벤트 타입 -> 복사 더미 설정
        GUILayout.Label("이벤트 타입");
        GUILayout.Label(_eventType.ToString());
        GUILayout.BeginHorizontal();
        if(GUILayout.Button("연속"))
        {
            _eventType = EVENT_CHAPTER_TYPE.FAIL_RESET;
            _dummyName = "event_v2_dummy_1/";
        }
        if(GUILayout.Button("순차"))
        {
            _eventType = EVENT_CHAPTER_TYPE.COLLECT;
            _dummyName = "event_v2_dummy_2/";
        }
        if(GUILayout.Button("스코어"))
        {
            _eventType = EVENT_CHAPTER_TYPE.SCORE;
            _dummyName = "event_v2_dummy_3/";
        }
        GUILayout.EndHorizontal();
        
        //이벤트 번호 -> 이름 변경
        GUILayout.Label("이벤트 인덱스");
        _eventNumber = GUILayout.TextField(_eventNumber);

        //확인 -> 더미 복사, 이름 변경, 프리펩 이미지 연결 수정
        if (GUILayout.Button("Make Dummy"))
        {
            if (_processState != ProcessState.None)
                return;
            
            MakeDummy();
        }
    }

    //더미 선택
    private void MakeDummy()
    {
        
        try
        {
            _processState = ProcessState.Copy;
            
            DirectoryInfo fromDir = null;
            fromDir = new DirectoryInfo(GetPath(false));
            if (fromDir.Exists == false)
            {
                Debug.Log("더미가 하위 경로에 존재하지 않습니다.");
                return;
            }
            
            DirectoryInfo newDir = null;
            newDir = new DirectoryInfo(GetPath(true, true));
            if (newDir.Exists)
            {
                Debug.Log("이미 존재하는 이벤트 인덱스입니다.");
                return;
            }
        
            //더미 복사
            new DirectoryInfo(GetPath(parent:false)).CreateSubdirectory($"event_v2_{_eventNumber}");
            FileInfo[] fromFiles = fromDir.GetFiles("*");
            
            for(int i = 0; i < fromFiles.Length; i++)
            {
                if (fromFiles[i].Name.Contains(".meta"))
                    continue;
                
                string toMetaPath = GetPath(fileName: fromFiles[i].Name);
                fromFiles[i].CopyTo(toMetaPath);
                AssetDatabase.ImportAsset(toMetaPath, ImportAssetOptions.Default);
            }
            
            _processState = ProcessState.MetaDataTransfer;
            
            //더미 메타 파일 데이터 이전
            DirectoryInfo toDir = new DirectoryInfo(GetPath());
            FileInfo[] toFiles = toDir.GetFiles("*");
            string[] fromGuids = new string[fromFiles.Length];
            string[] toGuids = new string[toFiles.Length];
            
            for(int i = 0; i < fromFiles.Length; i++)
            {
                if (fromFiles[i].Name.Contains(".meta") == false)
                {
                    //프리펩 데이터 인덱스 변경
                    string[] fromIdxx = toFiles[i].Name.Split('_');
                    if (fromIdxx.Length > 1)
                    {
                        string path = toFiles[i].FullName.Replace(fromIdxx[1], _eventNumber);
                        toFiles[i].MoveTo(path);
                        toFiles[i] = new FileInfo(path);
                    }
                    continue;
                }
                
                //기존 메타의 내용을 복사 및 guid 추출
                
                FileStream changeStream = fromFiles[i].Open(FileMode.Open);
                string changeText = StringToStream(changeStream);
                fromGuids[i] = GetGuid(GetPath(false, true, fileName: fromFiles[i].Name));
                toGuids[i] = GetGuid(GetPath(fileName: fromFiles[i].Name));
                changeText = changeText.Replace(fromGuids[i], toGuids[i]);
                
                //새 메타의 내용에 기존 메타 내용 replace guid를 넣음
                byte[] bytes = Encoding.UTF8.GetBytes(changeText);
                changeStream = toFiles[i].OpenWrite();
                changeStream.Write(bytes,0,bytes.Length);
                changeStream.Close();
                
                //프리펩 데이터 인덱스 변경
                string[] fromIdx = toFiles[i].Name.Split('_');
                if (fromIdx.Length > 1)
                {
                    string path = toFiles[i].FullName.Replace(fromIdx[1], _eventNumber);
                    toFiles[i].MoveTo(path);
                    toFiles[i] = new FileInfo(path);
                }
            }
            AssetDatabase.Refresh();
            
            _processState = ProcessState.ConnectPrefab;
            
            //프리펩 참조 guid 변경
            for (int i = 0; i < toFiles.Length; i++)
            {
                //프리펩 파일 추출
                if(toFiles[i].Name.Contains(".png"))
                    continue;

                //프리펩 파일에 연결된 이전 guid를 새로 변경된 guid로 변경
                FileStream changeStream = toFiles[i].Open(FileMode.Open);
                string changeText = StringToStream(changeStream);
                string dummyText = changeText.Replace("\r\n", "00");
                for (int j = 0; j < fromGuids.Length; j++)
                {
                    if (fromGuids[j] != null)
                    {
                        string fromG = fromGuids[j].Replace("\r", "");
                        int posIdx = dummyText.IndexOf(fromG);

                        while (posIdx > 0)
                        {
                            changeText = changeText.Remove(posIdx, fromG.Length);
                            changeText = changeText.Insert(posIdx, toGuids[j].Replace("\r", ""));
                            
                            dummyText = changeText.Replace("\r\n", "00");
                            posIdx = dummyText.IndexOf(fromG);
                        }
                    }
                }
                
                byte[] bytes = Encoding.UTF8.GetBytes(changeText);
                changeStream = toFiles[i].OpenWrite();
                changeStream.Write(bytes,0,bytes.Length);
                changeStream.Close();
            }
            
            if (_eventType == EVENT_CHAPTER_TYPE.SCORE)
            {
                //에셋 연결 수정
                string atlasPathBase = $"{GetPath()}event_{_eventNumber}_Atlas.";
                string atlasPath = atlasPathBase + "asset";
                string atlasMaterialPath = atlasPathBase + "mat";
                string atlasPngPath = atlasPathBase + "png";

                NGUISettings.atlas = AssetDatabase.LoadAssetAtPath<NGUIAtlas>(atlasPath);
                Material mat = AssetDatabase.LoadAssetAtPath<Material>(atlasMaterialPath);
                NGUISettings.atlas.spriteMaterial = mat;
                Texture2D tex = AssetDatabase.LoadAssetAtPath<Texture2D>(atlasPngPath);
                NGUISettings.atlas.spriteMaterial.mainTexture = tex;
            }
            
            //성공 로그
            Debug.Log($"번들 생성 성공 : {_eventNumber}번 {_eventType.ToString()} 이벤트");
            
            _processState = ProcessState.None;
        }
        catch (Exception e)
        {
            Debug.Log(e);
            throw;
        }
        finally
        {
            AssetDatabase.Refresh();
            if (_processState != ProcessState.None)
            {
                Debug.Log("error : " + _processState);
                _processState = ProcessState.None;
            }
        }
    }

    private string GetPath(bool toPath = true, bool parent = true, string fileName = "")
    {
        string path;
        if (toPath)
        {
            path = "Assets/5_OutResource/events/";
            if (parent)
                path += "event_v2_[_eventNumber]/".Replace("[_eventNumber]", _eventNumber);
        }
        else
        {
            path = "Assets/z.Tester/Dummy/Event_dummy/";
            if (parent)
                path += _dummyName;
        }
        if(parent)
            path += fileName;
        return path;
    }

    private static string StringToStream(FileStream input)
    {
        using (FileStream fstream = input)
        {
            byte[] array = new byte[fstream.Length];
            fstream.Read(array, 0, array.Length);
            return Encoding.Default.GetString(array);
        }
    }

    private string GetGuid(string path)
    {
        FileInfo meta = new FileInfo(path);
        FileStream fromFs = meta.Open(FileMode.Open);
        string str = StringToStream(fromFs);
        string[] fromText1 = str.Split('\n');
        string[] fromText2 = fromText1[1].Split(' ');
        fromFs.Close();
        return fromText2[1];
    }
}
