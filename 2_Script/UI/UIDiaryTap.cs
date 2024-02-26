using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

public class UIDiaryTap : MonoBehaviour
{
    public TypePopupDiary tapType = TypePopupDiary.eNone;

    //버튼 애니메이션.
    public new Animation animation;

    //버튼 아이콘 스프라이트.
    public GameObject iconObj;
    //버튼 이벤트 아이콘.
    public GameObject eventIconObj;
    //버튼 알림 아이콘.
    public GameObject alarmIconObj;
    //버튼 스프라이트.
    public UISprite tap;
    //버튼 아이콘 스프라이트.
    public UISprite[] tapIcon;
    //버튼 글자 스프라이트.
    public UIItemLabelActiveColor tapText;
    //버튼 그림자 스프라이트.
    public UISprite tapShadow;

    private Color BTN_SPRITE_COLOR = new Color(180f / 255f, 180f / 255f, 180f / 255f);
    private float moveTime = 0.1f;
    private bool bCurrent = false;

    [Header("DisableLink")]
    [SerializeField] private GameObject EnableObject;
    [SerializeField] private GameObject DisableObject;


    public void SetDiaryTap(bool bCurrentButton)
    {
        bCurrent = bCurrentButton;
        if (bCurrent == true)
        {
            tap.spriteName = "diary_tap_01";
            for (int i = 0; i < tapIcon.Length; i++)
            {
                UISprite icon = tapIcon[i];
                DOTween.To(() => icon.color, x => icon.color = x, Color.white, moveTime);
            }
            DOTween.To(() => tapText.SetLabel, x => tapText.SetLabel = x, tapText.SetActiveColor(bCurrent, UIItemLabelActiveColor.ColorType.NORMAL), moveTime);
            DOTween.To(() => tapText.SetShadowLabel, x => tapText.SetShadowLabel = x, tapText.SetActiveColor(bCurrent, UIItemLabelActiveColor.ColorType.SHADOW), moveTime);
            DOTween.To(() => tapShadow.color, x => tapShadow.color = x, Color.white, moveTime);
            tapText.transform.DOScale(Vector3.one, moveTime);
            tapShadow.transform.DOScale(Vector3.one, moveTime);

            //애니메이션 재생.
            if (animation != null)
            {
                animation.Stop();
                string aniName = GetTapIconAnimationName() + "appear";
                animation.Play(aniName);
                animation.wrapMode = WrapMode.Once;
                StartCoroutine(CoAnimationLoop(aniName));
            }

            if (eventIconObj != null)
            {
                eventIconObj.transform.DOLocalMove(new Vector3(-37.5f, 70f, 0f), 0.1f);
                eventIconObj.transform.DORotate(new Vector3(0f, 0f, 20f), 0.1f);
            }
        }
        else
        {   
            float textScale = 0.75f;
            if (tapType == TypePopupDiary.eCostume)
            {
                textScale = 0.9f;
            }
            tap.spriteName = "diary_tap_02";

            tapText.SetLabel = tapText.SetActiveColor(true, UIItemLabelActiveColor.ColorType.NORMAL);
            tapText.SetShadowLabel = tapText.SetActiveColor(true, UIItemLabelActiveColor.ColorType.SHADOW);

            tapShadow.color = Color.white;
            for (int i = 0; i < tapIcon.Length; i++)
            {
                UISprite icon = tapIcon[i];
                icon.color = Color.white;
                DOTween.To(() => icon.color, x => icon.color = x, BTN_SPRITE_COLOR, moveTime);
            }
            DOTween.To(() => tapText.SetLabel, x => tapText.SetLabel = x, tapText.SetActiveColor(bCurrent, UIItemLabelActiveColor.ColorType.NORMAL), moveTime);
            DOTween.To(() => tapText.SetShadowLabel, x => tapText.SetShadowLabel = x, tapText.SetActiveColor(bCurrent, UIItemLabelActiveColor.ColorType.SHADOW), moveTime);

            DOTween.To(() => tapShadow.color, x => tapShadow.color = x, BTN_SPRITE_COLOR, moveTime);
            tapText.transform.DOScale(Vector3.one * textScale, moveTime);
            tapShadow.transform.DOScale(Vector3.one * 0.75f, moveTime);

            //애니메이션 재생. 
            if (animation != null)
            {
                animation.Stop();
                string aniName = GetTapIconAnimationName() + "idle";
                animation.Play(aniName);
            }

            if (eventIconObj != null)
            {
                eventIconObj.transform.DOLocalMove(new Vector3(0f, 50f, 0f), 0.1f);
                eventIconObj.transform.DORotate(new Vector3(0f, 0f, 0f), 0.1f);
            }
        }
    }
    
    public void SettingAlarmIcon(bool bAlarm)
    {
        if(alarmIconObj != null && alarmIconObj.activeInHierarchy != bAlarm)
            alarmIconObj.SetActive(bAlarm);
    }

    public void SettingEventIcon(bool bEvnet)
    {
        eventIconObj.SetActive(bEvnet);
    }

    public bool CheckEventIconState()
    {
        if (eventIconObj == null)
            return false;
        return eventIconObj.activeInHierarchy;
    }

    IEnumerator CoAnimationLoop(string currentAniName)
    {
        while (bCurrent == true && animation.IsPlaying(currentAniName) == true)
        {
            yield return null;
        }
        yield return null;

        string aniName = GetTapIconAnimationName() + "loop";
        if (bCurrent == true)
        {
            animation.Play(aniName);
            animation.wrapMode = WrapMode.Loop;
        }
    }

    string GetTapIconAnimationName()
    {
        if (tapType == TypePopupDiary.eMission)
            return "mission_";
        else if (tapType == TypePopupDiary.eStorage)
            return "storageBox_";
        else if (tapType == TypePopupDiary.eStamp)
            return "stampbox_";
        else if (tapType == TypePopupDiary.eCostume)
            return "costume_";
        else if (tapType == TypePopupDiary.eGuest)
            return "guest_";
        else
            return "";
    }

    /// <summary>
    /// 탭의 기능을 사용하지 않을 때
    /// </summary>
    /// <param name="isActive"></param>
    public void InitTapLock(bool isMainLand, out bool isLock)
    {
        if (EnableObject == null || DisableObject == null) isLock = false;

        if(isMainLand)
        {
            EnableObject.SetActive(false);
            DisableObject.SetActive(true);
            this.GetComponent<BoxCollider>().enabled = false;

            isLock = true;
        }
        else
        {
            EnableObject.SetActive(true);
            DisableObject.SetActive(false);
            this.GetComponent<BoxCollider>().enabled = true;

            isLock = false;
        }
    }
}
