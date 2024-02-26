using System.IO;
using UnityEngine;
using System.Security.Cryptography;
using System.Text;
using System.Xml;
using System.Xml.Serialization;

public class StageHelper : MonoBehaviour
{
    public static string filekey = "myHa766b14dLs3w6UZvaZHTG4VAn115G";
    public static string Decrypt256(string Input, string key)
    {
        RijndaelManaged aes = new RijndaelManaged();
        aes.KeySize = 256;
        aes.BlockSize = 128;
        aes.Mode = CipherMode.CBC;
        aes.Padding = PaddingMode.PKCS7;
        aes.Key = Encoding.UTF8.GetBytes(key);
        aes.IV = new byte[] { 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 };

        var decrypt = aes.CreateDecryptor();
        byte[] xBuff = null;

        using (var ms = new MemoryStream())
        {
            using (var cs = new CryptoStream(ms, decrypt, CryptoStreamMode.Write))
            {
                byte[] xXml = System.Convert.FromBase64String(Input);
                cs.Write(xXml, 0, xXml.Length);
            }

            xBuff = ms.ToArray();
        }

        string Output = Encoding.UTF8.GetString(xBuff);
        return Output;
    }
    public static string AESEncrypt256(string Input, string key)
    {
        RijndaelManaged aes = new RijndaelManaged();
        aes.KeySize = 256;
        aes.BlockSize = 128;
        aes.Mode = CipherMode.CBC;
        aes.Padding = PaddingMode.PKCS7;
        aes.Key = Encoding.UTF8.GetBytes(key);
        aes.IV = new byte[] { 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 };

        var encrypt = aes.CreateEncryptor(aes.Key, aes.IV);
        byte[] xBuff = null;
        using (var ms = new MemoryStream())
        {
            using (var cs = new CryptoStream(ms, encrypt, CryptoStreamMode.Write))
            {
                byte[] xXml = Encoding.UTF8.GetBytes(Input);
                cs.Write(xXml, 0, xXml.Length);
            }

            xBuff = ms.ToArray();
        }

        string Output = System.Convert.ToBase64String(xBuff);
        return Output;
    }


    public static void StageEncrypt(string in_path)
    {
        string path = in_path;
        DirectoryInfo directoryInfo = new DirectoryInfo(path);
        FileInfo[] fileInfo = directoryInfo.GetFiles("*.xml");        

        foreach (FileInfo file in fileInfo)
        {
            bool eventStage = false;
            bool AdventureStage = false;
            FileInfo t = new FileInfo(path + file.Name);

            string fileName = "";
            string version = "0";
            string[] x = file.Name.Split('_');

            if (x.Length == 2)
            {
                fileName = x[0] + ".xml";
                version = x[1].Split('.')[0];    //버전(키에는 버전정보를 뺀 스테이지 이름만 사용)
            }
            else if (x.Length == 3)
            {          
                fileName = x[0]+ "_"+x[1] + ".xml";
                eventStage = true;
                version = x[2].Split('.')[0];
            }
            else 
            {
                AdventureStage = true;
                fileName = x[0]+ "_"+x[1] + "_"+x[2] + ".xml";
                version = x[3].Split('.')[0];
            }

            Debug.Log("파일이름=" + file.Name +", 저장이름=" + fileName + ", 버전=" + version + "이벤트=" + x.Length);

            FileStream stream = t.Open(FileMode.Open);

            var serializer = new XmlSerializer(typeof(StageInfo));
            StageInfo openStage = (StageInfo)serializer.Deserialize(stream);

            MemoryStream ms = new MemoryStream();
            XmlTextWriter xmlTextWriter = new XmlTextWriter(ms, Encoding.UTF8);
            serializer.Serialize(ms, openStage);
            string _in = Encoding.UTF8.GetString(ms.ToArray());

            string key = filekey;
            key = key.Insert(4, fileName);
            key = key.Substring(0, filekey.Length);
            _in = AESEncrypt256(_in, key);

            //구버전 목표 데이터를 사용하고 있는 맵의 경우, 암호화 할 때 석판 수를 맵 데이터에 넣어줌.
            if (openStage.collectCount.Length > 1)
            {
                int crackCount = 0;
                for (int i = 0; i < openStage.ListBlock.Count; i++)
                {
                    if (openStage.ListBlock[i].ListDeco.Count == 0)
                        continue;

                    for (int j = 0; j < openStage.ListBlock[i].ListDeco.Count; j++)
                    {
                        if (openStage.ListBlock[i].ListDeco[j].BoardType == (int)BoardDecoType.CARCK1)
                        {
                            crackCount++;
                        }
                    }
                }
                openStage.collectCount[0] += crackCount;
            }

            StageMapData stageData = new StageMapData();
            stageData.version = int.Parse(version);
            stageData.moveCount = openStage.turnCount;

            #region 구버전 목표 데이터
            stageData.collectCount = openStage.collectCount;
            stageData.collectType = openStage.collectType;
            stageData.collectColorCount = openStage.collectColorCount;
            #endregion

            stageData.gameMode = openStage.gameMode;
            stageData.isHardStage = openStage.isHardStage;
            stageData.listTargetInfo = openStage.listTargetInfo;
            stageData.data = _in;

            if (openStage.gameMode == (int)GameMode.ADVENTURE)
            {
                stageData.bossIdx = openStage.bossInfo.idx;
                stageData.bossAttr = openStage.bossInfo.attribute;
            }

            var serializerData = new XmlSerializer(typeof(StageMapData));
            MemoryStream msData = new MemoryStream();
            XmlTextWriter xmlTextWriterData = new XmlTextWriter(msData, Encoding.UTF8);
            serializerData.Serialize(msData, stageData);
            
            string fdata = Encoding.UTF8.GetString(msData.ToArray());    
            FileInfo fWrite = null;
            if (eventStage || AdventureStage)       
            {
                if (Application.platform == RuntimePlatform.OSXEditor)
                    fWrite = new FileInfo(path + "Result//" + fileName);
                else
                    fWrite = new FileInfo(path + "Result\\" + fileName);
            }
            else
            {
                if (Application.platform == RuntimePlatform.OSXEditor)
                {
                    fWrite = new FileInfo(path + "Result//" + Global.GetHashfromText(fileName) + ".xml");
                }
                else
                    fWrite = new FileInfo(path + "Result\\" + Global.GetHashfromText(fileName) + ".xml");
            }

            FileStream wStream = fWrite.OpenWrite();
            byte[] arrInput = Encoding.UTF8.GetBytes(fdata);

            wStream.Write(arrInput, 0, fdata.Length);
            wStream.Close();
        }
        Debug.Log(fileInfo.Length + "개완료  버전");
    }
}
