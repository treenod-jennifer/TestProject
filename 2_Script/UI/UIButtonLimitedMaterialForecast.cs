using System.Collections;
using System.Collections.Generic;

public class UIButtonLimitedMaterialForecast : UIButtonEventBase
{
    public  UILabel timeText;
    private long    expiredAt;
    private int     materialNo;
    private bool    bCantTouch = true;
    
    private static List<UIButtonLimitedMaterialForecast> objList    = new List<UIButtonLimitedMaterialForecast>();

    public static void RemoveAll()
    {
        var removeList = new List<UIButtonLimitedMaterialForecast>(objList);
        for (int i = 0; i < removeList.Count; i++)
        {
            if (removeList[i].gameObject != null)
            {
                ManagerUI._instance.ScrollbarRight.tempicons.Remove(removeList[i].gameObject);
                DestroyImmediate(removeList[i].gameObject);
                ManagerUI._instance.ScrollbarRight.PostRemoveTempIcon();
            }
        }

        objList.Clear();
    }

    public override void OnLoadComplete()
    {
        buttonTexture.gameObject.SetActive(true);
    }

    private void Awake()
    {
        objList.Add(this);
    }

    private void OnDestroy()
    {
        objList.Remove(this);
    }

    public void SetData(int materialNo, long expireAt)
    {
        if (gameObject.activeInHierarchy == false)
            return;

        this.expiredAt  = expireAt;
        this.materialNo = materialNo;

        StartCoroutine(CoPackageTime());
        buttonTexture.SettingTextureScale(80, 80);
        buttonTexture.SuccessEvent += OnLoadComplete;
        buttonTexture.LoadCDN(Global.gameImageDirectory, "IconMaterial/", "mt_" + materialNo.ToString());
    }

    private IEnumerator CoPackageTime()
    {
        yield return null;
        long leftTime = 0;

        while (gameObject.activeInHierarchy == true)
        {
            leftTime = Global.LeftTime(this.expiredAt);
            if (leftTime <= 0)
            {
                leftTime      = 0;
                timeText.text = "00:00:00";
                ManagerUI._instance.UpdateUI();
                yield break;
            }

            timeText.text = Global.GetLeftTimeText(this.expiredAt);
            yield return null;
        }

        yield return null;
    }

    private void TouchOn()
    {
        bCantTouch = true;
    }
    
    private void OnEnable()
    {
        StartCoroutine(CoPackageTime());
    }

    private void OnClicked()
    {
        if (ManagerLobby._instance == null || !ManagerLobby._instance.IsLobbyComplete)
        {
            return;
        }
        
        var infoPopup = ManagerUI._instance.OpenPopup<UIPopupEventLimitedMaterialInfo>();
        infoPopup.SetData(expiredAt, materialNo);
    }
}