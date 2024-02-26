using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UIMaterialTexture : MonoBehaviour
{
    public UIUrlTexture matTexture;
    private int materialIndex = 0;

    private const int texW = 128;
    private const int texH = 128;
    private const int atW = 512;
    private const int atH = 512;

    public void InitMaterialTexture(int matIdx, int sizeX, int sizeY)
    {
        materialIndex = matIdx;
        string fileName = "mt_" + materialIndex;

        long expireTs = 0;
        if (ManagerData._instance._materialSpawnProgress.ContainsKey(materialIndex))
            expireTs = ManagerData._instance._materialSpawnProgress[materialIndex];

        //현재 검사하는 재료 텍스쳐 정보가 있는지 검사.
        if (ManagerCdnTextureAtlas._instance != null && expireTs == 0)
        {
            bool bLocalLoadTexture = false;
            bLocalLoadTexture = ManagerCdnTextureAtlas._instance.CheckTextureData((int)TextureCategory_T.Material, materialIndex);

            if (bLocalLoadTexture == true)
            {
                string hashFileName = Global.GetHashfromText("IconMaterial/" + fileName) + ".png";
                string filePath = Global.gameImageDirectory + hashFileName;

                //로컬에 해당 재료 이미지 저장되어있는지 검사.
                if (System.IO.File.Exists(filePath))
                {
                    //해시값이 다른지 검사.
                    System.IO.FileInfo fileInfo = new System.IO.FileInfo(filePath);
                    if (HashChecker.IsHashChanged("IconMaterial", fileName + ".png", fileInfo))
                    {
                        //Debug.Log("1. ChangeHash : " + fileName);
                        bLocalLoadTexture = false;
                    }
                    else
                        bLocalLoadTexture = true;
                }
                else
                {
                    //Debug.Log("2. File Not Exists : " + fileName);
                    bLocalLoadTexture = false;
                }
            }
            //else
            //    Debug.Log("3. Matarial Data Exists : " + fileName);

            if (bLocalLoadTexture == false)
            {
                matTexture.SettingTextureScale(sizeX, sizeY);
                matTexture.SuccessEvent += ImageSaveCallBack;
                matTexture.LoadCDN(Global.gameImageDirectory, "IconMaterial/", fileName);
                matTexture.uvRect = new Rect(0.0f, 0.0f, 1.0f, 1.0f);
                ManagerCdnTextureAtlas._instance.InitTextureDataLoadState((int)TextureCategory_T.Material, materialIndex);
            }
            else
            {
                TextureKeyAndData textureData = new TextureKeyAndData();
                textureData = ManagerCdnTextureAtlas._instance.GetTextureKeyAndData((int)TextureCategory_T.Material, materialIndex);
                matTexture.mainTexture
                    = ManagerCdnTextureAtlas._instance.GetTexture((int)TextureCategory_T.Material, textureData, texW, texH, atW, atH);
                matTexture.uvRect = ManagerCdnTextureAtlas._instance.GetUVRect(textureData, texW, texH, atW, atH);

                matTexture.gameObject.SetActive(true);
            }
        }
        else
        {
            matTexture.SettingTextureScale(sizeX, sizeY);
            matTexture.LoadCDN(Global.gameImageDirectory, "IconMaterial/", fileName);
            matTexture.uvRect = new Rect(0.0f, 0.0f, 1.0f, 1.0f);
        }
    }

    private void ImageSaveCallBack()
    {
        long expireTs = 0;
        if (ManagerData._instance._materialSpawnProgress.ContainsKey(materialIndex))
            expireTs = ManagerData._instance._materialSpawnProgress[materialIndex];

        if (ManagerCdnTextureAtlas._instance != null && expireTs == 0)
        {
            if (ManagerCdnTextureAtlas._instance.CheckTextureData((int)TextureCategory_T.Material, materialIndex) == false)
            {  
                ManagerCdnTextureAtlas._instance.MakeAtlasAndTextureData(TextureCategory_T.Material, materialIndex, (Texture2D)matTexture.mainTexture, texW, texH, atW, atH);
            }
        }
    }
}
