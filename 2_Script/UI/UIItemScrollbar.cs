using UnityEngine;
using SideIcon;
using System.Collections.Generic;
using System.Collections;

public class UIItemScrollbar : MonoBehaviour
{
    public static NewChecker newChecker = new NewChecker("IconNewChecker");
    public static float EVENT_VIEW_COUNT = 5.82f;
    public static float SCREEN_CENTER_COUNT = 4;    // 화면 세로 중앙에 몇 번째 아이콘이 오는지 정의 (로비 개편 튜토리얼에서 사용)

    public List<Icon> icons = new List<Icon>();
    public List<GameObject> tempicons = new List<GameObject>();

    public UIGrid grid;
    [SerializeField] public UIScrollView scrollView;
    [SerializeField] private GameObject root;
    [SerializeField] private GameObject upArrow;
    [SerializeField] private GameObject downArrow;
    [SerializeField] private GameObject dragScrollView;
    [SerializeField] private ScrollBarType scrollBarType;
    [SerializeField] private GameObject newIconBubble;
    [SerializeField] private UIIconBubble bubble;
    
    //스크롤 뉴 아이콘 관련
    private const float NEW_ICON_HEIGHT    = 35;
    private const float BUBBLE_HEIGHT      = 92;
    private const float ICON_HEIGHT        = 135;
    private const float SCROLL_VIEW_HEIGHT = 810;

    public Transform IconRoot { get { return grid.transform; } }

    private bool isUpArrowOff = true;
    private bool isDownArrowOff = false;

    private Coroutine checkCoroutine = null;

    private int iconCount
    {
        get { return icons.Count + tempicons.Count; }
    }

    public enum ScrollBarType
    {
        EVENT,
        PACKAGE,
    }

    private void OnEnable()
    {
        SetArrowCoroutine();
    }

    private void OnDisable()
    {
        if (checkCoroutine != null)
        {
            StopCoroutine(checkCoroutine);
            checkCoroutine = null;
        }
    }

    public void Open()
    {
        if (scrollBarType == ScrollBarType.EVENT)
            SetScroll_Event();
        IconNewUpdate();
    }

    private void SetArrowCoroutine()
    {
        newIconBubble.SetActive(false);
        bubble.gameObject.SetActive(false);
        
        if (scrollBarType == ScrollBarType.EVENT && iconCount > EVENT_VIEW_COUNT)
        {        
            // 마지막 생성된 아이콘 기준으로 스크롤이 세팅되도록 (화살표 표시되는 y 포지션 계산 위함)
            if (checkCoroutine != null)
            {
                StopCoroutine(checkCoroutine);
                checkCoroutine = null;
            }
            if (gameObject.activeInHierarchy)
                checkCoroutine = StartCoroutine(CoCheckArrowActive(EVENT_VIEW_COUNT));

            ArrowSetActive();
        }
        else if (iconCount <= EVENT_VIEW_COUNT)
        {
            isUpArrowOff = true;
            isDownArrowOff = true;
            ArrowSetActive();
        }
    }

    /// <summary>
    /// 이벤트 스크롤 세팅 (아이콘 뷰 3개 반)
    /// </summary>
    private void SetScroll_Event()
    {
        if (iconCount > EVENT_VIEW_COUNT)
        {
            SetScroll(EVENT_VIEW_COUNT);
        }
        else if (iconCount > 0)
        {
            SetScrollFix();
        }
        else
        {
            root.SetActive(false);
        }
    }

    /// <summary>
    /// 현재 오픈된 아이콘이 네개 이상일 때 스크롤 세팅
    /// </summary>
    private void SetScroll(float viewCount)
    {
        root.SetActive(true);
        dragScrollView.SetActive(true);
        scrollView.enabled = true;
        SetDepth();
        
        SetScrollPositionY(0f);
        ArrowSetActive();
    }

    /// <summary>
    /// 현재 오픈된 아이콘이 세개 이하일 때 스크롤 고정
    /// </summary>
    private void SetScrollFix()
    {
        root.SetActive(true);
        dragScrollView.SetActive(false);
        scrollView.enabled = false;
        SetDepth();

        SetScrollPositionY(0f);

        isUpArrowOff = true;
        isDownArrowOff = true;
        ArrowSetActive();
    }

    /// <summary>
    /// 스크롤 세로 위치 세팅
    /// </summary>
    private void SetScrollPositionY(float y = 0f)
    {
        Vector3 pos = scrollView.transform.localPosition;
        pos.y = y;
        scrollView.transform.localPosition = pos;

        UIPanel panel = scrollView.GetComponent<UIPanel>();
        if (panel != null)
        {
            Vector2 offset = panel.clipOffset;
            offset.y = y;
            panel.clipOffset = offset;
        }

        grid.Reposition();
    }

    /// <summary>
    /// 현재 상태값으로 화살표 ON/OFF
    /// </summary>
    private void ArrowSetActive()
    {
        upArrow.SetActive(!isUpArrowOff);
        downArrow.SetActive(!isDownArrowOff);
    }

    /// <summary>
    /// 이벤트 아이콘 추가
    /// </summary>
    public void AddIcon(Icon icon)
    {
        if (icon.GameObject.transform.parent != grid.transform)
        {
            icon.GameObject.transform.SetParent(grid.transform, false);
        }

        icons.Add(icon);
        SetScroll_Event();
    }
    
    /// <summary>
    /// 특정 아이콘 삭제
    /// </summary>
    public void DestroyIcon(Icon icon)
    {
        DestroyImmediate(icon.GameObject);
        icons.Remove(icon);

        SetScroll_Event();
        SetArrowCoroutine();
    }

    /// <summary>
    /// 아이콘 전체 삭제
    /// </summary>
    public void DestroyAllIcon()
    {
        foreach (var icon in icons)
        {
            Destroy(icon.GameObject);
        }

        icons.Clear();

        Vector3 pos = root.transform.localPosition;
        root.transform.localPosition = pos;
        root.gameObject.SetActive(false);
    }
    
    public void SetDepth(int _depth = -30)
    {
        if (scrollView.gameObject.activeInHierarchy && scrollView.panel != null)
            scrollView.panel.depth = _depth;
    }

    /// <summary>
    /// 상하 화살표 활성화 / 비활성화를 위한 코루틴
    /// </summary>
    private IEnumerator CoCheckArrowActive(float viewCount)
    {
        // maxPosition (하단 화살표가 보이지 않는 위치 (스크롤 맨 아래)) : 아이콘 높이 X (전체 아이콘 개수 - 화면에서 보이는 아이콘 개수)
        float maxPosition = iconCount < viewCount ? 0 : grid.cellHeight * (iconCount - viewCount);
        
        //newIcon on/off
        float lastNewIconHeight    = NEW_ICON_HEIGHT + SCROLL_VIEW_HEIGHT;
        float newIconCheckPosition = maxPosition     - NEW_ICON_HEIGHT;
        bool  isUpArrowDisable     = false;
        bool  isDownArrowDisable   = false;
        
        var bubbleHeight        = BUBBLE_HEIGHT + SCROLL_VIEW_HEIGHT;
        var bubbleCheckPosition = maxPosition   - BUBBLE_HEIGHT;
        var isActiveNewIcon     = false;
        
        while (true)
        {
            bool isChange = false;
            if (scrollView.transform.localPosition.y <= 5)
            {
                isUpArrowDisable = true;
            }
            else if (scrollView.transform.localPosition.y >= maxPosition)
            {
                isDownArrowDisable = true;
            }
            else
            {
                isUpArrowDisable = false;
                isDownArrowDisable = false;
            }
            if (isUpArrowDisable != isUpArrowOff || isDownArrowDisable != isDownArrowOff)
                isChange = true;

            if (isChange)
            {
                isUpArrowOff = isUpArrowDisable;
                isDownArrowOff = isDownArrowDisable;
                ArrowSetActive();
            }

            //스크롤 뷰 아래에 newIcon 높이만큼의 공간이 남아있지 않다면 검사 x / 가려지는 경계에 걸친 아이콘 인덱스를 구한 후 해당 아이콘 이후 newIcon중에 켜진게 있는지 찾습니다.
            if (scrollView.transform.localPosition.y < newIconCheckPosition)
            {
                int iconIndex = (int)((lastNewIconHeight + scrollView.transform.localPosition.y) / ICON_HEIGHT - tempicons.Count);

                isActiveNewIcon = false;
                newIconBubble.SetActive(false);
                
                for (int i = iconIndex; i < icons.Count; i++)
                {
                    if (icons[i].IsActiveNewIcon())
                    {
                        newIconBubble.SetActive(true);
                        isActiveNewIcon = true;
                        break;
                    }
                }
            }
            else
            {
                newIconBubble.SetActive(false);
            }
            
            if (isActiveNewIcon == false)
            {
                if (scrollView.transform.localPosition.y < bubbleCheckPosition)
                {
                    var activeBubble = false;
                    var iconIndex    = (int)((bubbleHeight + scrollView.transform.localPosition.y) / ICON_HEIGHT - tempicons.Count);

                    for (var i = iconIndex; i < icons.Count; i++)
                    {
                        if (icons[i].IsActiveBubble())
                        {
                            activeBubble = true;
                            bubble.CopyBubble = icons[i].GetBubble().CopyBubble;
                            break;
                        }
                    }

                    if (activeBubble != bubble.gameObject.activeSelf)
                    {
                        bubble.gameObject.SetActive(activeBubble);
                    }
                }
                else
                {
                    bubble.gameObject.SetActive(false);
                }
            }
            else
            {
                bubble.gameObject.SetActive(false);
            }
            
            yield return null;
        }
    }

    //아이콘이 마스크 영역 밖에 있는지 확인
    public bool CheckIconPosition(Icon icon)
    {
        int iconIndex = (int)((NEW_ICON_HEIGHT + SCROLL_VIEW_HEIGHT + scrollView.transform.localPosition.y) / ICON_HEIGHT - tempicons.Count);

        int idx = icons.FindIndex(x => x == icon);
        if (idx < iconIndex)
            return false;
        
        return true;
    }

    public void AddTempIcon(GameObject tempIcon)
    {
        tempIcon.transform.SetAsFirstSibling();
        tempicons.Add(tempIcon);
        SetScroll_Event();
        SetArrowCoroutine();
    }
    
    public void PostRemoveTempIcon()
    {
        SetScroll_Event();
        SetArrowCoroutine();
    }
    
    private void IconNewUpdate()
    {
        List<string> keys = new List<string>();
        
        foreach(var icon in icons)
            keys.Add(icon.UniqueKey);
        AddEventGuideIcon();
    }
    
    public void AddEventGuideIcon()
    {
        if (GameData.User.missionCnt <= ManagerLobby._missionThreshold_eventstageOpen_noticeOpen_packageshopOpen)
        {
            SideIcon.Maker.MakeIcon<SideIcon.IconGuideEvent>(
                scrollBar: ManagerUI._instance.ScrollbarRight,
                init: (icon) => icon.Init(ServerRepos.LoginCdn));
        }
    }
}