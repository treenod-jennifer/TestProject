using UnityEngine;
using System.Collections;
using System.Runtime.Serialization.Formatters.Binary;
using System.IO;
using System.Collections;
using System.Runtime.Serialization;
using System.Security.Cryptography;
using System;
using System.Configuration;
using System.Text;
using System.Security.Cryptography;

public class FixSaveManager
{
    static bool init = false;
    static string sECRET_KEY = "";
    static byte[] sSkey = new byte[8];

    static bool needReLoad = true;


    static private string Decrypt(string Input)
    {
        RijndaelManaged aes = new RijndaelManaged();
        aes.KeySize = 256;
        aes.BlockSize = 128;
        aes.Mode = CipherMode.CBC;
        aes.Padding = PaddingMode.PKCS7;
        aes.Key = Encoding.UTF8.GetBytes(sECRET_KEY);
        aes.IV = new byte[] { 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 };


        var decrypt = aes.CreateDecryptor();
        byte[] xBuff = null;
        using (var ms = new MemoryStream())
        {
            using (var cs = new CryptoStream(ms, decrypt, CryptoStreamMode.Write))
            {
                byte[] xXml = Convert.FromBase64String(Input);
                cs.Write(xXml, 0, xXml.Length);
            }

            xBuff = ms.ToArray();
        }

        string Output = Encoding.UTF8.GetString(xBuff);



        //Debug.Log("Decrypt " + Output);
        return Output;
    }

    static private string Encrypt(string Input)
    {
        RijndaelManaged aes = new RijndaelManaged();
        aes.KeySize = 256;
        aes.BlockSize = 128;
        aes.Mode = CipherMode.CBC;
        aes.Padding = PaddingMode.PKCS7;
        aes.Key = Encoding.UTF8.GetBytes(sECRET_KEY);
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
        string Output = Convert.ToBase64String(xBuff);

        //Debug.Log("Encrypt " + Input);
        return Output;
    }


    public static void Save()
    {
        PlayerPrefs.Save();
    }
    static void Init()
    {
        if (init)
            return;

        init = true;
        sECRET_KEY = "1yHa76wd14dLs3w6UZvaddTG4VAn115G";
        sSkey = System.Text.ASCIIEncoding.ASCII.GetBytes(sECRET_KEY);
        sSkey[1] = 3;
        sSkey[4] = 9;
        sSkey[5] = 9;
    }
    public static void DeleteKey(string in_key)
    {
        if (in_key == null)
            return;

        if (in_key.Length <= 0)
            return;


        string name = Global.GetHashfromText(in_key);
        PlayerPrefs.DeleteKey(name);
    }
    static string getValue(string in_key)
    {
        Init();

        string name = Global.GetHashfromText(in_key);
        string value = PlayerPrefs.GetString(name, "no data");
        if (value != "no data")
        {
            value = Decrypt(value);
            return value;
        }
        return null;
    }
    static void setValue(string in_key, string in_value)
    {
        Init();
        string name = Global.GetHashfromText(in_key);
        string value = Encrypt(in_value);
        PlayerPrefs.SetString(name, value);
    }

    public static float GetFloat(string key)
    {
        string value = getValue(key);
        if (value != null)
            return float.Parse(value);
        return 0f;
    }
    public static float GetFloat(string key, float defaultValue)
    {
        string value = getValue(key);
        if (value != null)
            return float.Parse(value);
        return defaultValue;
    }
    public static int GetInt(string key)
    {
        string value = getValue(key);
        if (value != null)
            return int.Parse(value);
        return 0;
    }
    public static int GetInt(string key, int defaultValue)
    {
        string value = getValue(key);
        if (value != null)
            return int.Parse(value);
        return defaultValue;
    }
    public static long GetLong(string key, long defaultValue)
    {
        string value = getValue(key);
        if (value != null)
            return long.Parse(value);
        return defaultValue;
    }
    public static string GetString(string key)
    {
        string value = getValue(key);
        if (value != null)
            return value;
        return "";
    }
    public static string GetString(string key, string defaultValue)
    {
        string value = getValue(key);
        if (value != null)
            return value;
        return defaultValue;
    }
    public static bool HasKey(string key)
    {
        Init();
        string name = Global.GetHashfromText(key);
        return PlayerPrefs.HasKey(name);
    }

    //////////////////////////////////////////////////////////////////////////////

    public static void SetFloat(string key, float value)
    {
        setValue(key, value.ToString());
    }
    public static void SetInt(string key, int value)
    {
        setValue(key, value.ToString());
    }
    public static void SetString(string key, string value)
    {
        setValue(key, value);
    }
    public static void SetLong(string key, long value)
    {
        setValue(key, value.ToString());
    }
}

public class SaveManager
{
    static bool init = false;
    static string sECRET_KEY = "";
    static byte[] sSkey = new byte[8];

    static bool needReLoad = true;
    //  static Hashtable hash = new Hashtable();
    //  static MD5CryptoServiceProvider hashmd = new MD5CryptoServiceProvider();
    //static string dataPath = Application.persistentDataPath + "/DataFile.dat";
    //
    //  static byte[] keyArray;
    //  static DESCryptoServiceProvider mCryptoProvider = new System.Security.Cryptography.DESCryptoServiceProvider();
    //   static ICryptoTransform mEncryptorKey;
    //  static ICryptoTransform mDecryptorKey;
    static private string Decrypt(string Input)
    {
        RijndaelManaged aes = new RijndaelManaged();
        aes.KeySize = 256;
        aes.BlockSize = 128;
        aes.Mode = CipherMode.CBC;
        aes.Padding = PaddingMode.PKCS7;
        aes.Key = Encoding.UTF8.GetBytes(sECRET_KEY);
        aes.IV = new byte[] { 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 };


        var decrypt = aes.CreateDecryptor();
        byte[] xBuff = null;
        using (var ms = new MemoryStream())
        {
            using (var cs = new CryptoStream(ms, decrypt, CryptoStreamMode.Write))
            {
                byte[] xXml = Convert.FromBase64String(Input);
                cs.Write(xXml, 0, xXml.Length);
            }

            xBuff = ms.ToArray();
        }

        string Output = Encoding.UTF8.GetString(xBuff);



        //Debug.Log("Decrypt " + Output);
        return Output;
    }

    static private string Encrypt(string Input)
    {
        RijndaelManaged aes = new RijndaelManaged();
        aes.KeySize = 256;
        aes.BlockSize = 128;
        aes.Mode = CipherMode.CBC;
        aes.Padding = PaddingMode.PKCS7;
        aes.Key = Encoding.UTF8.GetBytes(sECRET_KEY);
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
        string Output = Convert.ToBase64String(xBuff);

        //Debug.Log("Encrypt " + Input);
        return Output;
    }


    public static void Save()
    {
        PlayerPrefs.Save();
    }
    /*public static string Encrypt(string ToEncrypt)
    {
        DESCryptoServiceProvider rc2 = new DESCryptoServiceProvider();

        rc2.Key = sSkey;
        rc2.IV = sSkey;

        MemoryStream ms = new MemoryStream();
        CryptoStream cryStream = new CryptoStream(ms, rc2.CreateEncryptor(), CryptoStreamMode.Write);

        byte[] data = Encoding.UTF8.GetBytes(ToEncrypt.ToCharArray());
        cryStream.Write(data, 0, data.Length);
        cryStream.FlushFinalBlock();

        return Convert.ToBase64String(ms.ToArray());
    }
    public static string Decrypt(string cypherString)
    {
        DESCryptoServiceProvider rc2 = new DESCryptoServiceProvider();

        rc2.Key = sSkey;
        rc2.IV = sSkey;

        MemoryStream ms = new MemoryStream();
        CryptoStream cryStream = new CryptoStream(ms, rc2.CreateDecryptor(), CryptoStreamMode.Write);

        byte[] data = Convert.FromBase64String(cypherString);

        cryStream.Write(data, 0, data.Length);
        cryStream.FlushFinalBlock();
        return Encoding.UTF8.GetString(ms.GetBuffer());
    }*/
    static public void SetInit(UserBase in_user)
    {
        sECRET_KEY = "myHa766b14dLs3w6UZvaZHTG4VAn115G";
        //sECRET_KEY = sECRET_KEY.Insert(2, in_user.authkey);
        sECRET_KEY = sECRET_KEY.Substring(0, "myHa766b14dLs3w6UZvaZHTG4VAn115G".Length);
        if (!string.Equals(PlayerPrefs.GetString("ds1$@d3kd3s@@1afm1*33"), sECRET_KEY))
        {
            //Debug.Log("-----------------.유저가 바뀜  - 로컬정보 리셋");

            DeleteKey("Protocol_AnimalCdn");
            DeleteKey("Protocol_ChapterCdn");
            DeleteKey("Protocol_StageCdn");

            DeleteKey("ProtocolCaching");
        }
        //Debug.Log("@@@@@@@@@@@@@@@@@@              void SetInit     " + in_user.authkey);

        PlayerPrefs.SetString("ds1$@d3kd3s@@1afm1*33", sECRET_KEY);
    }
  /*  static public bool IsInit()
    {
        return PlayerPrefs.HasKey("ds1$@d3kd3s@@1afm1*33");
    }
    */
    static void Init()
    {
        if (init)
            return;

        init = true;

        sECRET_KEY = PlayerPrefs.GetString("ds1$@d3kd3s@@1afm1*33", "myHa766b14dLs3w6UZvaZHTG4VAn115G");
      //  Debug.Log("@@@@@@@@@@@@@@@@@@              Init     " + sECRET_KEY);
       /* if (PlayerPrefs.HasKey("ds1$@d3kd3s@@1afm1*33"))
            sECRET_KEY = PlayerPrefs.GetString("d3kd3s1a_kimTT", "d3kd3s1a");
        else
        {

            string str = "";
            byte[] buffer = new byte[4];
            for (int b = 0; b < 4; ++b)
            {
                buffer[b] = (byte)(UnityEngine.Random.Range(0, 255));
                str += buffer[b].ToString("X").PadLeft(2, '0');
            }

            sECRET_KEY = str;
            PlayerPrefs.SetString("d3kd3s1a_kimTT", sECRET_KEY);
        }*/

        sSkey = System.Text.ASCIIEncoding.ASCII.GetBytes(sECRET_KEY);
        sSkey[1] = 3;
        sSkey[4] = 9;
        sSkey[5] = 9;
    }
  /*  public static void DeleteAll()
    {
        string text = PlayerPrefs.GetString("ds1$@d3kd3s@@1afm1*33", "d3kd3s1a");
        Init();
        PlayerPrefs.DeleteAll();
        PlayerPrefs.SetString("ds1$@d3kd3s@@1afm1*33", text);
    }*/
    public static void DeleteKey(string in_key)
    {
        if (in_key == null)
            return;

        if (in_key.Length <= 0)
            return;


        string name = Global.GetHashfromText(in_key);
        PlayerPrefs.DeleteKey(name);
    }
    static string getValue(string in_key)
    {
        Init();


        string name = Global.GetHashfromText(in_key);
        string value = PlayerPrefs.GetString(name, "no data");

        // Debug.Log(in_key +"  " +value);
        //Debug.Log("Encrypt  " + "  " + in_key + "  " + name + "  " + value);

        if (value != "no data")
        {
            value = Decrypt(value);
            return value;
        }
        return null;
    }
    static void setValue(string in_key, string in_value)
    {
        Init();


        string name = Global.GetHashfromText(in_key);
        string value = Encrypt(in_value);

        // Debug.Log("Encrypt  " + "  " + in_key + "  " + name + "  " + value);


        PlayerPrefs.SetString(name, value);
    }

    public static float GetFloat(string key)
    {
        string value = getValue(key);
        if (value != null)
            return float.Parse(value);
        return 0f;
    }
    public static float GetFloat(string key, float defaultValue)
    {
        string value = getValue(key);
        if (value != null)
            return float.Parse(value);
        return defaultValue;
    }
    public static int GetInt(string key)
    {
        string value = getValue(key);
        if (value != null)
            return int.Parse(value);
        return 0;
    }
    public static int GetInt(string key, int defaultValue)
    {
        string value = getValue(key);
        if (value != null)
            return int.Parse(value);
        return defaultValue;
    }
    public static long GetLong(string key, long defaultValue)
    {
        string value = getValue(key);
        if (value != null)
            return long.Parse(value);
        return defaultValue;
    }
    public static string GetString(string key)
    {
        string value = getValue(key);
        if (value != null)
            return value;
        return "";
    }
    public static string GetString(string key, string defaultValue)
    {
        string value = getValue(key);
        if (value != null)
            return value;
        return defaultValue;
    }
    public static bool HasKey(string key)
    {
        Init();
        string name = Global.GetHashfromText(key);
        return PlayerPrefs.HasKey(name);
    }

    //////////////////////////////////////////////////////////////////////////////

    public static void SetFloat(string key, float value)
    {
        setValue(key, value.ToString());
    }
    public static void SetInt(string key, int value)
    {
        setValue(key, value.ToString());
    }
    public static void SetString(string key, string value)
    {
        setValue(key, value);
    }
    public static void SetLong(string key, long value)
    {
        setValue(key, value.ToString());
    }
    public static void Test()
    {
        //   hash.Add("kskk", "a55555555555555aa");

        SetInt("aaa", 1234);
        Debug.Log(GetInt("aaa"));
        // Debug.Log(HasKey("kskk"));

        //     Debug.Log(GetString("kskk"));
        //  Debug.Log("Count " + hash.Count);
        //     for (int i = 0; i < hash.Count; i++)
        {
            //        Debug.Log(hash["aa"]);
            //      Debug.Log(GetString("aa"));
        }

    }
}