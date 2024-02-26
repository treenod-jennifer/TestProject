using DG.Tweening;
using System.Collections;
using UnityEngine;

public class UISpaceTravelEventGameItem : MonoBehaviour
{
    public ManagerSpaceTravel.BonusItemType _itemType;

    [SerializeField] private UISprite _spriteItem;
    [SerializeField] private UISprite _spriteBg;
    [SerializeField] private UILabel _labelItemCount;
    [SerializeField] private BoxCollider _itemCollider;

    private BlockBombType _bombType = BlockBombType.NONE;

    public void InitGameItem()
    {
        _spriteBg.atlas = ManagerSpaceTravel.instance._spaceTravelPackIngame.Atlas;
        _bombType = _itemType switch
        {
            ManagerSpaceTravel.BonusItemType.LINE_BOMB => BlockBombType.LINE,
            ManagerSpaceTravel.BonusItemType.CIRCLE_BOMB => BlockBombType.BOMB,
            ManagerSpaceTravel.BonusItemType.RAINBOW_BOMB => BlockBombType.RAINBOW,
            _ => BlockBombType.NONE,
        };

        //카운트 초기화.
        RefreshGameItemCount();
    }

    private void RefreshGameItemCount()
    {
        //아이템 카운트 갱신
        var itemCount = ManagerSpaceTravel.instance.selectItemDic.ContainsKey(_itemType) ? ManagerSpaceTravel.instance.selectItemDic[_itemType] : 0;

        //아이템 카운트에 따라 활성화 상태 설정
        var isActiveItem = (itemCount > 0);

        if (isActiveItem == false)
        {   //아이템 비활성화 설정
            _itemCollider.enabled = false;
            _labelItemCount.gameObject.SetActive(false);
            _spriteItem.color = new Color(0.5f, 0.5f, 0.5f);
        }
        else
        {   //아이템 활성화 설정
            if (itemCount == 1)
            {
                _itemCollider.enabled = true;
                _spriteItem.color = new Color(1f, 1f, 1f);
                _labelItemCount.gameObject.SetActive(true);
            }

            _labelItemCount.text = string.Format("+{0}", itemCount);
        }
    }

    public IEnumerator CoAction_AppearGameItem()
    {
        var actionTime = ManagerBlock.instance.GetIngameTime(0.2f);

        //스케일, 알파 연출
        _spriteItem.transform.DOScale(Vector3.one * 0.3f, actionTime);
        DOTween.ToAlpha(() => _spriteItem.color, x => _spriteItem.color = x, 0f, actionTime);
        yield return new WaitForSeconds(ManagerBlock.instance.GetIngameTime(0.2f));

        //카운트 및 상태 변화
        RefreshGameItemCount();

        _spriteItem.transform.DOScale(Vector3.one, actionTime).SetEase(Ease.OutBack);        
        DOTween.ToAlpha(() => _spriteItem.color, x => _spriteItem.color = x, 1f, actionTime).SetEase(Ease.OutBack);

        //폭탄 생성 이펙트
        InGameEffectMaker.instance.MakeBombMakeEffect(_spriteItem.transform.position);

        //사운드 출력
        ManagerSound.AudioPlay(AudioInGame.GET_CANDY);

        yield return new WaitForSeconds(ManagerBlock.instance.GetIngameTime(0.2f));
        
        transform.localScale = Vector3.one;
        _spriteItem.alpha = 1;
    }

    private void OnClickBtnUseItem()
    {
        if (GameManager.instance.state     != GameState.PLAY        ||
            GameManager.instance.moveCount == 0                     ||
            ManagerBlock.instance.state    != BlockManagrState.WAIT ||
            !ManagerBlock.instance.checkBlockWait()                 ||
            GameItemManager.instance != null)
        {
            return;
        }

        //폭탄이 생성될 위치의 블럭을 가져옴
        var targetBlock = PosHelper.GetRandomBlock();

        //폭탄이 생성될 수 없다면 반환.
        if (targetBlock == null)
        {
            return;
        }

        var makeBombType = _bombType;
        if (makeBombType == BlockBombType.LINE)
        {
            var randomLine = Random.Range(0, 2);
            makeBombType = (randomLine == 0) ? BlockBombType.LINE_V : BlockBombType.LINE_H;
        }
        
        //소지한 인게임 아이템 카운트 감소
        ManagerSpaceTravel.instance.AddSelectItemDictionary(_itemType, -1);
        ManagerSpaceTravel.instance._useBonusItem = true;
        
        // 아이템 사용 관련 그로시 로그 전송
        var achieve = new ServiceSDK.GrowthyCustomLog_Achievement
        (
            ServiceSDK.GrowthyCustomLog_Achievement.Code_L_TAG.EVENT_MODE,
            ServiceSDK.GrowthyCustomLog_Achievement.Code_L_CAT.SPACE_TRAVEL_USE,
            _itemType.ToString(),
            ServiceSDK.GrowthyCustomLog_Achievement.Code_L_ARLT.SUCCESS,
            ManagerSpaceTravel.instance.EventIndex, 
            Global.stageIndex.ToString()
        );
        var d = Newtonsoft.Json.JsonConvert.SerializeObject(achieve);
        ServiceSDK.ServiceSDKManager.instance.InsertGrowthyCustomLog("ACHIEVEMENT", d);

        //폭탄 생성
        ManagerBlock.instance.MakeBombRandom(targetBlock, makeBombType);

        //아이템 사용 후, 카운트 리프레시
        RefreshGameItemCount();
    }
}
