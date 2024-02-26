using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UIItemBingoEventBoard : MonoBehaviour
{
    [SerializeField] private GameObject slotClose;
    [SerializeField] private GameObject slotStage;
    [SerializeField] private GameObject slotBonus;
    [SerializeField] private GameObject objSpecialBonus;
    [SerializeField] private GameObject slotClear;

    [SerializeField] private Animation aniSelectSlot;
    
    //빙고 연출 이펙트
    [SerializeField] private GameObject select_effect;
    [SerializeField] private GameObject clear_effect;

    [SerializeField] private List<UISprite> spriteList;
    public enum SlotState
    {
        CLOSE,
        OPEN_STAGE,
        OPEN_BONUS,
        OPEN_SPECIAL_BONUS,
        CLEAR,
    }

    private SlotState _slotState = SlotState.CLOSE;

    private int slotIndex;
    
    public void InitDate(int slotIndex)
    {
        this.slotIndex = slotIndex;
    }

    public IEnumerator CoSetSlot(System.Action endAction)
    {   
        if (ManagerBingoEvent.instance.isLineClear || ManagerBingoEvent.instance.isSlotClear)
        {
            SetSlot();
            StartCoroutine(CoSetClear());
            yield return new WaitForSeconds(1f);
            yield return CoSlot();
        }
        else
        {
            yield return CoSlot();
        }
        endAction?.Invoke();
    }

    private void SetSlot()
    {
        if (ManagerBingoEvent.instance.IsSlotClear(slotIndex))
        {
            SetButtonFunction(SlotState.CLEAR);
            return;
        }
        
        SetButtonFunction(SlotState.CLOSE);

        if (ManagerBingoEvent.instance.IsSelectSlot(slotIndex))
        {
            SetButtonFunction(ManagerBingoEvent.instance.GetSlotSetting(slotIndex));
        }
        else
        {
            SetButtonFunction(SlotState.CLOSE);
        }
    }

    public static System.Action LineClearEvent;
    
    public IEnumerator CoSetClear()
    {
        if (ManagerBingoEvent.instance.IsClearSlot(slotIndex) && (ManagerBingoEvent.instance.isLineClear || ManagerBingoEvent.instance.isSlotClear))
        {
            aniSelectSlot.Play("complete_slot");

            yield return new WaitWhile(() => aniSelectSlot.IsPlaying("complete_slot"));
            
            // 라인 클리어 직후 SyncFromServerUserData 실행 전 팝업이 초기화 되며 라인 보상 관련 데이터가 팝업에 업데이트 되지 않는 이슈 수정을 위해 PostLineClear 실행
            if (ManagerBingoEvent.instance.isLineClear)
                PostLineClear();

            ManagerBingoEvent.instance.isLineClear = false;
            ManagerBingoEvent.instance.isSlotClear = false;
            
            ManagerBingoEvent.instance.SyncFromServerUserData();
            ManagerBingoEvent.instance.tempClearSlotIndex.Clear();

            LineClearEvent?.Invoke();
        }
    }

    private void PostLineClear()
    {
        LineClearEvent += () =>
        {
            if (UIPopupBingoEvent_Board._instance != null)
                UIPopupBingoEvent_Board._instance.SetBonusStageClearUpdate();
        };
    }

    public IEnumerator CoSlot()
    {
        if (ManagerBingoEvent.instance.IsSlotClear(slotIndex))
        {
            SetButtonFunction(SlotState.CLEAR);
            yield break;
        }
        
        SetButtonFunction(SlotState.CLOSE);

        if (ManagerBingoEvent.instance.IsSelectSlot(slotIndex))
        {
            if (ManagerBingoEvent.instance.isSlotOpen)
            {
                yield return new WaitForSeconds(0.5f);
            
                aniSelectSlot.Play("select_slot");

                yield return new WaitWhile(() => aniSelectSlot.IsPlaying("select_slot"));

                ManagerBingoEvent.instance.isSlotOpen = false;
                
                yield return new WaitForSeconds(0.5f);
            }
            else
            {
                SetButtonFunction(ManagerBingoEvent.instance.GetSlotSetting(slotIndex));
            }
        }
        else
        {
            SetButtonFunction(SlotState.CLOSE);
        }
    }

    public void SetButtonFunction(UIItemBingoEventBoard.SlotState state)
    {

        _slotState = state;
        
        switch (state)
        {
            case SlotState.CLOSE:
                {
                    slotClose.SetActive(true);
                    slotBonus.SetActive(false);
                    slotStage.SetActive(false);
                    slotClear.SetActive(false);
                }
                break;
            case SlotState.OPEN_STAGE:
                {
                    slotClose.SetActive(false);
                    slotBonus.SetActive(false);
                    slotStage.SetActive(true);
                    slotClear.SetActive(false);
                }
                break;
            case SlotState.OPEN_BONUS:
                {
                    slotClose.SetActive(false);
                    slotBonus.SetActive(true);
                    objSpecialBonus.SetActive(false);
                    slotStage.SetActive(false);
                    slotClear.SetActive(false);
                }
                break;
            case SlotState.OPEN_SPECIAL_BONUS:
                {
                    slotClose.SetActive(false);
                    slotBonus.SetActive(true);
                    objSpecialBonus.SetActive(true);
                    slotStage.SetActive(false);
                    slotClear.SetActive(false);
                }
                break;
            case SlotState.CLEAR:
                {
                    slotClose.SetActive(false);
                    slotBonus.SetActive(false);
                    slotStage.SetActive(false);
                    slotClear.SetActive(true);
                }
                break;
        }
    }

    void OnClick()
    {
        if (UIPopupBingoEvent_Board._instance.IsCanPopupTouch() == false) return;
        
        switch (_slotState)
        {
            case SlotState.OPEN_STAGE:
                {
                    ManagerUI._instance.OpenPopupReadyBingoEvent(ManagerBingoEvent.instance.GetSlotState(slotIndex));
                }
                break;
            case SlotState.OPEN_BONUS:
                {
                    ManagerUI._instance.OpenPopup<UIPopUpBingoEvent_Bonus>((popup) => popup.InitData(slotIndex, false));
                }
                break;
            case SlotState.OPEN_SPECIAL_BONUS:
                {
                    ManagerUI._instance.OpenPopup<UIPopUpBingoEvent_Bonus>((popup) => popup.InitData(slotIndex, true));
                }
                break;
            default:
                break;
        }
    }

    public void selectEffect()
    {
        var obj = NGUITools.AddChild(this.gameObject, select_effect);
        obj.transform.localRotation = Quaternion.Euler(90f, 0f, 0f);
        obj.transform.localScale = new Vector3(0.1f, 0.1f, 0.1f);
    }

    public IEnumerator clearEffect()
    {
        yield return new WaitForSeconds(0.3f);
        var obj = NGUITools.AddChild(this.gameObject, clear_effect);
        obj.transform.localRotation = Quaternion.Euler(90f, 0f, 0f);
        obj.transform.localScale = new Vector3(0.1f, 0.1f, 0.1f);
    }

    public void changeSlot()
    {
        SetButtonFunction(ManagerBingoEvent.instance.GetSlotSetting(slotIndex));
    }

    public IEnumerator changeClearSlot()
    {
        yield return new WaitForSeconds(0.5f);
        SetButtonFunction(SlotState.CLEAR);
    }
    
    public void SetAtlas()
    {
        for (int i = 0; i < spriteList.Count; i++)
        {
            spriteList[i].atlas = ManagerBingoEvent.bingoEventResource.bingoEventPack.AtlasOutgame;
        }
    }
}