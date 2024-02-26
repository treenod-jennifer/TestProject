using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Xml;
using UnityEngine;

public static class NGUIExtension {
 
    static public NGUIAtlas CompositeAtlas(List<INGUIAtlas> atlasShards, string atlasName)
    {
        var startTick = System.Environment.TickCount;
        List<Texture2D> textures = new List<Texture2D>();
        List<UISpriteData> spriteInfos = new List<UISpriteData>();
        List<bool> expandingFlags = new List<bool>();

        Dictionary<string, Texture2D> addAtlasSprite = new Dictionary<string, Texture2D>();
        TextureFormat texFormat = TextureFormat.ARGB32;

        for (int i = 0; i < atlasShards.Count; ++i)
        {
            var atlasShard = atlasShards[i];

            Texture2D atlasTex = atlasShard.spriteMaterial.mainTexture as Texture2D;
            texFormat = atlasTex.format;
            var atlasPixels = atlasTex.GetPixels32();

            for (int j = 0; j < atlasShard.spriteList.Count; ++j)
            {
                if( addAtlasSprite.ContainsKey(atlasShard.spriteList[j].name) )
                    continue;

                var sprData = atlasShard.spriteList[j];
                var sprTex = new Texture2D(sprData.width, sprData.height, atlasTex.format, false);
                sprTex.wrapMode = TextureWrapMode.Clamp;
                sprTex.filterMode = FilterMode.Trilinear;

                int wrapFlag = sprTex.CopyFromTexture(atlasPixels, atlasTex.width, sprData.x, sprData.x + sprData.width, atlasTex.height - (sprData.y + sprData.height), atlasTex.height - sprData.y);
                
                expandingFlags.Add(wrapFlag != 0);
                var sprDataBackup = new UISpriteData();
                sprDataBackup.CopyFrom(sprData);

                if (wrapFlag != 0)
                {
                    sprTex = sprTex.WrapTexture(2);
                }

                textures.Add(sprTex);
                spriteInfos.Add(sprDataBackup);
                addAtlasSprite.Add(atlasShard.spriteList[j].name, sprTex );
            }
        }
        // 아틀라스 패킹하고 저장
        var partialTextures = textures.ToArray();

        //var newAtlas = GameObject.Instantiate<NGUIAtlas>(atlas);
        var newAtlas = ScriptableObject.CreateInstance<NGUIAtlas>();
        newAtlas.name = $"{atlasName}(New)_Atlas";
        MonoBehaviour.DontDestroyOnLoad(newAtlas);

        newAtlas.spriteMaterial = new Material(Shader.Find("Unlit/Transparent Colored"));
        newAtlas.spriteMaterial.name = $"{atlasName}(New)_Mat";
        Texture2D newAtlasTex = new Texture2D(1, 1, texFormat, false);
        newAtlasTex.wrapMode = TextureWrapMode.Clamp;
        newAtlasTex.filterMode = FilterMode.Trilinear;
        newAtlasTex.name = $"{atlasName}(New)_Tex";

        var rects = newAtlasTex.PackTextures(partialTextures, 1);
        newAtlas.spriteMaterial.mainTexture = newAtlasTex;

        newAtlas.spriteList = spriteInfos;

        for (int i = 0; i < rects.Length; ++i)
        {
            newAtlas.spriteList[i].x = (int)(rects[i].x * newAtlasTex.width);
            newAtlas.spriteList[i].y = (int)((1.0f - rects[i].y - rects[i].height) * newAtlasTex.height);

            newAtlas.spriteList[i].width = (int)(rects[i].width * newAtlasTex.width);
            newAtlas.spriteList[i].height = (int)(rects[i].height * newAtlasTex.height);

            if (expandingFlags[i])
            {
                newAtlas.spriteList[i].x += 2;
                newAtlas.spriteList[i].width -= 4;

                newAtlas.spriteList[i].y += 2;
                newAtlas.spriteList[i].height -= 4;
            }
        }

        var elapsedTick = System.Environment.TickCount - startTick;
        //Debug.Log("MergeAtlas (" + atlas.name + "): " + elapsedTick.ToString());

        foreach (var texture in textures)
        {
            Object.DestroyImmediate(texture, true);
        }
        

        return newAtlas;
    }

    static public string Serialize(this UIAtlas atlas, int ver)
    {
        XmlDocument doc = new XmlDocument();
        doc.AppendChild(doc.CreateXmlDeclaration("1.0", "utf-8", "yes"));
        var rootNode = doc.CreateElement("Root");
        rootNode.SetAttribute("ver", ver.ToString());
        doc.AppendChild(rootNode);

        var atlasElem = doc.CreateElement("Atlas");
        atlasElem.SetAttribute("name", atlas.name);
        atlasElem.SetAttribute("shader_name", atlas.spriteMaterial.shader.name);
        rootNode.AppendChild(atlasElem);

        var spriteRootNode = doc.CreateElement("Sprites");
        for(int i = 0; i < atlas.spriteList.Count; ++i)
        {
            var sprElem = doc.CreateElement("Sprite");

            sprElem.SetAttribute("name", atlas.spriteList[i].name.ToString());
            sprElem.SetAttribute("x", atlas.spriteList[i].x.ToString());
            sprElem.SetAttribute("y", atlas.spriteList[i].y.ToString());
            sprElem.SetAttribute("width", atlas.spriteList[i].width.ToString());
            sprElem.SetAttribute("height", atlas.spriteList[i].height.ToString());
            sprElem.SetAttribute("borderLeft", atlas.spriteList[i].borderLeft.ToString());
            sprElem.SetAttribute("borderRight", atlas.spriteList[i].borderRight.ToString());
            sprElem.SetAttribute("borderTop", atlas.spriteList[i].borderTop.ToString());
            sprElem.SetAttribute("borderBottom", atlas.spriteList[i].borderBottom.ToString());
            sprElem.SetAttribute("paddingLeft", atlas.spriteList[i].paddingLeft.ToString());
            sprElem.SetAttribute("paddingRight", atlas.spriteList[i].paddingRight.ToString());
            sprElem.SetAttribute("paddingTop", atlas.spriteList[i].paddingTop.ToString());
            sprElem.SetAttribute("paddingBottom", atlas.spriteList[i].paddingBottom.ToString());

            spriteRootNode.AppendChild(sprElem);
        }
        rootNode.AppendChild(spriteRootNode);

        using (var stringWriter = new StringWriter())
        using (var xmlTextWriter = XmlWriter.Create(stringWriter))
        {
            doc.WriteTo(xmlTextWriter);
            xmlTextWriter.Flush();
            return stringWriter.GetStringBuilder().ToString();
        }
    }

    static public UIAtlas LoadAtlas(string serializedScript, int version)
    {
        XmlDocument doc = new XmlDocument();
        doc.LoadXml(serializedScript);

        var rootNode = doc.SelectSingleNode("Root");
        var atlasNode = doc.SelectSingleNode("Root/Atlas");
        var sprNodeList = doc.SelectNodes("Root/Sprites/Sprite");

        List<UISpriteData> spriteInfos = new List<UISpriteData>();
        foreach (XmlNode sprNode in sprNodeList)
        {
            UISpriteData sprData = new UISpriteData();
            sprData.name = xmlhelper.GetString(sprNode, "name");
            sprData.x = xmlhelper.GetInt(sprNode, "x", 0);
            sprData.y = xmlhelper.GetInt(sprNode, "y", 0);
            sprData.width = xmlhelper.GetInt(sprNode, "width", 0);
            sprData.height = xmlhelper.GetInt(sprNode, "height", 0);
            sprData.borderLeft = xmlhelper.GetInt(sprNode, "borderLeft", 0);
            sprData.borderRight = xmlhelper.GetInt(sprNode, "borderRight", 0);
            sprData.borderTop = xmlhelper.GetInt(sprNode, "borderTop", 0);
            sprData.borderBottom = xmlhelper.GetInt(sprNode, "borderBottom", 0);
            sprData.paddingLeft = xmlhelper.GetInt(sprNode, "paddingLeft", 0);
            sprData.paddingRight = xmlhelper.GetInt(sprNode, "paddingRight", 0);
            sprData.paddingTop = xmlhelper.GetInt(sprNode, "paddingTop", 0);
            sprData.paddingBottom = xmlhelper.GetInt(sprNode, "paddingBottom", 0);

            spriteInfos.Add(sprData);
        }

        var obj = new GameObject();
        UIAtlas newAtlas = obj.AddComponent<UIAtlas>();
        newAtlas.name = xmlhelper.GetString(atlasNode, "name");
        string shaderName = xmlhelper.GetString(atlasNode, "shader_name");
        newAtlas.spriteList = spriteInfos;
        newAtlas.spriteMaterial = new Material(Shader.Find(shaderName));

        // 텍스쳐 로드하는 부분 작업필요

        return newAtlas;
    }
}
