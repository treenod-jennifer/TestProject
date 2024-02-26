using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using DG.Tweening;

public class UIButtonAdventure: UIButtonEventBase
{
    public static UIButtonAdventure _instance;

    [SerializeField]
    GameObject rootObj;

    [SerializeField] GameObject eventObj;
    [SerializeField] GameObject saleObj;
    [SerializeField] UIUrlTexture collaboTexture;

    bool eventActivated = false;
    private bool needInit = false;

    private void Awake()
    {
        _instance = this;
    }

    private void OnDestroy()
    {
        if (_instance == this)
            _instance = null;
    }

    private void OnEnable()
    {
        if (needInit == true)
        {   //초기화가 필요한 시점에만 초기화를 호출
            SetButtonEvent(0);
        }
        else
        {
            needInit = true;
        }
    }

    /// <summary>
    /// 탐험모드 아이콘을 강제로 활성/비활성 할 때 사용
    /// </summary>
    public void SetEnable(bool enabled)
    {
        if (enabled == true)
            needInit = false;
        
        this.gameObject.SetActive(enabled);
    }

    private const int SELF_EVENT = 100000;
    public override void SetButtonEvent(int version)
    {
        //base.SetButtonEvent(version);
        //string fileName = "adventure_icon";

        //UIImageLoader.Instance.Load(Global.adventureDirectory, "Animal/", fileName, this);
        ManagerAdventure.User.SyncFromServer_Stage();

        bool bSale = ServerRepos.LoginCdn.RenewalAdvInGameItemSale == 1 ||
                     (ServerContents.WingShop.Normal != null &&
                      ServerContents.WingShop.Normal.Count(x => x.saleLevel > 0) > 0) ||
                     ServerRepos.LoginCdn.AdvContinueSale != 0;

        bool bEvent =   ManagerAdventure.GetActiveEvent_ExpUpMagnification() > 1 ||
                        ManagerAdventure.GetActiveEvent_GachaRateUp() ||
                        ManagerAdventure.GetActiveEvent_MileageGachaEvent() ||
                        ManagerAdventure.EventData.GetActiveEvent_AdventureEvent();

        int collaboIndex = ManagerAdventure.GetActiveCollabo();
        if (SetCollabo(collaboIndex) == false)
        {
            collaboIndex = 0;
        }

        if (collaboIndex > 0 && collaboIndex <= SELF_EVENT)  //외부 콜라보
        {
            eventObj.SetActive(true);
            saleObj.SetActive(false);
        }
        else if (collaboIndex > SELF_EVENT)  //자체 이벤트
        {
            eventObj.SetActive(true);
            saleObj.SetActive(false);
        }
        else if (bEvent == true)
        {
            eventObj.SetActive(true);
            saleObj.SetActive(false);
        }
        else if (bSale == true)
        {
            eventObj.SetActive(false);
            saleObj.SetActive(true);
        }
        else if (ManagerAdventure.GetActiveGachaSale())
        {
            eventObj.SetActive(false);
            saleObj.SetActive(true);
            
            var chapter = ManagerAdventure.User.GetChapterProgress(1);

            if (chapter == null || chapter.stageProgress[1] == null || chapter.stageProgress[1].clearLevel == 0)
            {
                saleObj.SetActive(false);
            }
        }
        else
        {
            eventObj.SetActive(false);
            saleObj.SetActive(false);
        }

        

        eventActivated = (collaboIndex != 0) || bEvent || bSale;

        if (eventActivated)
            rootObj.transform.DOLocalMoveY(-15f, 0.3f);
        else
            rootObj.transform.DOLocalMoveY(0f, 0.3f);

    }

    protected override void OnClickBtnEvent()
    {
        if (ManagerUI.IsLobbyButtonActive)
        {
            ManagerUI._instance.OpenPopupStageAdventure();
        }
    }

    public override float GetButtonSize()
    {
        if(eventActivated)
            return 200f;
        return 160f;
    }

    private bool SetCollabo(int collaboIndex)
    {
        if (collaboIndex != 0)
        {
            if( HashChecker.GetHash("Animal/", "collabo_lobby_" + collaboIndex + ".png") == null)
            {
                return false;
            }
            collaboTexture.SuccessEvent += () => collaboTexture.MakePixelPerfect();
            collaboTexture.LoadCDN(Global.adventureDirectory, "Animal/", "collabo_lobby_" + collaboIndex);
            //collaboTexture.Load(Global.adventureDirectory, "Animal/", "at_" + collaboIndex + "001");//test
            collaboTexture.gameObject.SetActive(true);
            return true;
        }
        else
        {
            collaboTexture.gameObject.SetActive(false);
            return false;
        }
    }
}
