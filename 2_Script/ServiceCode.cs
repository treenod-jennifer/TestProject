using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class ServiceCode
{
    public static string BuildServiceCode()
    {
        string cdnString = ServerRepos.CdnVsn.ToString();
        string cdnAddr = NetworkSettings.Instance.GetCDN_URL();
        string serverAddr = NetworkSettings.Instance.GetServer_URL();
        string svrBuild = "s-";

        if (ServerRepos.svrBuild.Contains("PUZZLE Server"))
        {
            string tmpString = ServerRepos.svrBuild.Replace("PUZZLE Server ", "");
            int substrLen = tmpString.IndexOf(" (build");
            if (substrLen > 8)
                substrLen = 8;
            svrBuild = tmpString.Substring(0, substrLen);
        }

        //RFC 3548, 4648 : safe filename base64 encoding
        string shortHash = HashChecker.ShortHash;
        shortHash = shortHash.Replace("/", "_");
        shortHash = shortHash.Replace("+", "-");
        shortHash = shortHash.Replace("=", "");

        return string.Format($"{cdnString}.{GetMD5(cdnAddr)}.{GetMD5(serverAddr)}.{shortHash}.{svrBuild}");

    }

    private static string GetMD5(string str)
    {
        var byteArr = System.Text.Encoding.ASCII.GetBytes(str);

        using (var md5 = System.Security.Cryptography.MD5.Create())
        {
            return ByteArrayToHexString(md5.ComputeHash(byteArr), 2);
        }
    }

    private static string ByteArrayToHexString(byte[] bytes, int byteCrop)
    {
        System.Text.StringBuilder builder = new System.Text.StringBuilder();
        for (int i = 0; i < bytes.Length && i < byteCrop; ++i)
            builder.Append(bytes[i].ToString("X2"));

        return builder.ToString();
    }
}
