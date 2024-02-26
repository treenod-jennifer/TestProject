using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UIButtonPackage : MonoBehaviour, IImageRequestable
{
    public UITexture icon;
    public UILabel timeText;
    //private CdnShopPackage packageData;
    private long expiredAt; // 패키지데이터에 들어있는 expireTs 혹은 display_day 등등이 전부 적용된 만료시간

    private bool bCantTouch = true;

    static public List<UIButtonPackage> packageList = new List<UIButtonPackage>();

    static public void RemoveAll()
    {
        var removeList = new List<UIButtonPackage>(packageList);
        for (int i = 0; i < removeList.Count; i++)
        {
            if (removeList[i].gameObject != null)
            {
                ManagerUI._instance.anchorTopRight.DestroyLobbyButton(removeList[i].gameObject);
                DestroyImmediate(removeList[i].gameObject);
            }
        }
        packageList.Clear();
    }

    void Awake()
    {
        packageList.Add(this);
    }
    void OnDestroy()
    {
        packageList.Remove(this);
    }
    public void OnLoadComplete(ImageRequestableResult r)
    {
        icon.mainTexture = r.texture;
        icon.MakePixelPerfect();
    }

    public void OnLoadFailed() { }
    public int GetWidth()
    {
        return 0;
    }
    public int GetHeight()
    {
        return 0;
    }
    /*
    public void SettingBtnPackage(CdnShopPackage data, long expireAt)
    {
        if (gameObject.activeInHierarchy == false)
            return;

        this.expiredAt = expireAt;
        packageData = data;
        StartCoroutine(CoPackageTime());
        UIImageLoader.Instance.Load(Global.gameImageDirectory, "Shop/", data.image + "_icon", this);
    }
    */
    IEnumerator CoPackageTime()
    {
        yield return null;
        long leftTime = 0;
        
        while (gameObject.activeInHierarchy == true)
        {
            leftTime = Global.LeftTime(this.expiredAt);
            if (leftTime <= 0)
            {
                leftTime = 0;
                timeText.text = "00:00:00";
                ManagerUI._instance.UpdateUI();
                yield break;
            }
            timeText.text = Global.GetTimeText_HHMMSS(this.expiredAt);
            yield return null;
        }
        yield return null;
    }

    void OnClickBtnPackage()
    {
        //터치 가능 체크.
        if (bCantTouch == false)
            return;

        bCantTouch = false;
       // ManagerUI._instance.OpenPopupPackage(packageData, TouchOn);
    }

    void TouchOn()
    {
        bCantTouch = true;
    }

    private void OnEnable()
    {
      //  if (packageData != null)
        {
            StartCoroutine(CoPackageTime());
        }
    }
}
