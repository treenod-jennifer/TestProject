using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System.IO;

public class Edit_SelectPack_Maker
{
    [MenuItem("Poko/MakeBundle/SelectPack_ver1")]

    private static void Version1() => MakeBundle(1);

    [MenuItem("Poko/MakeBundle/SelectPack_ver2")]

    private static void Version2() => MakeBundle(2);
    
    private static void MakeBundle(int version)
    { //버전에 따라 파일명 변경
    
        string            path    = "Assets/5_OutResource/PackageSelect/";
        DirectoryInfo     fileDir = new DirectoryInfo(path);
        DirectoryInfo[] dirInfos = new DirectoryInfo[2]; 
        dirInfos[0] = fileDir.GetDirectories("*_1_jp")[0];
        dirInfos[1] = fileDir.GetDirectories("*_1_tw")[0];
        
        bool exists = new DirectoryInfo(path+$"pack_select_{version}_jp").Exists;

        if (exists == false)
        {
            //폴더 복사
            foreach (var dirInfo in dirInfos)
            {
                string fromPath = path + dirInfo.Name;
                string toPath   = fromPath.Replace("1", version.ToString());
                
                new DirectoryInfo(toPath).Create();
                
                //파일 복사 
                FileInfo[] fromFileInfo = dirInfo.GetFiles("*");
                FileInfo[] toFileInfo = dirInfo.GetFiles("*");
            
                for(int i = 0; i < fromFileInfo.Length; i++)
                {
                    if (fromFileInfo[i].Name.Contains(".meta"))
                        continue;
                    string copyPath = toPath      + "/" + fromFileInfo[i].Name;
                    fromFileInfo[i].CopyTo(copyPath);
                    AssetDatabase.ImportAsset(copyPath, ImportAssetOptions.Default);
                }
            }
            
        }
        
        //아틀라스 변경
        {
            string[] country = { "jp", "tw" };
            string   result  = $"결과 ------------\n version : {version}";
            foreach (var code in country)
            {
                string        toPath     = $"Assets/5_OutResource/PackageSelect/images_{code}/";
                DirectoryInfo toFileDir  = new DirectoryInfo(toPath);
                FileInfo[]    toFileInfo = toFileDir.GetFiles("*.png");
                
                string atlasPath         = $"{path}pack_select_{version}_{code}/PackageSelectAtlas.asset";
                string atlasMeterialPath = $"{path}pack_select_{version}_{code}/PackageSelectAtlas.mat";
                string atlasPngPath       = $"{path}pack_select_{version}_{code}/PackageSelectAtlas.png";

                NGUISettings.atlas                = AssetDatabase.LoadAssetAtPath<NGUIAtlas>(atlasPath);
                Material mat = AssetDatabase.LoadAssetAtPath<Material>(atlasMeterialPath);
                NGUISettings.atlas.spriteMaterial             = mat;
                Texture2D tex = AssetDatabase.LoadAssetAtPath<Texture2D>(atlasPngPath);
                NGUISettings.atlas.spriteMaterial.mainTexture = tex;

                List<Texture> textures = new List<Texture>();
                foreach (var info in toFileInfo)
                {
                    string pngPath = $"{toPath}{info.Name}";
                    textures.Add(AssetDatabase.LoadAssetAtPath<Texture2D>(pngPath));
                }
                List<UIAtlasMaker.SpriteEntry> sprites = UIAtlasMaker.CreateSprites(textures);
                UIAtlasMaker.ExtractSprites(NGUISettings.atlas, sprites);
                UIAtlasMaker.UpdateAtlas(NGUISettings.atlas, sprites);
                
                result += $"\n country : {code} / atlas size : {NGUISettings.atlas.texture.width}x{NGUISettings.atlas.texture.height}";
            }

            result += "\n성공------------";
            Debug.Log(result);
        }
    }
}//PackImage
