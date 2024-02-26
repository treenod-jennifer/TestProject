using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UIPopupAnimalRewardInfo : UIPopupBase
{
    public static UIPopupAnimalRewardInfo _instance = null;
    
    private ManagerAdventure.AnimalInstance aData;
    int animalIdx;

    [SerializeField] private GenericReward reward;
    [SerializeField] private UILabel giftTimeText;
    [SerializeField] private GameObject rewardIcon;

    [Header("Linked Object")]
    [SerializeField] private UIItemAdventureAnimalInfo animalInfo;
    [SerializeField] private GameObject skillTitle;

    [SerializeField] GameObject infoRoot;
    [SerializeField] UIGrid starGrid;


    private void Awake()
    {
        if (_instance == null)
        {
            _instance = this;
        }
    }
    
   

    public void OnDestroy()
    {
        if (_instance == this)
            _instance = null;

        base.OnDestroy();
    }

    public void InitAnimalInfo(int animalIndex)
    {
        animalIdx = animalIndex;
        animalInfo.setAnimalCallBack += (ManagerAdventure.AnimalInstance aData) => skillTitle.SetActive(aData.skill != 0);
        animalInfo.setAnimalCallBack += (ManagerAdventure.AnimalInstance aData) => InitReward();
    }

    private IEnumerator Start()
    {
        bool waitFor = true;
        if (ManagerAdventure.Active == false)
        {
            infoRoot.SetActive(false);
            ManagerAdventure.OnInit((b) => { waitFor = false; infoRoot.SetActive(true); });
        }
            
        else
            waitFor = false;

        while (waitFor)
            yield return new WaitForSeconds(0.1f);

        aData = ManagerAdventure.User.GetAnimaDefault(animalIdx);
        animalInfo.SetAnimalSelect(aData);
        RewardSetActive();

        starGrid.enabled = true;
    }


    public override void OpenPopUp(int depth)
    {
        base.OpenPopUp(depth);

        UIPanel[] panels = GetComponentsInChildren<UIPanel>();
        for (int i = 1; i < panels.Length; i++)
        {
            panels[i].depth = depth + i;
        }
    }

    private void InitReward()
    {
        CdnAdventureAnimal cdnData = ServerContents.AdvAnimals[aData.idx];
        bool isExistReward = cdnData.lobby_rewards.Count > 0 ? true : false;

        if (isExistReward == true)
        {
            //보상 설정
            reward.SetReward(cdnData.lobby_rewards[0]);
            //보상 시간
            RewardTime();
        }
    }

    private void RewardTime()
    {
        long getRewardTs = 0;
        var animalData = ServerContents.AdvAnimals[aData.idx];

        if (animalData != null)
        {
            getRewardTs = animalData.lobby_reward_cool_time;
        }

        giftTimeText.text = Global.GetTimeText_HHMM(getRewardTs, false);
    }

    private void RewardSetActive()
    {
        GameObject rewardTrue = rewardIcon.transform.GetChild(0).gameObject;
        GameObject rewardFalse = rewardIcon.transform.GetChild(1).gameObject;
        GameObject rewardHomeButton = rewardIcon.transform.GetChild(2).gameObject;

        if (aData.lobbyCharIdx != -1)
        {
            //로비 캐릭터 o
            rewardTrue.SetActive(true);
            rewardFalse.SetActive(false);
            homeIconActive(rewardHomeButton, true);
        }
        else
        {
            //로비 캐릭터 x
            rewardTrue.SetActive(false);
            rewardFalse.SetActive(true);
            homeIconActive(rewardHomeButton, false);
        }
    }

    private float alphaColor = 125f / 255f;
    private void homeIconActive(GameObject rewardHomeButton, bool setActive)
    {
        GameObject homeIcon = rewardHomeButton.transform.GetChild(0).gameObject;

        if (setActive)
        {
            rewardHomeButton.GetComponent<BoxCollider>().enabled = true;
            homeIcon.GetComponent<TweenScale>().enabled = true;
            homeIcon.GetComponent<UISprite>().color = new Color(1f, 1f, 1f, 1f);
            if (ManagerCharacter._instance.IsSpecialLobbyChar(aData.idx)) // 스페셜 로비 배치 캐릭터 아이콘 예외 처리
            {
                homeIcon.GetComponent<UISprite>().spriteName = "icon_special_lobby";
                //기존의 아이콘은 크기를 조정하여 사용하기에 스페셜 로비 아이콘만 이미지 변경
                homeIcon.GetComponent<UISprite>().MakePixelPerfect();
            }
            else
                homeIcon.GetComponent<UISprite>().spriteName = "icon_home";
        }
        else
        {
            rewardHomeButton.GetComponent<BoxCollider>().enabled = false;
            homeIcon.GetComponent<TweenScale>().enabled = false;
            homeIcon.transform.localScale = Vector3.one;
            homeIcon.transform.GetComponent<UISprite>().color = new Color(1f, 1f, 1f, alphaColor);
        }
    }

    public void OnClickLobbyInfo()
    {
        var popup = ManagerUI._instance.OpenPopup<UIPopupLobbyGiftInfo>();
        popup.Init(aData.idx);
    }
}
