using System.Text;
using System.Globalization;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using UnityEngine;

public static class LanguageUtility
{
    private class LanguageInfo
    {
        public SystemLanguage Language { get; }
        public bool IsTest { get; }

        public LanguageInfo(SystemLanguage language, bool isTest = false)
        {
            Language = language;
            IsTest = isTest;
        }
    }

    private class LanguageInfoCollections
    {
        private Dictionary<string, LanguageInfo> languageInfoCollections = null;

        private Dictionary<string, SystemLanguage> dic = null;

        private Dictionary<SystemLanguage, string> dicMirror = null;



        public ReadOnlyDictionary<string, SystemLanguage> Dic { get; }

        public ReadOnlyDictionary<SystemLanguage, string> DicMirror { get; }

        public LanguageInfoCollections(Dictionary<string, LanguageInfo> languageInfo)
        {
            if (languageInfo == null) return;


            languageInfoCollections = languageInfo;
            

            dic = new Dictionary<string, SystemLanguage>();

            foreach (var info in languageInfoCollections)
            {
                if (IncludeTestLanguage || (!IncludeTestLanguage && !info.Value.IsTest))
                {
                    dic.Add(info.Key, info.Value.Language);
                }
            }

            Dic = new ReadOnlyDictionary<string, SystemLanguage>(dic);


            dicMirror = new Dictionary<SystemLanguage, string>();

            foreach (var info in dic)
            {
                dicMirror.Add(info.Value, info.Key);
            }

            DicMirror = new ReadOnlyDictionary<SystemLanguage, string>(dicMirror);
        }
    }



    private const string FORCED_CHANGE_LANGUAGE_KEY = "ForcedChangeLanguage";

    private const SystemLanguage DEFAULT_LANGUAGE = SystemLanguage.Japanese;

    private static LanguageInfoCollections languageInfoCollections = new LanguageInfoCollections(languageInfo: new Dictionary<string, LanguageInfo>()
    {
        { "kr", new LanguageInfo(language: SystemLanguage.Korean, isTest: true) },
        { "jp", new LanguageInfo(language: SystemLanguage.Japanese) },
        { "tw", new LanguageInfo(language: SystemLanguage.ChineseTraditional) }
    });

    /// <summary>
    /// 테스트용 언어의 사용여부 입니다.
    /// </summary>
    private static bool IncludeTestLanguage
    {
        get
        {
            if(NetworkSettings.Instance.buildPhase == NetworkSettings.eBuildPhases.SANDBOX)
            {
                if (NetworkSettings.Instance.serverTarget == NetworkSettings.ServerTargets.DevQAServer)
                    return false;
                else
                    return true;
            }
            else
            {
                return false;
            }

            return NetworkSettings.Instance.buildPhase == NetworkSettings.eBuildPhases.SANDBOX;
        }
    }

    /// <summary>
    /// 유저가 설정한 언어 입니다.
    /// </summary>
    private static SystemLanguage ForcedChangeLanguage
    {
        get
        {
            bool hasPlayerPrefsKey = PlayerPrefs.HasKey(FORCED_CHANGE_LANGUAGE_KEY);

            if (hasPlayerPrefsKey)
            {
                return (SystemLanguage)PlayerPrefs.GetInt(FORCED_CHANGE_LANGUAGE_KEY);
            }
            else
            {
                return SystemLanguage.Unknown;
            }
        }
        set
        {
            PlayerPrefs.SetInt(FORCED_CHANGE_LANGUAGE_KEY, (int)value);
        }
    }

    /// <summary>
    /// 디바이스의 언어 입니다.
    /// </summary>
    private static SystemLanguage DeviceLanguage
    {
        get
        {
            var language = Application.systemLanguage;
            if (language == SystemLanguage.Japanese || language == SystemLanguage.ChineseTraditional)
            {
                return language;
            }
            else
            {
                return SystemLanguage.Japanese;
            }
        }
    }

    /// <summary>
    /// 트라이던트의 언어 입니다.
    /// </summary>
    private static SystemLanguage TridentLanguage
    {
        get
        {
            return CountryCodeToLanguage(TridentCountryCode);
        }
    }

    /// <summary>
    /// 트라이던트의 국가 코드 입니다.
    /// </summary>
    private static string TridentCountryCode
    {
        get
        {
            return ServiceSDK.ServiceSDKManager.instance?.GetCountryCode().ToLower();
        }
    }



    /// <summary>
    /// 현재 포코팡타운의 언어 입니다.
    /// </summary>
    public static SystemLanguage SystemLanguage
    {
        get
        {
            // 1순위 : 유저가 직접 설정한 언어, 2순위 : 디바이스 언어, 3순위 : 트라이던트 언어, 4순위 : 기본 설정 (일본어)
            if (IsAvailableLanguage(ForcedChangeLanguage))
            {
                return ForcedChangeLanguage;
            }
            else if (IsAvailableLanguage(DeviceLanguage))
            {
                return DeviceLanguage;
            }
            else if (IsAvailableLanguage(TridentLanguage))
            {
                return TridentLanguage;
            }
            else
            {
                return DEFAULT_LANGUAGE;
            }

            bool IsAvailableLanguage(SystemLanguage language)
            {
                return languageInfoCollections.DicMirror.ContainsKey(language);
            }
        }
        set
        {
            ForcedChangeLanguage = value;
        }
    }

    /// <summary>
    /// 포코팡 타운의 국가 코드 입니다.
    /// </summary>
    public static string SystemCountryCode
    {
        get
        {
            return LanguageToCountryCode(SystemLanguage);
        }
    }

    public static string SystemCountryCodeForAssetBundle
    {
        get
        {
            var language = SystemLanguage;
            if (language == SystemLanguage.Korean)
            {
                language = SystemLanguage.Japanese;
            }

            return LanguageToCountryCode(language);
        }
    }

    /// <summary>
    /// 유저가 설정한 언어값이 있는지 나타냅니다.
    /// </summary>
    public static bool HasForcedChangeLanguage
    {
        get
        {
            return PlayerPrefs.HasKey(FORCED_CHANGE_LANGUAGE_KEY);
        }
    }



    /// <summary>
    /// 국가 코드를 언어 정보로 변환 합니다.
    /// </summary>
    public static SystemLanguage CountryCodeToLanguage(string countryCode)
    {
        if (languageInfoCollections.Dic.TryGetValue(countryCode, out SystemLanguage language))
        {
            return language;
        }
        else
        {
            return SystemLanguage.Unknown;
        }
    }

    /// <summary>
    /// 언어 정보를 국가 코드로 변환 합니다.
    /// </summary>
    public static string LanguageToCountryCode(SystemLanguage language)
    {
        if (languageInfoCollections.DicMirror.TryGetValue(language, out string country))
        {
            return country;
        }
        else
        {
            return string.Empty;
        }
    }

    /// <summary>
    /// 서버에 저장된 가격 정보에서 현재 시스템에 설정된 언어에 해당하는 가격을 가져옵니다.
    /// </summary>
    /// <param name="prices">서버에 저장된 배열 형태의 가격 정보</param>
    /// <returns>가격(단위 포함)</returns>
    public static string GetPrices(List<string> prices)
    {
        if (prices == null || prices.Count == 0)
        {
            return string.Empty;
        }

        string price;

        try
        {
            switch (SystemLanguage)
            {
                case SystemLanguage.Japanese:
                    price = prices[1];
                    break;
                case SystemLanguage.ChineseTraditional:
                    price = prices[2];
                    break;
                default:
                    price = prices[0];
                    break;
            }

            return $"{Global._instance.GetString("cur_1")}{price}";
        }
        catch (System.ArgumentOutOfRangeException)
        {
            return "Unknown";
        }
    }
    
    /// <summary>
    /// 서버에 저장된 가격 정보에서 현재 시스템에 설정된 언어에 해당하는 가격을 가져옵니다. (가격 비교, int 형)
    /// </summary>
    /// <param name="prices">서버에 저장된 배열 형태의 가격 정보</param>
    /// <returns>가격(단위 포함)</returns>
    public static double GetPrice(List<string> prices)
    {
        if (prices == null || prices.Count == 0)
        {
            return -1;
        }

        string price;

        try
        {
            switch (SystemLanguage)
            {
                case SystemLanguage.Japanese:
                    price = prices[1];
                    break;
                case SystemLanguage.ChineseTraditional:
                    price = prices[2];
                    break;
                default:
                    price = prices[0];
                    break;
            }

            double.TryParse(price, NumberStyles.Number, CultureInfo.InvariantCulture, out double numValue);
            return numValue;
        }
        catch (System.ArgumentOutOfRangeException)
        {
            return -1;
        }
    }

    /// <summary>
    /// 일반 파일명에 국가 코드를 붙여줍니다.
    /// </summary>
    /// <param name="fileName">원본 파일명</param>
    /// <returns>국가 코드를 포함한 파일명</returns>
    public static string FileNameConversion(string fileName)
    {
        if (string.IsNullOrEmpty(fileName))
        {
            return string.Empty;
        }


        const char SEPARATOR = '.';

        string country = $"_{LanguageToCountryCode(SystemLanguage)}";

        StringBuilder stringBuilder = new StringBuilder(fileName, fileName.Length + country.Length);

        for (int i = stringBuilder.Length - 1; i >= 0; i--)
        {
            if (stringBuilder[i] == SEPARATOR)
            {
                stringBuilder.Insert(i, country);
                return stringBuilder.ToString();
            }
        }

        stringBuilder.Append(country);
        return stringBuilder.ToString();
    }

    /// <summary>
    /// 포코팡 타운에서 지원하는 언어 목록을 가져옵니다.
    /// </summary>
    /// <returns></returns>
    public static List<SystemLanguage> GetLanguageList()
    {
        List<SystemLanguage> languages = new List<SystemLanguage>();

        foreach (var language in languageInfoCollections.Dic)
        {
            languages.Add(language.Value);
        }

        return languages;
    }
    
    public static bool IsShowBuyInfo => SystemLanguage == SystemLanguage.Japanese;
}
