#if UNITY_EDITOR
using UnityEditor;
#endif
using UnityEngine;
using System.IO;
using System.Collections;
using System.Collections.Generic;

public class DefaultTextureImporter : AssetPostprocessor
{
    static readonly int DEFAULT_TEXTURE_MAX_SIZE = 512;

    enum TextureType
    {
        Unknown,
        Character,
        Block,
        Board,
        UI_AtlasElement,
        UI_IconTrueColor,
        UI_IconCompressed,
    }

    void OnPreprocessTexture()
    {
        TextureImporter importer = assetImporter as TextureImporter;
        if (null == importer) return;

    /*    var textureType = GetTextureType(importer.assetPath);
        if (TextureType.Unknown == textureType)
            return;*/
        importer.mipmapEnabled = false;
        // Already imported.
      /*  if (importer.textureType == TextureImporterType.Advanced)
            return;



        {
        //    importer.ClearPlatformTextureSettings("Android");
        //    importer.ClearPlatformTextureSettings("iPhone");
            importer.textureType = TextureImporterType.Advanced;
            //importer.anisoLevel = 3;
            //importer.compressionQuality = 100;
            importer.filterMode = FilterMode.Bilinear;
            importer.textureFormat = TextureImporterFormat.AutomaticCompressed;
            importer.generateCubemap = TextureImporterGenerateCubemap.None;
            importer.generateMipsInLinearSpace = false;
            importer.grayscaleToAlpha = false;
            importer.linearTexture = false;
            importer.isReadable = false;
            importer.lightmap = false;
            importer.mipmapEnabled = false;
            importer.normalmap = false;
            importer.spriteImportMode = SpriteImportMode.None;
            importer.npotScale = TextureImporterNPOTScale.ToNearest;
        }*/

  /*      switch (textureType)
        {
            case TextureType.Character:
            case TextureType.Block:
            case TextureType.Board:
                ImportTexture(importer, DEFAULT_TEXTURE_MAX_SIZE);
                break;

            case TextureType.UI_AtlasElement:
                ImportUIAtlasElementTexture(importer, 2048);
                break;

            case TextureType.UI_IconTrueColor:
                ImportUITrueColorTexture(importer, 2048);	// 2048 size used for the title background textures.
                break;

            case TextureType.UI_IconCompressed:
                ImportUICompressedTexture(importer, 2048);
                break;
        }*/
    }

  /*  TextureType GetTextureType(string assetPath)
    {
        if (assetPath.Contains("Assets/Character/"))
            return TextureType.Character;
        else if (assetPath.Contains("Assets/Block/"))
            return TextureType.Block;
        else if (assetPath.Contains("Assets/Board"))
            return TextureType.Board;
        else if (assetPath.Contains("Assets/Resources/Atlas"))
            return TextureType.UI_AtlasElement;
        else if (assetPath.Contains("Assets/Resources/CharacterImage"))
            return TextureType.UI_IconTrueColor;
        
        else if(assetPath.Contains("Assets/Resources/UI/Image/"))
        {
            if(assetPath.Contains("Assets/Resources/UI/Image/CharacterIcon/") 
               || assetPath.Contains("Assets/Resources/UI/Image/SkillIcon/")
               || assetPath.Contains("Assets/Resources/UI/Image/BuffIcon")
               || assetPath.Contains("Assets/Resources/UI/Image/JobIcon"))
            {
                return TextureType.UI_IconTrueColor;
            }
            else if(assetPath.Contains("Assets/Resources/UI/Image/TalkBox"))
            {
                var fileName = System.IO.Path.GetFileNameWithoutExtension(assetPath);
                int characterID = 0;

                // Character portriat illusts.
                if(int.TryParse(fileName, out characterID))
                    return TextureType.UI_IconTrueColor;
                else
                    return TextureType.UI_IconCompressed;
            }
            else
            {
                return TextureType.UI_IconCompressed;
            }
        }
        
        return TextureType.Unknown;
    }*/
  /*  public static void ImportTexture(TextureImporter importer, int size)
    {
        importer.ClearPlatformTextureSettings("Android");
        importer.ClearPlatformTextureSettings("iPhone");
        importer.textureType = TextureImporterType.Advanced;
        importer.anisoLevel = 3;
        importer.compressionQuality = 100;
        importer.filterMode = FilterMode.Bilinear;
        importer.textureFormat = TextureImporterFormat.AutomaticCompressed;
        importer.generateCubemap = TextureImporterGenerateCubemap.None;
        importer.generateMipsInLinearSpace = false;
        importer.grayscaleToAlpha = false;
        importer.linearTexture = false;
        importer.isReadable = false;
        importer.lightmap = false;
        importer.mipmapEnabled = false;
        importer.normalmap = false;
        importer.spriteImportMode = SpriteImportMode.None;
        importer.npotScale = TextureImporterNPOTScale.ToNearest;
        importer.maxTextureSize = size;
    }

    public static void ImportUIAtlasElementTexture(TextureImporter importer, int size)
    {
        importer.textureType = TextureImporterType.GUI;
        importer.alphaIsTransparency = true;
        importer.filterMode = FilterMode.Bilinear;
        importer.maxTextureSize = size;
        importer.textureFormat = TextureImporterFormat.AutomaticTruecolor;
    }

    public static void ImportUITrueColorTexture(TextureImporter importer, int size)
    {
        importer.ClearPlatformTextureSettings("Android");
        importer.ClearPlatformTextureSettings("iPhone");
        importer.textureType = TextureImporterType.Advanced;
        importer.alphaIsTransparency = true;
        importer.linearTexture = true;
        importer.anisoLevel = 0;
        importer.compressionQuality = 100;
        importer.filterMode = FilterMode.Bilinear;
        importer.textureFormat = TextureImporterFormat.AutomaticTruecolor;
        importer.generateCubemap = TextureImporterGenerateCubemap.None;
        importer.generateMipsInLinearSpace = false;
        importer.grayscaleToAlpha = false;
        importer.isReadable = false;
        importer.lightmap = false;
        importer.maxTextureSize = size;
        importer.mipmapEnabled = false;
        importer.normalmap = false;
        importer.spriteImportMode = SpriteImportMode.None;
        importer.wrapMode = TextureWrapMode.Clamp;
        importer.npotScale = TextureImporterNPOTScale.ToNearest;
        importer.SetPlatformTextureSettings("Android", size, TextureImporterFormat.RGBA32, 100, false);
    }

    public static void ImportUICompressedTexture(TextureImporter importer, int size)
    {
        importer.ClearPlatformTextureSettings("Android");
        importer.ClearPlatformTextureSettings("iPhone");
        importer.textureType = TextureImporterType.Advanced;
        importer.alphaIsTransparency = true;
        importer.linearTexture = true;
        importer.anisoLevel = 0;
        importer.compressionQuality = 100;
        importer.filterMode = FilterMode.Bilinear;
        importer.textureFormat = TextureImporterFormat.AutomaticCompressed;
        importer.generateCubemap = TextureImporterGenerateCubemap.None;
        importer.generateMipsInLinearSpace = false;
        importer.grayscaleToAlpha = false;
        importer.isReadable = false;
        importer.lightmap = false;
        importer.maxTextureSize = size;
        importer.mipmapEnabled = false;
        importer.normalmap = false;
        importer.spriteImportMode = SpriteImportMode.None;
        importer.wrapMode = TextureWrapMode.Clamp;
        importer.npotScale = TextureImporterNPOTScale.ToNearest;
        importer.SetPlatformTextureSettings("Android", size, TextureImporterFormat.ETC2_RGBA8, 100, false);
    }*/
}
