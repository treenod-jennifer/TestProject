using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using SideBanner;
using DG.Tweening;

public class UIItemSidebar : MonoBehaviour
{
    public static NewChecker newChecker = new NewChecker("BannerNewChecker");

    /// <summary>
    /// 스크롤 뷰 영역을 제외한 나머지 크기
    /// </summary>
    private const int DEFAULT_TITLE_HEIGHT = 120;

    /// <summary>
    /// 사이드바의 최대 높이
    /// </summary>
    private const int ROOT_MAX_HEIGHT = 1000;

    /// <summary>
    /// 사이드바가 최대 크기가 아닐때, 하단이 너무 많이 남았을 경우 줄이기 위한 수치
    /// </summary>
    private const int REMOVE_BOTTOM_SPACE = 15;

    /// <summary>
    /// 사이드바가 열리거나 닫히는 시간
    /// </summary>
    private const float OPEN_CLOSE_TIME = 0.25f;

    [Header("Setting")]
    [SerializeField] private int openPosX;
    [SerializeField] private int closePosX;
    [SerializeField] private UIButtonSidebar.IconState lobbyButtonSpriteName;

    [Header("Link Object")]
    [SerializeField] public UIWidget root;
    [SerializeField] private UISprite background;
    [SerializeField] private UIScrollView scrollView;
    [SerializeField] private BoxCollider dragBox;
    [SerializeField] private UIGrid grid;
    [SerializeField] private GameObject closeButton;

    private UILobbyButtonListManager lobbyButtonRoot;

    public List<Banner> banners = new List<Banner>();

    private UIButtonSidebar lobbyButton;

    private Coroutine moveCoroutine;
    // 사이드바에서 자체적으로 돌아가는 타이머 코루틴
    public Coroutine countCoroutine;

    private UILobbyButtonListManager LobbyButtonRoot
    {
        get
        {
            if (lobbyButtonRoot == null)
            {
                lobbyButtonRoot = transform.parent.GetComponent<UILobbyButtonListManager>();
            }

            return lobbyButtonRoot;
        }
    }

    private int RootHeight
    {
        get
        {
            return root.height;
        }
        set
        {
            int height;

            if (value < ROOT_MAX_HEIGHT)
            {
                height = value - REMOVE_BOTTOM_SPACE;
                IsScrollActive = false;
            }
            else
            {
                height = ROOT_MAX_HEIGHT;
                IsScrollActive = true;
            }

            root.height = height;

            Vector3 size = dragBox.size;
            size.y = height - DEFAULT_TITLE_HEIGHT;
            dragBox.size = size;

            Vector3 offset = dragBox.center;
            offset.y = (ROOT_MAX_HEIGHT - DEFAULT_TITLE_HEIGHT - dragBox.size.y) * 0.5f;
            dragBox.center = offset;
        }
    }

    private bool IsScrollActive
    {
        get
        {
            return scrollView.enabled;
        }
        set
        {
            scrollView.enabled = value;
        }
    }
    
    private List<BannerMaker> bannerMaker = new List<BannerMaker>();

    public class BannerMaker
    {
        public int    priority;
        public Action action;

        public BannerMaker(int _priority, Action _action)
        {
            priority = _priority;
            action   = _action;
        }
    }

    private void MoveRoot(int posX, System.Action endAction = null)
    {
        Vector3 endPos = root.transform.localPosition;
        endPos.x = posX;

        if(moveCoroutine != null)
        {
            StopCoroutine(moveCoroutine);
        }

        moveCoroutine = StartCoroutine(Move(root.transform, endPos, OPEN_CLOSE_TIME, endAction));

    }

    private IEnumerator Move(Transform target, Vector3 endPos, float time, System.Action endAction = null)
    {
        float totalTime = 0.0f;

        Vector3 startPos = target.localPosition;

        

        while (totalTime < time)
        {
            totalTime += Global.deltaTimeLobby;

            float t = Mathf.Clamp(totalTime / time, 0.0f, 1.0f);

            t = Mathf.Sin(t * Mathf.PI * 0.5f);

            target.localPosition = Vector3.Lerp(startPos, endPos, t);

            yield return null;
        }

        moveCoroutine = null;
        endAction?.Invoke();
    }

    private void ResetScroll()
    {
        Vector3 pos = scrollView.transform.localPosition;
        pos.y = 0.0f;
        scrollView.transform.localPosition = pos;

        Vector2 offset = scrollView.panel.clipOffset;
        offset.y = 0.0f;
        scrollView.panel.clipOffset = offset;
    }

    public void EventButtonSet()
    {
        if (lobbyButtonSpriteName == UIButtonSidebar.IconState.EVENT)
            SetButton();
        else if (lobbyButtonSpriteName == UIButtonSidebar.IconState.PACKAGE)
            SetButton();
    }

    private void OpenSystemPopUp()
    {
        UIPopupSystem popup = ManagerUI._instance.OpenPopupSystem(false).GetComponent<UIPopupSystem>();
        popup.InitSystemPopUp(Global._instance.GetString("p_t_4"), Global._instance.GetString("n_ev_20"), false);
        popup.SetResourceImage("Message/soon");
        popup.SortOrderSetting();
    }

    private void OpenGuidancePopUp()
    {
        ManagerUI._instance.OpenPopup<UIPopUpEventGuidance>((popup) => popup.InitPopUp());
    }

    public void ResetButtonEvent(UIButtonSidebar _btn)
    {
        if (_btn == null)
            _btn = lobbyButton;

        if (banners.Count == 0)
        {
            // 미션 카운트가 모자라 이벤트가 표시되지 않는 경우 클릭 이벤트로 미션 안내 팝업 표시
            if (GameData.User.missionCnt <= ManagerLobby._missionThreshold_eventstageOpen_noticeOpen_packageshopOpen)
                _btn.Init(lobbyButtonSpriteName, () => OpenGuidancePopUp());
            // 그냥 이 시기에 이벤트가 없는 경우 클릭 이벤트로 이벤트 안내 팝업 표시
            else
                _btn.Init(lobbyButtonSpriteName, () => OpenSystemPopUp());
        }
        else
            _btn.Init(lobbyButtonSpriteName, () => Open());
    }

    private void SetButton()
    {
        if (lobbyButton == null)
        {
            if (GameData.User == null)
                return;
            UIButtonSidebar buttonPrefab = Resources.Load<UIButtonSidebar>($"UIPrefab/UIButtonSidebar");
            UIButtonSidebar button = Instantiate(buttonPrefab, LobbyButtonRoot.transform);
            LobbyButtonRoot.AddLobbyButtonTop(button.gameObject);
            ResetButtonEvent(button);
            lobbyButton = button;
        }
        else
            ResetButtonEvent(lobbyButton);

        ButtonNewUpdate();
    }

    private void ButtonNewUpdate()
    {
        List<string> keys = new List<string>();
        bool isBubbleOn = false;
        bool isNewIconOn = false;

        foreach(var banner in banners)
        {
            keys.Add(banner.UniqueKey);

            if (banner is BannerRandomBoxADPackage)
                isBubbleOn = (banner as BannerRandomBoxADPackage).isCanShowAD;
            
            if(banner is BannerDiaStashEvent)
                if (ManagerDiaStash.CheckStartable() && ManagerDiaStash.instance.IsFullDia())
                    isNewIconOn = true;
        }

        lobbyButton.SetActiveNewIcon(newChecker.IsNew(keys) || isNewIconOn);
        lobbyButton.SetActiveBubbleIcon(isBubbleOn);
    }

    public Transform BannerRoot { get { return grid.transform; } }

    public void Open()
    {
        closeButton.SetActive(false);
        root.gameObject.SetActive(true);

        ResetScroll();
        grid.Reposition();

        MoveRoot(openPosX, () =>
        {
            closeButton.SetActive(true);
        });

        background.gameObject.SetActive(true);
        ManagerUI._instance.ScrollbarRight.SetDepth(-31);
        DOTween.ToAlpha(() => background.color, x => background.color = x, 190f / 255f, 0.15f);
        
        //배너 오픈
        if (ServerRepos.LoginCdn.packageSegmentVer > 0)
        {
            foreach (var banner in banners)
            {
                if (banner.IsPriorityPackage())
                {
                    banner.OnBannerOpen(true);
                    break;
                }
            }
        }
    }

    //연출을 통해서 패키지 배너 리스트를 닫습니다.
    public void Close()
    {
        ButtonNewUpdate();

        closeButton.SetActive(false);

        MoveRoot(closePosX, () =>
        {
            root.gameObject.SetActive(false);
        });

        background.gameObject.SetActive(true);
        ManagerUI._instance.ScrollbarRight.SetDepth();
        DOTween.ToAlpha(() => background.color, x => background.color = x, 0f, 0.15f);
    }

    //연출을 사용하지 않고 로비 이동 시 임의로 패키지 배너 리스트를 닫습니다.
    public void ResetClose()
    {
        background.color = Color.clear;
        Vector3 pos = root.transform.localPosition;
        pos.x                        = closePosX;
        root.transform.localPosition = pos;
        root.gameObject.SetActive(false);
    }

    public void AddBanner(Banner banner)
    {
        if (banner.GameObject.transform.parent != grid.transform)
        {
            banner.GameObject.transform.SetParent(grid.transform, false);
        }

        grid.Reposition();
        banners.Add(banner);

        RootHeight = DEFAULT_TITLE_HEIGHT + Mathf.RoundToInt(banners.Count * grid.cellHeight);
        SetButton();
    }

    #region 패키지 우선순위 정렬
    
    public class PackagePriorityComparer : IComparer<BannerMaker>
    {
        public int Compare(BannerMaker a, BannerMaker b)
        {
            if (a.priority < b.priority)
                return -1;
            else if (a.priority > b.priority)
                return 1;
            else
                return 0;
        }
    }
    
    /// <summary>
    /// 패키지 타입이 우선 순위가 있다면 우선 순위와 함께 액션을 저장합니다.
    /// </summary>
    /// <param name="type"> 패키지 타입을 입력받습니다. 광고의 경우 -1, 그 외 순위 미지정의 경우 100을 사용</param>
    public void AddBannerMaker(int type, Action action)
    {
        int priority = type;
        if (ServerRepos.UserPackageSegment != null && ServerRepos.UserPackageSegment.packagePriority != null && ServerRepos.UserPackageSegment.packagePriority.Count > 0)
        {
            var packageConfig = ServerRepos.UserPackageSegment.packagePriority.Find(x => x.type == type);
            if (packageConfig != null)
            {
                priority = packageConfig.idx;
            }
            else if(priority != -1)
            {
                priority = 100;
            }
        }

        //우선 순위에 포함되지 않거나 -1 인경우 임의로 설정한 값을 우선순위로 함
        bannerMaker.Add(new BannerMaker(priority, action));
    }

    /// <summary>
    /// 우선순위에 따라 정렬 및 호출
    /// </summary>
    public void AddBannerToSidebar()
    {
        bannerMaker.Sort(new PackagePriorityComparer());
        
        foreach (var _banner in bannerMaker)
        {
            _banner.action();
        }
        
        if(scrollView.panel != null)
            ResetScroll();
        grid.Reposition();
    }
    
    #endregion

    public void DestroyAllBanner()
    {
        foreach(var banner in banners)
        {
            Destroy(banner.GameObject);
        }
        grid.transform.DestroyChildren();

        banners.Clear();
        bannerMaker.Clear();

        if(lobbyButton != null)
        {
            lobbyButton.DestroyButton();
            lobbyButton = null;
        }
    }

    public IEnumerator CheckBannerTimer(long expiredTime, bool isBubbleOn = false)
    {
        while (Global.LeftTime(expiredTime) > 0)
        {
            if (gameObject == null || lobbyButton == null)
                break;
            yield return new WaitForSeconds(1f);
        }

        if (lobbyButton != null)
        {
            lobbyButton.SetActiveNewIcon(true);
            lobbyButton.SetActiveBubbleIcon(isBubbleOn);
        }
        countCoroutine = null;
        yield return null;
    }
}