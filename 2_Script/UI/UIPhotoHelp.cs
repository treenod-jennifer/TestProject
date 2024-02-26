using UnityEngine;
using System.Collections;
using System.Reflection;
using System.IO;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Runtime.Serialization.Formatters.Binary;
using System.Runtime.Serialization;
using System.Security.Cryptography;
using System.Text;

public class PhotoHelp : MonoBehaviour
{

    public static void SaveData(byte[] byteData, string fileName)
    {

        //    Debug.Log("SaveData " + Application.persistentDataPath + "/" + fileName);

        FileStream fs = new FileStream(Application.persistentDataPath + "/" + fileName, FileMode.Create);
        fs.Seek(0, SeekOrigin.Begin);
        fs.Write(byteData, 0, byteData.Length);
        fs.Close();

        //    Debug.Log("SaveData end ");

    }

    #region 원래 글로벌에서 해주던 함수
    static string ByteArrayToString(byte[] arrInput)
    {
        int i;
        StringBuilder sOutput = new StringBuilder(arrInput.Length);
        for (i = 0; i < arrInput.Length - 1; i++)
        {
            sOutput.Append(arrInput[i].ToString("X2"));
        }
        return sOutput.ToString();
    }

    static MD5CryptoServiceProvider _hash = new MD5CryptoServiceProvider();
    static public string GetHashfromText(string in_text)
    {
        byte[] tmpSource = ASCIIEncoding.ASCII.GetBytes(in_text);
        byte[] tmpHash = _hash.ComputeHash(tmpSource);

        return ByteArrayToString(tmpHash);
    }
    #endregion

    public static IEnumerator WWW_LoadImage(string in_url, bool in_photo = true)
    {
        Texture2D image = null;
        if (in_url != null)
        {
            if (in_url.Length > 0)
            {
                // string[] fileName_ = in_url.Split(new char[] { '/' });
                string fileName = GetHashfromText(in_url) + ".png";//fileName_[fileName_.Length - 2] + fileName_[fileName_.Length - 1] + ".png";

                //string localPath = Global.localPath + fileName;
                string localPath = Global.FileUri + Application.persistentDataPath + "/";

                WWW www;
                www = new WWW(localPath);
                yield return www;
                bool loadwww = true;
                if (www.error == null && www.texture != null)
                {
                    if (www.text.Length > 0)
                    {

                        //Debug.Log("__ LoadImage local  " + in_url);
                        image = www.texture;
                        loadwww = false;

                        if (image.width == 8 && image.height == 8)
                        {
                            loadwww = true;
                            File.Delete(Application.persistentDataPath + fileName);
                        }
                    }
                }

                if (loadwww)
                {
                    //www = new WWW(in_url);
                    if (in_photo)
                        www = new WWW(in_url + "/small"); // 작은사진읽기
                    else
                    {
                        www = new WWW(in_url + ".png");
                    }

                    //Debug.Log(in_url + ".png");

                    yield return www;
                    if (www.error == null && www.texture != null)
                    {
                        if (www.text.Length > 0)
                        {
                            image = www.texture;
                            SaveData(image.EncodeToPNG(), fileName);
                        }
                    }
                }
                www.Dispose();
            }
        }

        yield return image;
    }
}
