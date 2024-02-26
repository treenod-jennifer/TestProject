using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class UIItemGuest
{
    #region 선물
    public GenericReward reward;
    public UILabel giftTimeText;
    public UISprite gifeBg;
    public GameObject giftTimeRoot;
    #endregion

    #region 캐릭터
    public UIUrlTexture animalPicture;
    public GameObject specialFrame;
    #endregion

    #region 버튼
    public GameObject objBtn;
    public UISprite spriteBtn;
    public UILabel textBtn;
    #endregion
}

public class UIItemLobbyGuest : MonoBehaviour
{
    private enum LobbyGuestSelectState
    {
        NONE,   //기본
        NOTSELECTABLE,  //선택 불가(캐릭터 수 부족)
        SELECT,     //선택 중
        INACTIVE,   //다른 캐릭터가 선택된 상태
    }

    public UIItemGuest itemGuest;
    public BoxCollider[] colliderBtn;   // [0]은 버튼, [1]은 캐릭터 누를수 있는 영역

    public UITexture textureNoCharacter;

    #region 캐릭터 선택
    public GameObject selectBox;
    public GameObject clipBox;
    #endregion
    
    #region 콜백
    //캐릭터 변경 선택 콜백
    public System.Action<int> selectCharacterAction = null;
    //캐릭터 변경 취소 콜백
    public System.Action selectCancelAction = null;
    #endregion

    private Color btnTextEffectColor_1 = new Color(68f / 255f, 144f / 255f, 41f / 255f);
    private Color btnTextEffectColor_2 = new Color(118f / 255f, 118f / 255f, 118f / 255f);
    private bool isClickChangeButton = false;

    private float alphaColor = 125f / 255f;
    private int listIdx = 0;
    private int charIdx = 0;
    private LobbyGuestSelectState state = LobbyGuestSelectState.NONE;

    private Coroutine timeRoutine = null;

    private LobbyGuestSelectState itemState
    {
        set
        {
            if (state == value)
                return;

            state = value;
            switch (state)
            {
                case LobbyGuestSelectState.NONE:
                    selectBox.SetActive(false);
                    clipBox.SetActive(false);
                    colliderBtn.SetEnable(true);
                    break;
                case LobbyGuestSelectState.NOTSELECTABLE:
                    selectBox.SetActive(false);
                    clipBox.SetActive(false);
                    colliderBtn.SetEnable(false);
                    break;
                case LobbyGuestSelectState.SELECT:
                    selectBox.SetActive(true);
                    clipBox.SetActive(false);
                    colliderBtn.SetEnable(true);
                    break;
                case LobbyGuestSelectState.INACTIVE:
                    selectBox.SetActive(false);
                    clipBox.SetActive(true);
                    colliderBtn.SetEnable(false);
                    break;
            }
        }
    }

    public void InitDefaultItem(int listIdx)
    {
        this.listIdx = listIdx;
        textureNoCharacter.gameObject.SetActive(true);
        itemGuest.animalPicture.gameObject.SetActive(false);
        InitRewardUI(false);
        SetItemState_NotSelectable();
        SetNotSelectableButton();
    }

    public void InitItem(int listIdx, int characterIdx, bool isCanChange)
    {
        this.listIdx = listIdx;
        textureNoCharacter.gameObject.SetActive(false);
        UpdateItem(characterIdx, isCanChange);
    }

    public void UpdateItem(int characterIdx, bool isCanChange)
    {
        charIdx = characterIdx;
        int coustumeIdx = ManagerCharacter._instance.IsSpecialLobbyChar(characterIdx) ? ManagerAdventure.User.GetAnimalInstance(characterIdx).lookId : 0;
        string fileName = ManagerAdventure.GetAnimalTextureFilename(charIdx, coustumeIdx);
        itemGuest.animalPicture.SettingTextureScale(166, 166);
        itemGuest.animalPicture.LoadCDN(Global.adventureDirectory, "Animal/", fileName);
        InitReward();
        if (isCanChange == true)
        {
            SetItemState_Normal();
            SetChangeButton();
        }
        else
        {
            SetItemState_NotSelectable();
            SetNotSelectableButton();
        }
    }

    private void OnClickBtnChange()
    {
        if (isClickChangeButton == false)
        {
            SetItemState_Select();
            SetSelectedButton();
            if (selectCharacterAction != null)
                selectCharacterAction(listIdx);
        }
        else
        {
            SetItemState_Normal();
            SetChangeButton();
            if (selectCancelAction != null)
                selectCancelAction();
        }
        isClickChangeButton = !isClickChangeButton;
    }

    #region 보상관련
    private void InitReward()
    {
        InitCoroutine();
        CdnAdventureAnimal aData = ServerContents.AdvAnimals[charIdx];
        bool isExistReward = aData.lobby_rewards.Count > 0 ? true : false;
        InitRewardUI(isExistReward);
        if (isExistReward == true)
        {
            InitRewardIcon(aData.lobby_rewards[0]);
        }
    }

    private void InitRewardUI(bool isExistReward)
    {
        itemGuest.reward.gameObject.SetActive(isExistReward);
        itemGuest.giftTimeRoot.SetActive(isExistReward);
        if (isExistReward == true)
        {
            itemGuest.gifeBg.color = new Color(1f, 1f, 1f, 1f);
        }
        else
        {
            itemGuest.gifeBg.color = new Color(1f, 1f, 1f, alphaColor);
        }
    }

    #endregion

    private void InitCoroutine()
    {
        if (timeRoutine != null)
        {
            StopCoroutine(timeRoutine);
            timeRoutine = null;
        }
    }

    private void InitRewardIcon(Reward lobby_rewards)
    {
        //보상 설정
        itemGuest.reward.SetReward(lobby_rewards);
        //보상 시간
        timeRoutine = StartCoroutine(CoRewardTimer());
    }

    private IEnumerator CoRewardTimer()
    {
        long getRewardTs = 0;
        var userAnimal = ServerRepos.UserAdventureLobbyAnimals.Find(x => x.animalId == charIdx);
        if (userAnimal != null)
        {
            getRewardTs = userAnimal.get_reward_ts;
        }

        float leftTime = Global.LeftTime(getRewardTs);
        while (true)
        {
            if (leftTime <= 0)
                break;

            leftTime = Global.LeftTime(getRewardTs);
            itemGuest.giftTimeText.text = Global.GetTimeText_HHMMSS(getRewardTs);
            yield return new WaitForSeconds(0.2f);
        }
        itemGuest.giftTimeText.text = "00:00:00";
        timeRoutine = null;
    }

    public void SetItemState_Normal()
    {
        itemState = LobbyGuestSelectState.NONE;
    }

    public void SetItemState_NotSelectable()
    {
        itemState = LobbyGuestSelectState.NOTSELECTABLE;
    }

    public void SetItemState_Select()
    {
        itemState = LobbyGuestSelectState.SELECT;
    }

    public void SetItemState_Inactive()
    {
        itemState = LobbyGuestSelectState.INACTIVE;
    }

    #region 버튼
    public void SetChangeButton()
    {
        itemGuest.textBtn.text = Global._instance.GetString("btn_34");
        itemGuest.textBtn.effectColor = btnTextEffectColor_1;
        itemGuest.spriteBtn.spriteName = "diary_button_green";
        itemGuest.spriteBtn.width = 80;
        colliderBtn[0].size = new Vector3(100, colliderBtn[0].size.y, colliderBtn[0].size.z);
    }

    private void SetSelectedButton()
    {
        itemGuest.textBtn.text = Global._instance.GetString("btn_35");
        itemGuest.textBtn.effectColor = btnTextEffectColor_2;
        itemGuest.spriteBtn.spriteName = "diary_button_gray";
        itemGuest.spriteBtn.width = 122;
        colliderBtn[0].size = new Vector3(122, colliderBtn[0].size.y, colliderBtn[0].size.z);
    }

    private void SetNotSelectableButton()
    {
        itemGuest.textBtn.text = Global._instance.GetString("btn_34");
        itemGuest.textBtn.effectColor = btnTextEffectColor_2;
        itemGuest.spriteBtn.spriteName = "diary_button_gray";
        itemGuest.spriteBtn.width = 80;
        colliderBtn[0].size = new Vector3(100, colliderBtn[0].size.y, colliderBtn[0].size.z);
    }
    #endregion
}
