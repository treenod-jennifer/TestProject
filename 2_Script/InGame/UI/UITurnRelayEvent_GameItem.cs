using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UITurnRelayEvent_GameItem : MonoBehaviour
{
    public ManagerTurnRelay.BONUSITEM_TYPE itemType;

    [SerializeField] private UISprite spriteItem;
    [SerializeField] private UILabel labelItemCount;
    [SerializeField] private BoxCollider itemCollider;

    private BlockBombType bombType = BlockBombType.NONE;
    private Transform _transform;

    public void InitGameItem()
    {
        _transform = this.transform;
        bombType = ManagerTurnRelay.turnRelayIngame.GetItemBombType(itemType);

        //카운트 초기화.
        RefreshGameItemCount();
    }

    public void RefreshGameItemCount()
    {
        //아이템 카운트 갱신
        int itemCount = ManagerTurnRelay.turnRelayIngame.GetData_DicIngameItemCount(itemType);

        //아이템 카운트에 따라 활성화 상태 설정
        bool isActiveItem = (itemCount > 0);

        if (isActiveItem == false)
        {   //아이템 비활성화 설정
            itemCollider.enabled = false;
            labelItemCount.gameObject.SetActive(false);
            spriteItem.color = new Color(0.5f, 0.5f, 0.5f);
        }
        else
        {   //아이템 활성화 설정
            if (itemCount == 1)
            {
                itemCollider.enabled = true;
                spriteItem.color = new Color(1f, 1f, 1f);
                labelItemCount.gameObject.SetActive(true);
            }

            labelItemCount.text = string.Format("+{0}", itemCount);
        }
    }

    public IEnumerator CoAction_AppearGameItem()
    {
        float actionTime = ManagerBlock.instance.GetIngameTime(0.2f);

        //스케일, 알파 연출
        spriteItem.transform.DOScale(Vector3.one * 0.3f, actionTime);
        DOTween.ToAlpha(() => spriteItem.color, x => spriteItem.color = x, 0f, actionTime);
        yield return new WaitForSeconds(ManagerBlock.instance.GetIngameTime(0.2f));

        //카운트 및 상태 변화
        RefreshGameItemCount();

        spriteItem.transform.DOScale(Vector3.one, actionTime).SetEase(Ease.OutBack);        
        DOTween.ToAlpha(() => spriteItem.color, x => spriteItem.color = x, 1f, actionTime).SetEase(Ease.OutBack);

        //폭탄 생성 이펙트
        InGameEffectMaker.instance.MakeBombMakeEffect(spriteItem.transform.position);

        //사운드 출력
        ManagerSound.AudioPlay(AudioInGame.GET_CANDY);

        yield return new WaitForSeconds(ManagerBlock.instance.GetIngameTime(0.2f));
        
        _transform.localScale = Vector3.one;
        spriteItem.alpha = 1;
    }

    private void OnClickBtnUseItem()
    {
        if (GameManager.instance.state != GameState.PLAY)
            return;

        if (GameManager.instance.moveCount == 0)
            return;

        if (ManagerBlock.instance.state != BlockManagrState.WAIT)
            return;

        if (!ManagerBlock.instance.checkBlockWait())
            return;

        if (GameItemManager.instance != null)
            return;

        //폭탄이 생성될 위치의 블럭을 가져옴
        BlockBase targetBlock = PosHelper.GetRandomBlock();

        //폭탄이 생성될 수 없다면 반환.
        if (targetBlock == null)
            return;

        BlockBombType makeBombType = bombType;
        if (makeBombType == BlockBombType.LINE)
        {
            int randomLine = Random.Range(0, 2);
            makeBombType = (randomLine == 0) ? BlockBombType.LINE_V : BlockBombType.LINE_H;
        }

        //소지한 인게임 아이템 카운트 감소
        ManagerTurnRelay.turnRelayIngame.SetData_DicIngameItemGainCount(itemType, -1);

        //폭탄 생성
        ManagerBlock.instance.MakeBombRandom(targetBlock, makeBombType);

        //아이템 사용 후, 카운트 리프레시
        RefreshGameItemCount();
    }
}
