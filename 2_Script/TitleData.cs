using System;
using UnityEngine;

public class TitleData : MonoBehaviour {
    [Obsolete]
    public int version;
    public string copyright;
    public Texture2D copyright_Texture;
    public float audioDelay = 0.2f;
    public AudioClip audioStart = null;
    public AudioClip audioLoop = null;

    public long startTs_localTime = 0;
    public long endTs_localTime = 0;

    public int alterAppIcon = 0;

    public string alterNotificationFile = "";

    [SerializeField] private UIUrlTexture title;

    private const string TITLE_PATH = "Title/title.png";

    private void Start () 
    {
        string path = LanguageUtility.FileNameConversion(TITLE_PATH);
        title.LoadStreaming(path);
    }

    public bool IsExpired()
    {
        if (startTs_localTime == 0 && endTs_localTime == 0)
            return false;

        DateTime origin = new DateTime(1970, 1, 1, 0, 0, 0, 0).ToLocalTime();
        TimeSpan diff = DateTime.Now - origin;
        long diffTs = (long)Math.Floor(diff.TotalSeconds);

        bool startCheck = !(startTs_localTime != 0 && startTs_localTime > diffTs); // 시작도 안했거나.
        bool endCheck = !(endTs_localTime != 0 && endTs_localTime < diffTs); // 이미 끝났거나.

        if (startCheck == false || endCheck == false)
            return true;

        return false;
    }
}
