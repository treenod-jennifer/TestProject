using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UIItemDiaStashEvent : MonoBehaviour
{
    [SerializeField] private UILabel labelAddDia;
    [SerializeField] private UILabel labelStashDia;
    [SerializeField] private UISprite sprIcon;

    [SerializeField] private UISprite sprMultiplier;
    
    [SerializeField] private GameObject addDiaRoot;
    [SerializeField] private GameObject objAlarmBtn;
    [SerializeField] private GameObject fx_GetDia;
    
    [SerializeField] private TweenScale tweenScale;
    
    [SerializeField] private List<UISprite> listDiaStashSprite;
    
    private bool fullDiaActionStop = false;

    //패키지를 구매 했는지 확인 하는 값.
    private int currentGrade = 0;

    public void InitData()
    {
        if (fx_GetDia != null)
            fx_GetDia.SetActive(false);
        
        SetAtlas();
        
        labelStashDia.text = ManagerDiaStash.instance.GetIsFullDia(ManagerDiaStash.instance.currentUserDia);
        labelAddDia.text = $"{ManagerDiaStash.instance.GetAddBonusDia()}";

        sprIcon.spriteName = $"mayuji_package{ManagerDiaStash.instance.GetPackageGrade()}_UI";

        fullDiaActionStop = ManagerDiaStash.instance.currentUserDia == ManagerDiaStash.instance.GetFullDia();

        sprMultiplier.spriteName = $"mayuji_package_clear{ManagerDiaStash.instance.GetPackageGrade()}";
        
        objAlarmBtn.SetActive(ManagerDiaStash.instance.GetCurrentDia() == ManagerDiaStash.instance.GetFullDia());

        currentGrade = ManagerDiaStash.instance.GetPackageGrade();
    }
    
    private void SetAtlas()
    {
        for (int i = 0; i < listDiaStashSprite.Count; i++)
        {
            listDiaStashSprite[i].atlas = ManagerDiaStash.instance.diaStashResource.GetDiaStashPack().AtlasUI;
        }
    }
    
    public void StartDirecting()
    {
        if (fullDiaActionStop) return;

        if (ManagerDiaStash.instance.GetCurrentDia() == ManagerDiaStash.instance.currentUserDia) return;

        if (fx_GetDia != null && ManagerDiaStash.instance.IsFullDia())
            fx_GetDia.SetActive(true);
        addDiaRoot.SetActive(true);
    }
    
    public void StartAction()
    {
        StartCoroutine(CoAction());
    }

    private IEnumerator CoAction()
    {
        yield return new WaitForSeconds(0.45f);
        tweenScale.enabled = true;

        StartCoroutine(CoPlayDiaSound());

        yield return StartCoroutine(CoCountLabel(labelStashDia, ManagerDiaStash.instance.GetCurrentDia(), ManagerDiaStash.instance.currentUserDia, 0.6f));

        addDiaRoot.SetActive(false);
    }
    
    private IEnumerator CoCountLabel(UILabel uILabel, float target, float current, float time = 0.4f)
    {
        float duration = time;
        float offset = (target - current) / duration;

        while(current < target)
        {
            current += offset * Time.deltaTime;
            uILabel.text = ((int)current).ToString();

            yield return null;
        }

        tweenScale.enabled = false;
        tweenScale.GetComponent<Transform>().localScale = Vector3.one;

        current = target;
        ManagerDiaStash.instance.SyncUserDiaCount();
        uILabel.text = ((int)current).ToString();
    }

    void OnClickBtnDia()
    {
        ManagerUI._instance.OpenPopup<UIPopUpDiaStashEvent>((popup) =>
        {
            popup.InitData();
            popup.SetPurchaseCompleteEvent(PostAllBuyDiaStash);
        });
    }

    public void PostAllBuyDiaStash()
    {
        if(ManagerDiaStash.CheckStartable() == false)
        {
            gameObject.SetActive(false);
            return;
        }
        
        if(currentGrade != ManagerDiaStash.instance.GetPackageGrade())
            InitData();
    }
    
    IEnumerator CoPlayDiaSound()
    {
        while(tweenScale.enabled)
        {
            ManagerSound.AudioPlay(AudioInGame.COINSTASH_COIN);

            yield return new WaitForSeconds(0.1f);
        }
    }
}
