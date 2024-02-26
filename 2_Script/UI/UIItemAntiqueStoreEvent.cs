using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;

public class UIItemAntiqueStoreEvent : MonoBehaviour
{
    [Header("Icon")] 
    [SerializeField] private GameObject objIcon;
    [SerializeField] private GameObject objEffect;
    [SerializeField] private UISprite sprIconBG;

    [Header("Label")] 
    [SerializeField] private UILabel labelUserToken;
    [SerializeField] private UILabel labelFullToken;

    //번들 처리 sprite
    [SerializeField] private List<UISprite> sprAntiqueStoreIconList;

    private void Start()
    {
        for (int i = 0; i < sprAntiqueStoreIconList.Count; i++)
        {
            sprAntiqueStoreIconList[i].atlas =
                ManagerAntiqueStore.instance.antiqueStoreResource.antiqueStorePack.AtlasUI;
        }
    }

    public void InitData()
    {
        labelFullToken.text = $"/{ServerContents.AntiqueStore.assetMaxCount}";
        labelUserToken.text = $"{ManagerAntiqueStore.instance.currentUserToken}";
        sprIconBG.spriteName = $"AntiqueStore_e_iconBG_{(ManagerAntiqueStore.instance.isSpecialEvent ? "2" : "1")}";
    }

    public void StartDirection()
    {
        if (ServerRepos.UserAntiqueStore.assetAmount == ManagerAntiqueStore.instance.currentUserToken) return;
        
        StartCoroutine(CoAction());
    }

    private IEnumerator CoAction()
    {
        objIcon.GetComponent<TweenScale>().enabled = true;
        objEffect.SetActive(true);
        
        yield return CoCountLabel(labelUserToken, ServerRepos.UserAntiqueStore.assetAmount, ManagerAntiqueStore.instance.currentUserToken, 0.9f);

        var effectColor = objEffect.GetComponent<UISprite>().color;

        DOTween.ToAlpha(() => effectColor, x => effectColor = x, 0f, 0.5f);
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

        current = target;
        ManagerAntiqueStore.instance.SyncUserToken();
        uILabel.text = ((int)current).ToString();
    }

    private void OnClickAntiqueStoreOpen()
    {
        ManagerAntiqueStore.instance.PostCheckAntiqueStorePopupOpen_CheckEvent(null, InitData);
    }
}
