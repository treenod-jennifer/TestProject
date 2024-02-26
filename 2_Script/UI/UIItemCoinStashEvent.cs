using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UIItemCoinStashEvent : MonoBehaviour
{
    [SerializeField] private UILabel labelAddCoin;
    [SerializeField] private UILabel labelStashCoin;

    [SerializeField] private UISprite sprMultiplier;

    [SerializeField] private GameObject AddCoinRoot;
    [SerializeField] private GameObject StashCoinRoot;
    [SerializeField] private GameObject objAlarmBtn;
    [SerializeField] private GameObject fx_GetCoin;

    [SerializeField] private List<GameObject> objCoinItmes;

    [SerializeField] private GameObject objCoinItem;

    [SerializeField] private TweenScale tweenScale;

    private bool FullCoinActionStop = false;

    public void InitData()
    {
        if (fx_GetCoin != null)
            fx_GetCoin.SetActive(false);
        labelStashCoin.text = ManagerCoinStashEvent.GetIsFullCoin(ManagerCoinStashEvent.currentUserCoin);
        labelAddCoin.text = $"{ServerRepos.UserCoinStash.storedCoin - ManagerCoinStashEvent.currentUserCoin}";

        FullCoinActionStop = ManagerCoinStashEvent.currentUserCoin == ServerContents.CoinStashEvent.coinMax;

        sprMultiplier.spriteName = $"clear_coinstash_{ServerContents.CoinStashEvent.CalcMultiplierLevel(ManagerCoinStashEvent.currentCoinMultiplierState)}";
        
        objAlarmBtn.SetActive(ManagerCoinStashEvent.GetCoinStashState() > ManagerCoinStashEvent.CoinStashState.NOT_BUY_COUNTLACK);
    }

    public void StartDirecting()
    {
        if (FullCoinActionStop) return;

        if (ServerRepos.UserCoinStash.storedCoin == ManagerCoinStashEvent.currentUserCoin) return;

        if (fx_GetCoin != null)
            fx_GetCoin.SetActive(true);
        AddCoinRoot.SetActive(true);
    }

    public void StartAction()
    {
        StartCoroutine(CoAction());
    }

    //연출 변경시 수정.
    private IEnumerator CoAction()
    {
        //yield return StartCoroutine(CoObjectMove(AddCoinRoot.gameObject, StashCoinRoot.transform.localPosition, AddCoinRoot.transform.localPosition));
        yield return new WaitForSeconds(0.45f);
        tweenScale.enabled = true;

        StartCoroutine(CoPlayCoinSound());

        yield return StartCoroutine(CoCountLabel(labelStashCoin, ServerRepos.UserCoinStash.storedCoin, ManagerCoinStashEvent.currentUserCoin, 0.6f));

        AddCoinRoot.SetActive(false);
    }

    public void StartCoinCount()
    {
        AddCoinRoot.SetActive(false);
        StartCoroutine(CoCountLabel(labelStashCoin, ServerRepos.UserCoinStash.storedCoin, ManagerCoinStashEvent.currentUserCoin, 0.8f));
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
        ManagerCoinStashEvent.currentUserCoin = (int)current;
        uILabel.text = ((int)current).ToString();
    }

    private IEnumerator CoObjectMove(GameObject objCurrent, Vector3 target, Vector3 current, float time = 0.2f)
    {
        float duration = time;
        var offset = (target - current) / duration;

        if(objCurrent.GetComponent<TweenAlpha>() != null)
            objCurrent.GetComponent<TweenAlpha>().enabled = true;

        while (current.y < target.y)
        {
            current += offset * Time.deltaTime;
            objCurrent.transform.localPosition = current;

            yield return null;
        }

        current = target;
        objCurrent.transform.localPosition = current;
    }

    private void OnClickBtnCoinStage()
    {
        ManagerUI._instance.OpenPopup<UIPopupCoinStashEvent>()._callbackClose += () => PostAllBuyCoinStash();
    }


    public void PostAllBuyCoinStash()
    {
        if (ManagerCoinStashEvent.IsBuyCoinStash() == false) InitData();

        if(ServerContents.CoinStashEvent.maxBuyCount > 0 && ServerRepos.UserCoinStash.buyCount == ServerContents.CoinStashEvent.maxBuyCount)
        {
            this.gameObject.SetActive(false);
        }
    }

    IEnumerator CoPlayCoinSound()
    {
        while(tweenScale.enabled)
        {
            ManagerSound.AudioPlay(AudioInGame.COINSTASH_COIN);

            yield return new WaitForSeconds(0.1f);
        }
    }
}