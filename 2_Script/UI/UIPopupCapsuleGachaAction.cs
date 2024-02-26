using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Spine.Unity;

public class UIPopupCapsuleGachaAction : UIPopupBase
{

    [Header("Phase1Link")]
    [SerializeField] private SkeletonAnimation spineGachaObject;
    [SerializeField] private GameObject skipBtnObj;

    [Header("Phase2Link")]
    [SerializeField] private GameObject CapsuleRoot;
    [SerializeField] private GameObject oneGachaRoot;
    [SerializeField] private GameObject tenGachaRoot;
    [SerializeField] private GameObject objLastPhaseUIRoot;
    [SerializeField] private UISprite sprBlackOut;

    [Header("SkipLink")]
    [SerializeField] private GameObject objWhiteOut;

    private List<GradeReward> capsuleReward = new List<GradeReward>();
    private List<UIItemCapsuleitem> capuleItemObject = new List<UIItemCapsuleitem>();

    // 타임 스케일 오류 체크
    private bool checkSkipTime = false;

    // 백버튼 딜레이 체크
    private bool isCanClickBackButton = false;

    public override void SettingSortOrder(int layer)
    {
        base.SettingSortOrder(layer);
        spineGachaObject.GetComponent<MeshRenderer>().sortingOrder = layer + 2;
    }

    public override void OnClickBtnBack()
    {
        if (bCanTouch == false)
            return;
        if (skipBtnObj.activeSelf)
            OnClickBtnSkip();
        else if (isCanClickBackButton)
            OnClickBtnClose();
    }

    protected override void OnClickBtnClose()
    {
        ManagerSound._instance.UnPauseBGM();
        Time.timeScale = 1.0f;
        checkSkipTime = true;

        base.OnClickBtnClose();
    }

    public void InitPopup(List<GradeReward> capsuleReward)
    {
        this.capsuleReward = capsuleReward;

        ManagerSound._instance.PauseBGM();

        SetRewardItem();

        StartCoroutine(StartPhase1(GetGachaAnimationName(ManagerCapsuleGachaEvent.GetGachaType(this.capsuleReward))));
    }

    void SetRewardItem()
    {
        if (ManagerCapsuleGachaEvent.GetGachaType(this.capsuleReward) == ManagerCapsuleGachaEvent.GachaType.TENGACHA)
        {
            oneGachaRoot.SetActive(false);
            tenGachaRoot.SetActive(true);

            capuleItemObject.AddRange(tenGachaRoot.GetComponentsInChildren<UIItemCapsuleitem>());
        }
        else
        {
            oneGachaRoot.SetActive(true);
            tenGachaRoot.SetActive(false);

            capuleItemObject.AddRange(oneGachaRoot.GetComponentsInChildren<UIItemCapsuleitem>());
        }

        for (int i = 0; i < capuleItemObject.Count; i++)
        {
            capuleItemObject[i].UpdataData(capsuleReward[i]);
        }
    }

    IEnumerator StartPhase1(string aniName)
    {
        isCanClickBackButton = false;
        yield return new WaitForSeconds(0.2f);

        spineGachaObject.gameObject.SetActive(true);
        spineGachaObject.state.SetAnimation(0, aniName, false);

        yield return new WaitForSeconds(0.3f);

        if (isSkip == false)
            ManagerSound.AudioPlay(AudioLobby.event_capsule_coin);

        yield return new WaitForSeconds(0.4f);

        if (isSkip == false)
        {
            if (ManagerCapsuleGachaEvent.GetGachaType(this.capsuleReward) == ManagerCapsuleGachaEvent.GachaType.NORMAL)
                ManagerSound.AudioPlay(AudioLobby.event_capsule_shake_n);
            else
                ManagerSound.AudioPlay(AudioLobby.event_capsule_shake_r);
        }

        yield return new WaitForSeconds(1.05f);

        if (isSkip == false)
        {
            if (ManagerCapsuleGachaEvent.GetGachaType(this.capsuleReward) == ManagerCapsuleGachaEvent.GachaType.TENGACHA)
                ManagerSound.AudioPlay(AudioLobby.event_capsule_out_r);
            else
                ManagerSound.AudioPlay(AudioLobby.event_capsule_out_n);
        }

        spineGachaObject.state.Complete += delegate
        {
            spineGachaObject.gameObject.SetActive(false);

            CapsuleRoot.SetActive(true);

            StartCoroutine(CoStartPhase2());
        };
    }

    IEnumerator CoBlackOutFade()
    {
        float time = 0f;
        while (time < 1f)
        {
            time += Time.deltaTime;

            sprBlackOut.color = new Color(0f, 0f, 0f, Mathf.Lerp(1f, 0f, time));
            yield return null;
        }
    }

    IEnumerator CoStartPhase2()
    {
        if (ManagerCapsuleGachaEvent.GetGachaType(this.capsuleReward) == ManagerCapsuleGachaEvent.GachaType.NORMAL)
            ManagerSound.AudioPlay(AudioLobby.phonebox_result_3star);
        else
            ManagerSound.AudioPlay(AudioLobby.phonebox_result_5star);

        skipBtnObj.SetActive(false);

        yield return StartCoroutine(CoBlackOutFade());

        for (int i = 0; i < capuleItemObject.Count; i++)
        {
            capuleItemObject[i].OpenCapsule(isSkip);

            yield return new WaitForSeconds(0.2f);
        }

        skipTrigger = true;
        objLastPhaseUIRoot.SetActive(true);
        skipBtnObj.SetActive(false);
        yield return new WaitForSeconds(0.5f);
        isCanClickBackButton = true;

        yield return null;
    }

    string GetGachaAnimationName(ManagerCapsuleGachaEvent.GachaType aniName)
    {
        switch (aniName)
        {
            case ManagerCapsuleGachaEvent.GachaType.NORMAL:
                return "1_appear";
            case ManagerCapsuleGachaEvent.GachaType.RARE:
                return "2_appear";
            case ManagerCapsuleGachaEvent.GachaType.TENGACHA:
                return "3_appear";
            default:
                return null;
        }
    }

    bool skipTrigger = false;
    bool isSkip = false;

    public void OnClickBtnSkip()
    {
        if (skipBtnObj.activeSelf == false) return;

        isSkip = true;

        skipBtnObj.SetActive(false);

        objWhiteOut.SetActive(true);

        StartCoroutine(CoSkipAction());
    }

    IEnumerator CoSkipAction()
    {
        yield return new WaitUntil(() => objWhiteOut.GetComponent<UISprite>().color.a > 0.9f);

        if (checkSkipTime)
            checkSkipTime = false;
        else
            Time.timeScale = 10.0f;

        yield return new WaitUntil(() => skipTrigger == true);

        Time.timeScale = 1.0f;

        yield return new WaitForSeconds(1.0f);
    }
}
