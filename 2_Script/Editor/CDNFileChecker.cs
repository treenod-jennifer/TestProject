using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEditor;
using System.Linq;
using System.Net;
using Newtonsoft.Json;

namespace CDNFileCheck
{
    //CDN 디렉토리 선택 창입니다.
    public class CDNFileChecker : EditorWindow
    {
        enum Window
        {
            serverSelect,
            cdnDirectory
        }

        private Window window = Window.serverSelect;
        
        private       Dictionary<string, string> cdnServerMap = new Dictionary<string, string>();
        private       string                     curServer;                  // 현재 선택된 서버
        private       DirectoryInfo              curDir;                     // 현재 선택된 CDN 하위 디렉토리
        private       DirectoryInfo[]            curChildDirs;               // 현재 디렉토리의 하위 디렉토리
        private       List<string>               paths = new List<string>(); // 상위 디렉토리 ~ 현재 디렉토리까지 이름이 순차로 들어있는 리스트
        public static CDNFileChecker             openedWindow;
        private       Vector2                    scroll = Vector2.zero;
        
        [MenuItem("Poko/CDNFileChecker")]
        private static void OpenWindow()
        {
            openedWindow = GetWindow<CDNFileChecker>();
        }

        private void OnGUI()
        {
            if (cdnServerMap.Count == 0)
            {
                ServerLoad();
            }

            //서버의 CDN 리스트 선택 시 디렉토리로 이동합니다.
            if (window == Window.serverSelect)
            {
                openedWindow.titleContent.text = "서버 선택창";
                var enumerator = cdnServerMap.GetEnumerator();
                while (enumerator.MoveNext())
                {
                    if (GUILayout.Button(enumerator.Current.Key))
                    {
                        curServer = enumerator.Current.Key;

                        openedWindow.titleContent.text = enumerator.Current.Key;
                        
                        DirectoryInfo dir = new DirectoryInfo(@"CDN\");
                        if (dir.Exists)
                        {
                            window = Window.cdnDirectory;
                            MoveDirectory(new DirectoryInfo(@"CDN\"));
                        }
                        else
                        {
                            Debug.Log("경로가 비어있습니다. 탐색할 폴더를 생성해주세요.");
                        }
                    }
                }

                if(GUILayout.Button("폴더 일괄 생성", new GUIStyle(GUI.skin.button){normal = new GUIStyleState() {textColor = Color.yellow}}))
                {
                    CreateDirectories();
                }
            }
            //cdn 디렉토리 선택(실제로는 프로젝트의 CDN 폴더 내부 경로)
            else if (window == Window.cdnDirectory)
            {
                if (GUILayout.Button("... 참조 경로 리셋"))
                {
                    cdnServerMap.Clear();
                    paths.Clear();
                
                    window = Window.serverSelect;
                    ServerLoad();
                }
                
                if(GUILayout.Button("... 서버 재선택"))
                {
                    paths.Clear();
                    window = Window.serverSelect;
                }
                
                //디렉토리 상위 폴더가 프로젝트가 아닌 경우 상위 폴더로 이동할 수 있습니다.
                if (curDir.Parent.Name.Contains("lgpkv-") == false)
                {
                    if (GUILayout.Button("... 상위 폴더"))
                    {
                        paths.Remove(paths.Last());
                        MoveDirectory(curDir.Parent);
                    }
                }
                
                GUILayout.Label(GetCurrentServerPath(1));

                //현재 디렉토리의 하위 디렉토리가 있는 경우 버튼 클릭 시 디렉토리로 이동합니다.
                if (curChildDirs.Length > 0)
                {
                    scroll = GUILayout.BeginScrollView(scroll);
                    foreach (var dir in curChildDirs)
                    {
                        if (GUILayout.Button(dir.Name))
                        {
                            paths.Add($@"\{dir.Name}");
                            MoveDirectory(dir);
                        }
                    }
                    GUILayout.EndScrollView();
                }

                //현재 디렉토리의 리소스 정보를 출력하는 창을 엽니다.
                if (GUILayout.Button("현재 경로 리소스 확인", new GUIStyle(GUI.skin.button){normal = new GUIStyleState() {textColor = Color.yellow}}))
                {
                    CDNViewer window = GetWindow<CDNViewer>();
                    window.InitCDNWindow(curDir);
                }
            }
        }

        //서버에 접속해서 CDN 서버 리스트를 받아옵니다.
        private void ServerLoad()
        {
            string url = "https://lgpkv-launcher.lt.treenod.com/";

            WebClient webClient = new WebClient();

            string html = webClient.DownloadString(url);
            var datas = JsonConvert.DeserializeObject<List<UIPopupServerSelect.ServerUrl>>(html);
            
            if (datas != null && datas.Count > 0)
            {
                List<UIPopupServerSelect.ServerUrl> serverUrlList = datas.OrderBy(x => x.idx).ToList();
                        
                foreach (var data in serverUrlList)
                {
                    if (!string.IsNullOrEmpty(data.cdnUrl))
                    {
                        cdnServerMap.Add(data.desc, data.cdnUrl);
                    }
                }
            }
        }

        private void CreateDirectories()
        {
            string[] dirs = new[]
            {
                @"Animal\", @"AssetBundles\", @"Banner\", @"CachedResource\", @"CachedScript\", @"Costume\", @"DecoCollection\", @"DecoInfo\", @"Effect\", @"GimmickTutorial\", @"GuideEvent\"
                , @"HashGen\", @"IconCapsule\", @"IconEvent\", @"IconHousing\", @"IconLandImage\", @"IconLoginAD\", @"IconMaterial\", @"IconMission\", @"IconPremiumPass\", @"IconQuest\"
                , @"IconSetHousing\", @"IconShopItem\", @"Invite\", @"movie\", @"Notice\", @"Pokoyura\", @"Shop\", @"Sound\", @"stage\", @"Temp\"
            };
            Directory.CreateDirectory(@"CDN\");

            foreach (var value in dirs)
            {
                DirectoryInfo info = new DirectoryInfo(@"CDN\" + value);
                if (info.Exists == false)
                {
                    Directory.CreateDirectory(@"CDN\" + value);
                }
            }
        }

        //현재 프로젝트 하위 CDN 폴더의 내부 경로 내에서 이동합니다.(서버 디렉토리가 아님)
        private void MoveDirectory(DirectoryInfo directoryInfo)
        {
            curDir       = directoryInfo;
            curChildDirs = curDir.GetDirectories();
        }

        //현재 선택된 CDN 서버 + 디렉토리 경로를 반환합니다. 0일 경우 url 반환, 1일 경우 desc 경로 반환
        public string GetCurrentServerPath(int type = 0)
        {
            string path = "";
            if (type == 0)
            {
                path = cdnServerMap[curServer];
            }
            else
            {
                path = curServer + @"\CDN";
            }
            foreach (var root in paths)
            {
                path += @$"{root}\";
            }
            return path;
        }

    }

    //CDN 리소스 뷰어 창입니다.
    public class CDNViewer : EditorWindow
    {
        private List<string>  textureNames = new List<string>();
        private List<Texture> textures     = new List<Texture>();
        private List<string>  othersName   = new List<string>();
        GUIStyle              style        = new GUIStyle();
        GUIStyle              style2       = new GUIStyle();
        
        Vector2 scroll = Vector2.zero;
        Vector2 scroll2 = Vector2.zero;

        private string searchString = "";
        
        //서버의 선택한 경로에 포함된 리소스들을 출력합니다.
        private void OnGUI()
        {
            GUILayout.Label(CDNFileChecker.openedWindow.GetCurrentServerPath(1));
            
            //파일명 검색 
            GUILayout.BeginHorizontal();
            GUILayout.Button("검  색", style2);
            searchString = GUILayout.TextField(searchString);
            GUILayout.EndHorizontal();
            
            //png 파일
            NGUIEditorTools.DrawHeader("png files", true);
            {
                if (textures.Count > 0)
                {
                    scroll = GUILayout.BeginScrollView(scroll);
                    GUILayout.BeginHorizontal();

                    float totalWidth  = 0;
                    float windowWidth = 1000;

                    for(int i = 0; i < textures.Count; i ++)
                    {
                        //이번 줄의 이미지들 너비의 합이 1000을 넘을 경우 다음 줄로 이동
                        if (totalWidth > windowWidth)
                        {
                            totalWidth = 0;
                            GUILayout.EndHorizontal();
                            GUILayout.BeginHorizontal();
                        }

                        //찾은 텍스처, 이름 출력
                        GUILayout.BeginVertical();

                        totalWidth += textures[i].width;
                        GUILayout.Box(textures[i]);
                        
                        if (string.IsNullOrEmpty(searchString) == false && textureNames[i].Contains(searchString))
                        {
                            GUILayout.Label(textureNames[i], style);
                        }
                        else
                        {
                            GUILayout.Label(textureNames[i]);
                        }
                        
                        GUILayout.EndVertical();
                    }

                    GUILayout.EndHorizontal();
                    GUILayout.EndScrollView();
                    
                }
            }
            //png 외 파일
            NGUIEditorTools.DrawHeader("other files", true);
            {
                GUILayout.BeginVertical(new GUIStyle(){fixedHeight = 300});
                scroll2 = GUILayout.BeginScrollView(scroll2);
                GUILayout.BeginHorizontal();

                int fileCount = 0;
                for(int i = 0; i < othersName.Count; i ++)
                {
                    if (fileCount > 4)
                    {
                        fileCount = 0;
                        GUILayout.EndHorizontal();
                        GUILayout.BeginHorizontal();
                    }
                    fileCount++;
                    
                    if (string.IsNullOrEmpty(searchString) == false && othersName[i].Contains(searchString))
                    {
                        GUILayout.Label(othersName[i], style);
                    }
                    else
                    {
                        GUILayout.Label(othersName[i]);
                    }
                }
                
                GUILayout.EndHorizontal();
                GUILayout.EndScrollView();
                GUILayout.EndVertical();
            }
        }

        public void InitCDNWindow(DirectoryInfo directory)
        {
            style.normal.textColor  = Color.cyan;
            style.fontSize          = 16;
            style2.normal.textColor = Color.white;
            style2.fontSize         = 15;
            style2.fixedWidth       = 100;
            style2.alignment        = TextAnchor.MiddleCenter;
            
            string       nullNames  = "CDN 경로에 없는 리소스가 있습니다. [n]개 / 찾은 파일 [m]개 :\n ↓↓↓↓↓↓↓↓↓ \n";
            var          files      = directory.GetFiles();
            int          nullCount  = 0;
            
            textureNames.Clear();
            textures.Clear();
            othersName.Clear();
            
            foreach (var file in files)
            {
                //파일이 있다면 OnGUI에 출력하는 리스트에 추가합니다.
                try
                {
                    using (WebClient webClient = new WebClient())
                    {
                        byte[] bytes = webClient.DownloadData(CDNFileChecker.openedWindow.GetCurrentServerPath() + file.Name);
                        
                        if (bytes.Length > 0)
                        {
                            if (file.Name.Contains(".png"))
                            {
                                Texture2D texture = new Texture2D(0, 0);
                                texture.LoadImage(bytes);
                                if (texture == null)
                                    continue;
                                textureNames.Add(file.Name);
                                textures.Add(texture);
                            }
                            else
                            {
                                othersName.Add(file.Name);
                            }
                        }
                    }
                }
                //에러가 발생한 리소스는 없는 리소스에 추가합니다.
                catch(Exception e)
                {
                    nullCount++;
                    nullNames += $"{file.Name} \n - error {e.Message}\n";
                }
            }

            //결과를 출력합니다.
            if (nullCount <= 0)
            {
                Debug.Log($"CDN 경로에 모든 리소스가 포함되어 있습니다. {files.Length}개");
            }
            else
            {
                nullNames = nullNames.Replace("[n]", nullCount.ToString());
                nullNames = nullNames.Replace("[m]", (files.Length - nullCount).ToString());
                Debug.Log(nullNames);
            }
        }
    }
}